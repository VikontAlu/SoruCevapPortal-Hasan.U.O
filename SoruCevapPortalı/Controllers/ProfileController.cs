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
            // Eğer id boşsa, giriş yapan kullanıcının kendi profilini getir
            var userId = string.IsNullOrEmpty(id) ? User.FindFirstValue(ClaimTypes.NameIdentifier) : id;
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null) return NotFound("Kullanıcı bulunamadı.");

            // Kullanıcının verilerini çekelim
            var questions = await _unitOfWork.Questions.GetAllAsync(q => q.ApplicationUserId == user.Id, "Category");
            var answers = await _unitOfWork.Answers.GetAllAsync(a => a.ApplicationUserId == user.Id, "Question");

            // İstatistikler
            ViewBag.QuestionCount = questions.Count();
            ViewBag.AnswerCount = answers.Count();

            // Toplam Görüntülenme (Sorularından)
            ViewBag.TotalViews = questions.Sum(q => q.ViewCount);

            // Toplam Oy Puanı (Basit bir hesaplama: Soru Oyları + Cevap Oyları)
            // Not: Gerçek bir Reputation sistemi için daha detaylı sorgu gerekir ama şimdilik bu yeterli.
            ViewBag.Reputation = questions.Sum(q => q.VoteCount) + answers.Sum(a => a.VoteCount);

            // Listeleri View'a gönder (En yeni en üstte)
            ViewBag.UserQuestions = questions.OrderByDescending(q => q.CreatedDate).ToList();
            ViewBag.UserAnswers = answers.OrderByDescending(a => a.CreatedDate).ToList();

            // Kendi profili mi?
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