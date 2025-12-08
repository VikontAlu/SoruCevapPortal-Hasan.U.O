using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using SoruCevapPortalı.Data;
using SoruCevapPortalı.Models;
using System.Security.Claims;

namespace SoruCevapPortalı.Controllers
{
    public class QuestionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuestionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= INDEX =================
        public IActionResult Index()
        {
            var questions = _context.Questions
                .Include(q => q.Category)
                .Include(q => q.ApplicationUser)
                .Include(q => q.Answers)
                .OrderByDescending(q => q.CreatedDate)
                .ToList();

            return View(questions);
        }

        // ================= DETAILS =================
        public IActionResult Details(int id)
        {
            var question = _context.Questions
                .Include(q => q.Category)
                .Include(q => q.ApplicationUser)
                .Include(q => q.Answers)
                    .ThenInclude(a => a.ApplicationUser)
                .Include(q => q.Votes)
                .FirstOrDefault(q => q.Id == id);

            if (question == null)
                return NotFound();

            question.ViewCount++;
            _context.SaveChanges();

            return View(question);
        }

        // ================= ASK (GET) =================
        [Authorize]
        public IActionResult Ask()
        {
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }

        // ================= ASK (POST) =================
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult Ask(Question question)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _context.Categories.ToList();
                return View(question);
            }

            question.ApplicationUserId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            question.CreatedDate = DateTime.Now;
            question.ViewCount = 0;
            question.VoteCount = 0;
            question.IsSolved = false;

            _context.Questions.Add(question);
            _context.SaveChanges();

            return RedirectToAction(nameof(Details), new { id = question.Id });
        }

        // ================= AJAX GET VOTES =================
        [HttpGet]
        public JsonResult GetVotes(int id)
        {
            var up = _context.QuestionVotes
                .Count(v => v.QuestionId == id && v.IsUpVote);

            var down = _context.QuestionVotes
                .Count(v => v.QuestionId == id && !v.IsUpVote);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string userVote = "none";

            if (userId != null)
            {
                var vote = _context.QuestionVotes
                    .FirstOrDefault(v =>
                        v.QuestionId == id &&
                        v.ApplicationUserId == userId
                    );

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

        // ================= AJAX VOTE =================
        [HttpPost]
        [Authorize]
        public JsonResult Vote(int questionId, bool isUpVote)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Json(new { success = false, message = "Giriş yapmalısınız." });

            var question = _context.Questions
                .FirstOrDefault(q => q.Id == questionId);

            if (question == null)
                return Json(new { success = false, message = "Soru bulunamadı." });

            var existingVote = _context.QuestionVotes
                .FirstOrDefault(v =>
                    v.QuestionId == questionId &&
                    v.ApplicationUserId == userId
                );

            if (existingVote != null)
            {
                if (existingVote.IsUpVote == isUpVote)
                    _context.QuestionVotes.Remove(existingVote);
                else
                    existingVote.IsUpVote = isUpVote;
            }
            else
            {
                _context.QuestionVotes.Add(new QuestionVote
                {
                    QuestionId = questionId,
                    ApplicationUserId = userId,
                    IsUpVote = isUpVote
                });
            }

            _context.SaveChanges();

            var up = _context.QuestionVotes
                .Count(v => v.QuestionId == questionId && v.IsUpVote);

            var down = _context.QuestionVotes
                .Count(v => v.QuestionId == questionId && !v.IsUpVote);

            return Json(new
            {
                success = true,
                totalScore = up - down
            });
        }

        // ================= BY CATEGORY =================
        public IActionResult ByCategory(int id)
        {
            var category = _context.Categories
                .Include(c => c.Questions)
                    .ThenInclude(q => q.ApplicationUser)
                .FirstOrDefault(c => c.Id == id);

            if (category == null)
                return NotFound();

            ViewBag.CategoryName = category.Name;
            return View(category.Questions.ToList());
        }
    }
}
