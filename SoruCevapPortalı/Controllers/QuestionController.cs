using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SoruCevapPortalı.Models;
using SoruCevapPortalı.Interfaces;
using System.Security.Claims;

namespace SoruCevapPortalı.Controllers
{
    public class QuestionController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public QuestionController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ================= INDEX =================
        public async Task<IActionResult> Index()
        {
            // İlişkili verilerle birlikte tüm soruları çekiyoruz
            var questions = await _unitOfWork.Questions.GetAllAsync(null, "Category,ApplicationUser,Answers");

            // Tarihe göre sıralayıp View'a gönderiyoruz
            return View(questions.OrderByDescending(q => q.CreatedDate).ToList());
        }

        // ================= DETAILS =================
        public async Task<IActionResult> Details(int id)
        {
            // Soru detaylarını, cevapları, cevaplayan kullanıcıları ve oyları getiriyoruz
            var question = await _unitOfWork.Questions.GetAsync(
                q => q.Id == id,
                "Category,ApplicationUser,Answers,Answers.ApplicationUser,Votes");

            if (question == null)
                return NotFound();

            // Görüntülenme sayısını artır
            question.ViewCount++;
            _unitOfWork.Questions.Update(question);
            await _unitOfWork.CompleteAsync();

            return View(question);
        }

        // ================= ASK (GET) =================
        [Authorize]
        public async Task<IActionResult> Ask()
        {
            ViewBag.Categories = await _unitOfWork.Categories.GetAllAsync();
            return View();
        }

        // ================= ASK (POST) =================
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ask(Question question)
        {
            // Eğer Model (Models/Question.cs) içinde [ValidateNever] eklemediysen
            // burası sürekli False döner ve kayıt yapmaz.
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _unitOfWork.Categories.GetAllAsync();
                return View(question);
            }

            question.ApplicationUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            question.CreatedDate = DateTime.Now;
            question.ViewCount = 0;
            question.VoteCount = 0;
            question.IsSolved = false;

            await _unitOfWork.Questions.AddAsync(question);
            await _unitOfWork.CompleteAsync();

            return RedirectToAction(nameof(Details), new { id = question.Id });
        }

        // ================= AJAX GET VOTES (EKSİKTİ, EKLENDİ) =================
        [HttpGet]
        public async Task<JsonResult> GetVotes(int id)
        {
            // Oyları çek
            var votes = await _unitOfWork.QuestionVotes.GetAllAsync(v => v.QuestionId == id);

            var up = votes.Count(v => v.IsUpVote);
            var down = votes.Count(v => !v.IsUpVote);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string userVote = "none";

            if (userId != null)
            {
                var vote = votes.FirstOrDefault(v => v.ApplicationUserId == userId);
                if (vote != null)
                    userVote = vote.IsUpVote ? "up" : "down";
            }

            return Json(new
            {
                success = true,
                upvotes = up,
                downvotes = down,
                totalScore = up - down,
                userVote
            });
        }

        // ================= AJAX VOTE (OY VERME) =================
        [HttpPost]
        [Authorize]
        public async Task<JsonResult> Vote(int questionId, bool isUpVote)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Json(new { success = false, message = "Giriş yapmalısınız." });

            var question = await _unitOfWork.Questions.GetAsync(q => q.Id == questionId);
            if (question == null)
                return Json(new { success = false, message = "Soru bulunamadı." });

            var existingVote = await _unitOfWork.QuestionVotes.GetAsync(v => v.QuestionId == questionId && v.ApplicationUserId == userId);

            if (existingVote != null)
            {
                if (existingVote.IsUpVote == isUpVote)
                    _unitOfWork.QuestionVotes.Remove(existingVote); // Aynı oya tekrar basarsa oyu geri al
                else
                {
                    existingVote.IsUpVote = isUpVote; // Oyu değiştir (Up -> Down veya tam tersi)
                    _unitOfWork.QuestionVotes.Update(existingVote);
                }
            }
            else
            {
                await _unitOfWork.QuestionVotes.AddAsync(new QuestionVote
                {
                    QuestionId = questionId,
                    ApplicationUserId = userId,
                    IsUpVote = isUpVote
                });
            }

            await _unitOfWork.CompleteAsync();

            // Güncel sayıları hesapla ve geri dön
            var allVotes = await _unitOfWork.QuestionVotes.GetAllAsync(v => v.QuestionId == questionId);
            var up = allVotes.Count(v => v.IsUpVote);
            var down = allVotes.Count(v => !v.IsUpVote);

            return Json(new { success = true, totalScore = up - down });
        }

        // ================= BY CATEGORY (EKSİKTİ, EKLENDİ) =================
        public async Task<IActionResult> ByCategory(int id)
        {
            // Kategoriyi ve içindeki soruları çekiyoruz
            var category = await _unitOfWork.Categories.GetAsync(c => c.Id == id, "Questions,Questions.ApplicationUser");

            if (category == null)
                return NotFound();

            ViewBag.CategoryName = category.Name;
            return View(category.Questions.ToList());
        }
    }
}