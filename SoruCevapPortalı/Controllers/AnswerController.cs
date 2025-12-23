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

        // -------------------- CEVAP EKLE --------------------
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

        // -------------------- CEVABI KABUL ET (+PUAN EKLENDİ) --------------------
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AcceptAnswer(int id)
        {
            var answer = _context.Answers
                .Include(a => a.Question)
                .Include(a => a.ApplicationUser)          // ✅ Cevabı veren kullanıcıyı çek
                .Include(a => a.Question.ApplicationUser) // ✅ Soruyu soran kullanıcıyı çek
                .FirstOrDefault(a => a.Id == id);

            if (answer == null)
                return Json(new { success = false, message = "Cevap bulunamadı!" });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Sadece soruyu soran kişi kabul edebilir
            if (answer.Question.ApplicationUserId != userId)
                return Json(new { success = false, message = "Bu işlem için yetkiniz yok!" });

            // Aynı soruya ait tüm cevapların kabulünü kaldır
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

            // ✅ PUAN DAĞITIMI
            // 1. Cevabı yazana 15 puan ver
            if (answer.ApplicationUser != null)
                answer.ApplicationUser.Reputation += 15;

            // 2. Soruyu sorana (kabul ettiği için) 2 puan ver
            if (answer.Question.ApplicationUser != null)
                answer.Question.ApplicationUser.Reputation += 2;

            _context.SaveChanges();

            return Json(new { success = true, message = "Cevap kabul edildi!" });
        }

        // -------------------- OY VER (+PUAN EKLENDİ) --------------------
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Vote(int answerId, bool isUpvote)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var answer = _context.Answers
                .Include(a => a.Votes)
                .Include(a => a.ApplicationUser) // ✅ Kullanıcı puanı için gerekli
                .FirstOrDefault(a => a.Id == answerId);

            if (answer == null)
                return Json(new { success = false, message = "Cevap bulunamadı!" });

            var existingVote = _context.AnswerVotes
                .FirstOrDefault(v => v.AnswerId == answerId && v.ApplicationUserId == userId);

            int reputationChange = 0; // Puan değişimi değişkeni

            if (existingVote != null)
            {
                // Eğer kullanıcı zaten aynı yönde oy verdiyse (Örn: Like atmış, tekrar Like'a bastı)
                // O zaman oyu geri çekiyoruz (Siliyoruz)
                if (existingVote.IsUpVote == isUpvote)
                {
                    _context.AnswerVotes.Remove(existingVote);
                    // Oyu geri çekince verilen puanı geri al
                    reputationChange = isUpvote ? -10 : 2;
                }
                else
                {
                    // Farklı yönde bastıysa (Like -> Dislike), güncelliyoruz
                    existingVote.IsUpVote = isUpvote;
                    // Puan farkını yansıt (Örn: -2'den +10'a çıkış için +12 gerekir)
                    reputationChange = isUpvote ? 12 : -12;
                }
            }
            else
            {
                // Hiç oyu yoksa yeni ekliyoruz
                _context.AnswerVotes.Add(new AnswerVote
                {
                    AnswerId = answerId,
                    ApplicationUserId = userId,
                    IsUpVote = isUpvote
                });

                // Yeni oy puanı: Like +10, Dislike -2
                reputationChange = isUpvote ? 10 : -2;
            }

            // ✅ PUANI KULLANICIYA İŞLE
            // Kendi cevabına oy veremezsin kontrolü burada da yapılabilir, 
            // ama kendi kendine puan kazandırmaması için ID kontrolü yapıyoruz.
            if (answer.ApplicationUser != null && answer.ApplicationUser.Id != userId)
            {
                answer.ApplicationUser.Reputation += reputationChange;
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
                userVote = existingVote != null && existingVote.IsUpVote == isUpvote ? "none" : (isUpvote ? "up" : "down")
            });
        }
    }
}