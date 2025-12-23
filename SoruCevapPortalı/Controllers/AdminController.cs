using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity; // UserManager için
using Microsoft.EntityFrameworkCore;   // ✅ Include ve ToListAsync için
using SoruCevapPortalı.Interfaces;
using SoruCevapPortalı.Models;
using SoruCevapPortalı.Data;           // ✅ DbContext için
using System.Security.Claims;

namespace SoruCevapPortalı.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context; // ✅ Raporlar için

        public AdminController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _context = context;
        }

        // -------------------- DASHBOARD --------------------
        public async Task<IActionResult> Index()
        {
            var questions = await _unitOfWork.Questions.GetAllAsync();
            var categories = await _unitOfWork.Categories.GetAllAsync();
            var answers = await _unitOfWork.Answers.GetAllAsync();

            ViewBag.TotalQuestions = questions.Count();
            ViewBag.TotalCategories = categories.Count();
            ViewBag.TotalAnswers = answers.Count();
            ViewBag.TotalUsers = _userManager.Users.Count();

            // Bekleyen Rapor Sayısı
            ViewBag.PendingReports = await _context.Reports.CountAsync();

            return View();
        }

        // -------------------- KATEGORİ --------------------
        public async Task<IActionResult> Categories()
        {
            var categories = await _unitOfWork.Categories.GetAllAsync();
            return View(categories);
        }

        public IActionResult CreateCategory() => View();

        [HttpPost]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            if (!ModelState.IsValid) return View(category);

            category.CreatedDate = DateTime.Now;
            await _unitOfWork.Categories.AddAsync(category);
            await _unitOfWork.CompleteAsync();

            return RedirectToAction(nameof(Categories));
        }

        public async Task<IActionResult> EditCategory(int id)
        {
            var category = await _unitOfWork.Categories.GetAsync(c => c.Id == id);
            return category == null ? NotFound() : View(category);
        }

        [HttpPost]
        public async Task<IActionResult> EditCategory(Category category)
        {
            if (!ModelState.IsValid) return View(category);

            _unitOfWork.Categories.Update(category);
            await _unitOfWork.CompleteAsync();

            return RedirectToAction(nameof(Categories));
        }

        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _unitOfWork.Categories.GetAsync(c => c.Id == id);
            if (category == null) return NotFound();

            _unitOfWork.Categories.Remove(category);
            await _unitOfWork.CompleteAsync();

            return RedirectToAction(nameof(Categories));
        }

        // -------------------- SORULAR (ARAMA EKLENDİ) --------------------
        public async Task<IActionResult> Questions(string search)
        {
            // Tüm soruları ilişkili verilerle çek
            var questions = await _unitOfWork.Questions.GetAllAsync(null, "Category,ApplicationUser,Answers");

            // Arama filtresi uygula
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                // Not: Repository yapısında GetAllAsync IEnumerable döndüğü için filtreleme bellekte yapılır.
                questions = questions.Where(q => q.Title.ToLower().Contains(search) ||
                                                 q.Content.ToLower().Contains(search));
            }

            ViewBag.Search = search;
            return View(questions);
        }

        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var question = await _unitOfWork.Questions.GetAsync(q => q.Id == id);
            if (question == null) return NotFound();

            _unitOfWork.Questions.Remove(question);
            await _unitOfWork.CompleteAsync();

            return RedirectToAction(nameof(Questions));
        }

        // -------------------- KULLANICI DETAY --------------------
        public async Task<IActionResult> UserDetail(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var userQuestions = await _unitOfWork.Questions.GetAllAsync(q => q.ApplicationUserId == id);
            var userAnswers = await _unitOfWork.Answers.GetAllAsync(a => a.ApplicationUserId == id);

            ViewBag.TotalQuestions = userQuestions.Count();
            ViewBag.TotalAnswers = userAnswers.Count();
            ViewBag.MemberSince = user.RegistrationDate.Year;

            return View(user);
        }

        // -------------------- KULLANICILAR (ARAMA EKLENDİ) --------------------
        public IActionResult UserManagement(string search)
        {
            var users = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                users = users.Where(u => u.UserName.ToLower().Contains(search) ||
                                         u.Email.ToLower().Contains(search));
            }

            ViewBag.Search = search;
            return View(users.ToList());
        }

        public async Task<IActionResult> DeleteUser(string id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == id)
            {
                TempData["ErrorMessage"] = "Kendinizi silemezsiniz!";
                return RedirectToAction(nameof(UserManagement));
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // İlişkili verileri temizle
            var userQuestions = await _unitOfWork.Questions.GetAllAsync(q => q.ApplicationUserId == id);
            var userAnswers = await _unitOfWork.Answers.GetAllAsync(a => a.ApplicationUserId == id);

            _unitOfWork.Questions.RemoveRange(userQuestions);
            _unitOfWork.Answers.RemoveRange(userAnswers);
            await _unitOfWork.CompleteAsync();

            await _userManager.DeleteAsync(user);

            return RedirectToAction(nameof(UserManagement));
        }

        // ================= RAPORLAR SAYFASI (ARAMA EKLENDİ) =================
        public async Task<IActionResult> Reports(string search)
        {
            var reportsQuery = _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.Question)
                .Include(r => r.Answer)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                reportsQuery = reportsQuery.Where(r =>
                    r.Reason.ToLower().Contains(search) ||
                    r.Reporter.UserName.ToLower().Contains(search));
            }

            ViewBag.Search = search;

            // Tarihe göre tersten sıralayıp gönder
            return View(await reportsQuery.OrderByDescending(r => r.CreatedDate).ToListAsync());
        }

        // Raporu Silme / Tamamlandı İşaretleme
        [HttpPost]
        public async Task<IActionResult> DeleteReport(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report != null)
            {
                _context.Reports.Remove(report);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Reports));
        }
    }
}