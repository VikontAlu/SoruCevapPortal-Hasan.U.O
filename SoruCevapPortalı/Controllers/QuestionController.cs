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
        public async Task<IActionResult> Index(string search, string sortOrder)
        {
            var questions = await _unitOfWork.Questions.GetAllAsync(null, "Category,ApplicationUser,Answers");
            var query = questions.AsQueryable();

            // ARAMA
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(q => q.Title.ToLower().Contains(search) ||
                                         q.Content.ToLower().Contains(search));
                ViewBag.Search = search;
            }

            // SIRALAMA
            ViewBag.SortOrder = sortOrder;

            switch (sortOrder)
            {
                case "date_asc": query = query.OrderBy(q => q.CreatedDate); break;
                case "title_asc": query = query.OrderBy(q => q.Title); break;
                case "title_desc": query = query.OrderByDescending(q => q.Title); break;

                case "view_desc": query = query.OrderByDescending(q => q.ViewCount); break;
                case "view_asc": query = query.OrderBy(q => q.ViewCount); break;

                case "vote_desc": query = query.OrderByDescending(q => q.VoteCount); break;
                case "vote_asc": query = query.OrderBy(q => q.VoteCount); break;

                default: query = query.OrderByDescending(q => q.CreatedDate); break;
            }

            return View(query.ToList());
        }

        // ================= DETAILS =================
        public async Task<IActionResult> Details(int id)
        {
            var question = await _unitOfWork.Questions.GetAsync(
                q => q.Id == id,
                "Category,ApplicationUser,Answers,Answers.ApplicationUser,Votes");

            if (question == null) return NotFound();

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

        // ================= AJAX VOTE (PUAN SİSTEMİ EKLENDİ) =================
        [HttpPost]
        [Authorize]
        public async Task<JsonResult> Vote(int questionId, bool isUpVote)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Json(new { success = false, message = "Giriş yapmalısınız." });

            // Puan verebilmek için sorunun sahibini (ApplicationUser) dahil ederek çekiyoruz.
            var question = await _unitOfWork.Questions.GetAsync(q => q.Id == questionId, "ApplicationUser");

            if (question == null) return Json(new { success = false, message = "Soru bulunamadı." });

            var existingVote = await _unitOfWork.QuestionVotes.GetAsync(v => v.QuestionId == questionId && v.ApplicationUserId == userId);

            int reputationChange = 0; // Puan değişimi

            if (existingVote != null)
            {
                if (existingVote.IsUpVote == isUpVote)
                {
                    // Oyu geri çekme
                    _unitOfWork.QuestionVotes.Remove(existingVote);
                    // Puanı geri al (Like ise -5, Dislike ise +2)
                    reputationChange = isUpVote ? -5 : 2;
                }
                else
                {
                    // Oyu değiştirme (Like -> Dislike veya tam tersi)
                    existingVote.IsUpVote = isUpVote;
                    _unitOfWork.QuestionVotes.Update(existingVote);
                    // Puan farkı (Like olduysa +7, Dislike olduysa -7)
                    reputationChange = isUpVote ? 7 : -7;
                }
            }
            else
            {
                // Yeni oy
                await _unitOfWork.QuestionVotes.AddAsync(new QuestionVote
                {
                    QuestionId = questionId,
                    ApplicationUserId = userId,
                    IsUpVote = isUpVote
                });

                // Yeni puan (Like +5, Dislike -2)
                reputationChange = isUpVote ? 5 : -2;
            }

            // ✅ PUANI KULLANICIYA İŞLE
            // Kendi sorusuna oy veremez mantığı olsa da, backend tarafında puan kazanmasını engelliyoruz.
            if (question.ApplicationUser != null && question.ApplicationUser.Id != userId)
            {
                question.ApplicationUser.Reputation += reputationChange;
            }

            await _unitOfWork.CompleteAsync();

            // Güncel oyları say
            var allVotes = await _unitOfWork.QuestionVotes.GetAllAsync(v => v.QuestionId == questionId);
            var score = allVotes.Count(v => v.IsUpVote) - allVotes.Count(v => !v.IsUpVote);

            // Sorunun toplam oy sayısını güncelle
            question.VoteCount = score;
            _unitOfWork.Questions.Update(question);
            await _unitOfWork.CompleteAsync();

            return Json(new { success = true, totalScore = score });
        }

        // ================= BY CATEGORY =================
        public async Task<IActionResult> ByCategory(int id, string search, string sortOrder)
        {
            var category = await _unitOfWork.Categories.GetAsync(c => c.Id == id, "Questions,Questions.ApplicationUser,Questions.Answers");

            if (category == null) return NotFound();

            var query = category.Questions.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(q => q.Title.ToLower().Contains(search) ||
                                         q.Content.ToLower().Contains(search));
                ViewBag.Search = search;
            }

            ViewBag.SortOrder = sortOrder;

            switch (sortOrder)
            {
                case "date_asc": query = query.OrderBy(q => q.CreatedDate); break;
                case "title_asc": query = query.OrderBy(q => q.Title); break;
                case "title_desc": query = query.OrderByDescending(q => q.Title); break;

                case "view_desc": query = query.OrderByDescending(q => q.ViewCount); break;
                case "view_asc": query = query.OrderBy(q => q.ViewCount); break;

                case "vote_desc": query = query.OrderByDescending(q => q.VoteCount); break;
                case "vote_asc": query = query.OrderBy(q => q.VoteCount); break;

                default: query = query.OrderByDescending(q => q.CreatedDate); break;
            }

            ViewBag.CategoryName = category.Name;
            ViewBag.CategoryId = id;

            return View(query.ToList());
        }
    }
}