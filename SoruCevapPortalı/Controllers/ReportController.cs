using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoruCevapPortalı.Data;
using SoruCevapPortalı.Models;
using System.Security.Claims;

namespace SoruCevapPortalı.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Create(int? questionId, int? answerId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                return Json(new { success = false, message = "Lütfen bir sebep yazın." });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var report = new Report
            {
                ReporterId = userId,
                QuestionId = questionId,
                AnswerId = answerId,
                Reason = reason,
                CreatedDate = DateTime.Now,
                IsReviewed = false
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Bildiriminiz alındı. Teşekkürler!" });
        }
    }
}