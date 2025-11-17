using Microsoft.AspNetCore.Mvc;

namespace SoruCevapPortalı.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            // Test verileri
            ViewBag.TotalQuestions = 25;
            ViewBag.TotalCategories = 8;
            ViewBag.TotalAnswers = 150;
            ViewBag.TotalUsers = 42;

            return View();
        }

        public IActionResult Test()
        {
            return Content("✅ AdminController çalışıyor!");
        }
    }
}