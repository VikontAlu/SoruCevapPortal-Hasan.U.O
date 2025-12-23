using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR; // ✅ SignalR kütüphanesi
using SoruCevapPortalı.Data;
using SoruCevapPortalı.Hubs;        // ✅ Hub'ımızın olduğu yer
using SoruCevapPortalı.Models;
using System.Security.Claims;

namespace SoruCevapPortalı.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ReportHub> _hubContext; // ✅ Hub Context

        public ReportController(ApplicationDbContext context, IHubContext<ReportHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
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

            // ✅ SIGNALR İLE ADMİNLERE BİLDİRİM GÖNDER
            // "ReceiveReportNotification" adında bir fonksiyonu tetikliyoruz.
            await _hubContext.Clients.All.SendAsync("ReceiveReportNotification", "Yeni bir şikayet bildirimi alındı!");

            return Json(new { success = true, message = "Bildiriminiz alındı. Teşekkürler!" });
        }
    }
}