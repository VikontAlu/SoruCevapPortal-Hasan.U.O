using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SoruCevapPortalı.Interfaces;
using SoruCevapPortalı.Models;
using System.Security.Claims;

namespace SoruCevapPortalı.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public ProfileController(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index(string id)
        {
            var userId = string.IsNullOrEmpty(id) ? User.FindFirstValue(ClaimTypes.NameIdentifier) : id;
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null) return NotFound("Kullanıcı bulunamadı.");

            var questions = await _unitOfWork.Questions.GetAllAsync(q => q.ApplicationUserId == user.Id, "Category");
            var answers = await _unitOfWork.Answers.GetAllAsync(a => a.ApplicationUserId == user.Id, "Question");

            ViewBag.QuestionCount = questions.Count();
            ViewBag.AnswerCount = answers.Count();
            ViewBag.TotalViews = questions.Sum(q => q.ViewCount);

            // 👇 DEĞİŞİKLİK BURADA: Artık hesaplama yapmıyoruz, kayıtlı puanı çekiyoruz.
            ViewBag.Reputation = user.Reputation;

            ViewBag.UserQuestions = questions.OrderByDescending(q => q.CreatedDate).ToList();
            ViewBag.UserAnswers = answers.OrderByDescending(a => a.CreatedDate).ToList();
            ViewBag.IsMyProfile = (User.FindFirstValue(ClaimTypes.NameIdentifier) == user.Id);

            return View(user);
        }
        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            return View(user);
        }

        // ================= AYARLAR (POST) =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(ApplicationUser model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            // Sadece izin verilen alanları güncelle
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.JobTitle = model.JobTitle;
            user.AboutMe = model.AboutMe;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Profiliniz başarıyla güncellendi!";
                return RedirectToAction(nameof(Index)); // Profil sayfasına dön
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }
    }
}