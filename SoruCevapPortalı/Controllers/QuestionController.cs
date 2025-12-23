using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SoruCevapPortalı.Models;
using SoruCevapPortalı.Interfaces; // UnitOfWork için
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

        public async Task<IActionResult> Index()
        {
            // Repository deseni ile verileri çekiyoruz
            var questions = await _unitOfWork.Questions.GetAllAsync(null, "Category,ApplicationUser,Answers");

            // ❗ DÜZELTME BURADA: .ToList() eklendi
            return View(questions.OrderByDescending(q => q.CreatedDate).ToList());
        }

        // ================= DETAILS =================
        public async Task<IActionResult> Details(int id)
        {
            // ThenInclude (Answers.ApplicationUser) string olarak şöyle yazılır: "Answers.ApplicationUser"
            var question = await _unitOfWork.Questions.GetAsync(
                q => q.Id == id,
                "Category,ApplicationUser,Answers,Answers.ApplicationUser,Votes");

            if (question == null)
                return NotFound();

            question.ViewCount++;
            _unitOfWork.Questions.Update(question); // Update metodu void olduğu için await yok
            await _unitOfWork.CompleteAsync(); // Kaydet

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

        // ================= AJAX VOTE =================
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
                    _unitOfWork.QuestionVotes.Remove(existingVote);
                else
                {
                    existingVote.IsUpVote = isUpVote;
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

            // Güncel sayıları hesapla
            var allVotes = await _unitOfWork.QuestionVotes.GetAllAsync(v => v.QuestionId == questionId);
            var up = allVotes.Count(v => v.IsUpVote);
            var down = allVotes.Count(v => !v.IsUpVote);

            return Json(new { success = true, totalScore = up - down });
        }
    }
}