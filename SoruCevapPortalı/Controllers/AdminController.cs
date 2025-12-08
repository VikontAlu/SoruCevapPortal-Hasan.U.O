using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoruCevapPortalı.Data;
using SoruCevapPortalı.Models;

namespace SoruCevapPortalı.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // -------------------- DASHBOARD --------------------
        public IActionResult Index()
        {
            ViewBag.TotalQuestions = _context.Questions.Count();
            ViewBag.TotalCategories = _context.Categories.Count();
            ViewBag.TotalAnswers = _context.Answers.Count();
            ViewBag.TotalUsers = _context.Users.Count();
            return View();
        }

        // -------------------- DB TEST --------------------
        public IActionResult TestDatabase()
        {
            try
            {
                return Content(
                    $"✅ DB OK | Kategoriler: {_context.Categories.Count()}, " +
                    $"Sorular: {_context.Questions.Count()}, " +
                    $"Cevaplar: {_context.Answers.Count()}, " +
                    $"Kullanıcılar: {_context.Users.Count()}"
                );
            }
            catch (Exception ex)
            {
                return Content($"❌ DB Hatası: {ex.Message}");
            }
        }

        // -------------------- KATEGORİ --------------------
        public IActionResult Categories()
        {
            return View(_context.Categories.ToList());
        }

        public IActionResult CreateCategory() => View();

        [HttpPost]
        public IActionResult CreateCategory(Category category)
        {
            if (!ModelState.IsValid)
                return View(category);

            category.CreatedDate = DateTime.Now;
            _context.Categories.Add(category);
            _context.SaveChanges();

            return RedirectToAction(nameof(Categories));
        }

        public IActionResult EditCategory(int id)
        {
            var category = _context.Categories.Find(id);
            return category == null ? NotFound() : View(category);
        }

        [HttpPost]
        public IActionResult EditCategory(Category category)
        {
            if (!ModelState.IsValid)
                return View(category);

            _context.Categories.Update(category);
            _context.SaveChanges();

            return RedirectToAction(nameof(Categories));
        }

        public IActionResult DeleteCategory(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
                return NotFound();

            _context.Categories.Remove(category);
            _context.SaveChanges();

            return RedirectToAction(nameof(Categories));
        }

        // -------------------- SORULAR --------------------
        public IActionResult Questions()
        {
            var questions = _context.Questions
                .Include(q => q.Category)
                .Include(q => q.ApplicationUser)
                .Include(q => q.Answers)
                .ToList();

            return View(questions);
        }

        public IActionResult QuestionDetail(int id)
        {
            var question = _context.Questions
                .Include(q => q.Category)
                .Include(q => q.ApplicationUser)
                .Include(q => q.Answers)
                    .ThenInclude(a => a.ApplicationUser)
                .FirstOrDefault(q => q.Id == id);

            return question == null ? NotFound() : View(question);
        }

        public IActionResult DeleteQuestion(int id)
        {
            var question = _context.Questions.Find(id);
            if (question == null)
                return NotFound();

            _context.Questions.Remove(question);
            _context.SaveChanges();

            return RedirectToAction(nameof(Questions));
        }

        // -------------------- KULLANICILAR --------------------
        public IActionResult UserManagement()
        {
            return View(_context.Users.ToList());
        }

        public IActionResult UserDetail(string id)
        {
            var user = _context.Users
                .Include(u => u.Questions)
                .Include(u => u.Answers)
                .FirstOrDefault(u => u.Id == id);

            if (user == null)
                return NotFound();

            ViewBag.TotalQuestions = user.Questions.Count;
            ViewBag.TotalAnswers = user.Answers.Count;

            return View(user);
        }

        public IActionResult DeleteUser(string id)
        {
            var user = _context.Users
                .Include(u => u.Questions)
                .Include(u => u.Answers)
                .FirstOrDefault(u => u.Id == id);

            if (user == null)
                return NotFound();

            var userQuestions = _context.Questions
                .Where(q => q.ApplicationUserId == id);

            var userAnswers = _context.Answers
                .Where(a => a.ApplicationUserId == id);

            _context.Questions.RemoveRange(userQuestions);
            _context.Answers.RemoveRange(userAnswers);
            _context.Users.Remove(user);

            _context.SaveChanges();

            return RedirectToAction(nameof(UserManagement));
        }
    }
}
