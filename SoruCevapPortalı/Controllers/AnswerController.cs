using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoruCevapPortalı.Data;
using SoruCevapPortalı.Models;
using System.Security.Claims;

namespace SoruCevapPortalı.Controllers
{
    public class AnswerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AnswerController(ApplicationDbContext context)
        {
            _context = context;
        }

        
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult Add(int questionId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Cevap boş olamaz!";
                return RedirectToAction("Details", "Question", new { id = questionId });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var answer = new Answer
            {
                QuestionId = questionId,
                Content = content,
                CreatedDate = DateTime.Now,
                ApplicationUserId = userId,
                VoteCount = 0,
                IsAccepted = false,
                IsBestAnswer = false
            };

            _context.Answers.Add(answer);
            _context.SaveChanges();

            return RedirectToAction("Details", "Question", new { id = questionId });
        }

       
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AcceptAnswer(int id)
        {
            var answer = _context.Answers
                .Include(a => a.Question)
                .FirstOrDefault(a => a.Id == id);

            if (answer == null)
                return Json(new { success = false, message = "Cevap bulunamadı!" });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            
            if (answer.Question.ApplicationUserId != userId)
                return Json(new { success = false, message = "Bu işlem için yetkiniz yok!" });

            // Aynı soruya ait tüm cevapları sıfırla
            var answers = _context.Answers
                .Where(a => a.QuestionId == answer.QuestionId)
                .ToList();

            foreach (var a in answers)
            {
                a.IsAccepted = false;
                a.IsBestAnswer = false;
            }

            // Seçilen cevabı kabul et
            answer.IsAccepted = true;
            answer.IsBestAnswer = true;

            // Soruyu çözüldü olarak işaretle
            answer.Question.IsSolved = true;

            _context.SaveChanges();

            return Json(new { success = true, message = "Cevap kabul edildi!" });
        }

        // -------------------- OY VER (AJAX) --------------------
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Vote(int answerId, bool isUpvote)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var answer = _context.Answers
                .Include(a => a.Votes)
                .FirstOrDefault(a => a.Id == answerId);

            if (answer == null)
                return Json(new { success = false, message = "Cevap bulunamadı!" });

            var existingVote = _context.AnswerVotes
                .FirstOrDefault(v => v.AnswerId == answerId && v.ApplicationUserId == userId);

            if (existingVote != null)
            {
                existingVote.IsUpVote = isUpvote;
            }
            else
            {
                _context.AnswerVotes.Add(new AnswerVote
                {
                    AnswerId = answerId,
                    ApplicationUserId = userId,
                    IsUpVote = isUpvote
                });
            }

            _context.SaveChanges();

            // Oyları tekrar hesapla
            var up = _context.AnswerVotes.Count(v => v.AnswerId == answerId && v.IsUpVote);
            var down = _context.AnswerVotes.Count(v => v.AnswerId == answerId && !v.IsUpVote);

            answer.VoteCount = up - down;
            _context.SaveChanges();

            return Json(new
            {
                success = true,
                upvotes = up,
                downvotes = down,
                totalScore = up - down,
                userVote = isUpvote ? "up" : "down"
            });
        }
    }
}
