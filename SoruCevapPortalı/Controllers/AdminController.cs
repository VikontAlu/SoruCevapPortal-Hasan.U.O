using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity; // UserManager için
using SoruCevapPortalı.Interfaces;
using SoruCevapPortalı.Models;
using System.Security.Claims;

namespace SoruCevapPortalı.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        // -------------------- DASHBOARD --------------------
        public async Task<IActionResult> Index()
        {
            // Count işlemleri için GetAll yapıp Count almak performanssızdır ama
            // generic repository'de CountAsync yoksa şimdilik idare eder.
            // Ödev olduğu için sorun olmaz.
            var questions = await _unitOfWork.Questions.GetAllAsync();
            var categories = await _unitOfWork.Categories.GetAllAsync();
            var answers = await _unitOfWork.Answers.GetAllAsync();

            ViewBag.TotalQuestions = questions.Count();
            ViewBag.TotalCategories = categories.Count();
            ViewBag.TotalAnswers = answers.Count();
            ViewBag.TotalUsers = _userManager.Users.Count(); // UserManager'dan çekiyoruz

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

        // -------------------- SORULAR --------------------
        public async Task<IActionResult> Questions()
        {
            var questions = await _unitOfWork.Questions.GetAllAsync(null, "Category,ApplicationUser,Answers");
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

        // -------------------- KULLANICI DETAY (EKSİK OLAN KISIM) --------------------
        public async Task<IActionResult> UserDetail(string id)
        {
            // Kullanıcıyı bul
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // İstatistikler için UnitOfWork kullanıyoruz
            var userQuestions = await _unitOfWork.Questions.GetAllAsync(q => q.ApplicationUserId == id);
            var userAnswers = await _unitOfWork.Answers.GetAllAsync(a => a.ApplicationUserId == id);

            // ViewBag ile view tarafına veri taşıyoruz
            ViewBag.TotalQuestions = userQuestions.Count();
            ViewBag.TotalAnswers = userAnswers.Count();
            ViewBag.MemberSince = user.RegistrationDate.Year; // Kayıt yılı

            return View(user);
        }

        // -------------------- KULLANICILAR (UserManager Kullanıyoruz) --------------------
        public IActionResult UserManagement()
        {
            // Kullanıcıları listeleme
            return View(_userManager.Users.ToList());
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

            // Kullanıcıya ait soruları ve cevapları silmemiz lazım
            // (Cascade Delete yoksa manuel sileriz, Repository ile)
            var userQuestions = await _unitOfWork.Questions.GetAllAsync(q => q.ApplicationUserId == id);
            var userAnswers = await _unitOfWork.Answers.GetAllAsync(a => a.ApplicationUserId == id);

            _unitOfWork.Questions.RemoveRange(userQuestions);
            _unitOfWork.Answers.RemoveRange(userAnswers);
            await _unitOfWork.CompleteAsync();

            // Identity üzerinden kullanıcıyı sil
            await _userManager.DeleteAsync(user);

            return RedirectToAction(nameof(UserManagement));
        }
    }
}