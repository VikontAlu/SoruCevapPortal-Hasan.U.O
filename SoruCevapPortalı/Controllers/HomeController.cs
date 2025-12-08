using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoruCevapPortalı.Data;
using SoruCevapPortalı.Models;

namespace SoruCevapPortalı.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Ana sayfa - TÜM VERİLERLE BİRLİKTE
        public IActionResult Index()
        {
            try
            {
                // Son 10 soruyu getir (en yeni sorular)
                var recentQuestions = _context.Questions
                    .Include(q => q.ApplicationUser)
                    .Include(q => q.Category)
                    .Include(q => q.Answers)
                    .OrderByDescending(q => q.CreatedDate)
                    .Take(10)
                    .ToList();

                // Tüm kategorileri getir (soru sayılarıyla birlikte)
                var categories = _context.Categories
                    .Include(c => c.Questions)
                    .OrderBy(c => c.Name)
                    .Take(8)  // En fazla 8 kategori göster
                    .ToList();

                // Popüler sorular (en çok cevap alanlar)
                var popularQuestions = _context.Questions
                    .Include(q => q.ApplicationUser)
                    .Include(q => q.Category)
                    .Include(q => q.Answers)
                    .OrderByDescending(q => q.Answers.Count)
                    .ThenByDescending(q => q.ViewCount)
                    .Take(5)
                    .ToList();

                // İstatistikleri hesapla
                var totalQuestions = _context.Questions.Count();
                var totalAnswers = _context.Answers.Count();
                var totalUsers = _context.Users.Count();
                var totalCategories = _context.Categories.Count();

                // Çözülmüş soru sayısı
                var solvedQuestions = _context.Questions.Count(q => q.IsSolved);
                var unsolvedQuestions = totalQuestions - solvedQuestions;

                // ViewBag ile tüm verileri gönder
                ViewBag.RecentQuestions = recentQuestions;
                ViewBag.Categories = categories;
                ViewBag.PopularQuestions = popularQuestions;

                ViewBag.TotalQuestions = totalQuestions;
                ViewBag.TotalAnswers = totalAnswers;
                ViewBag.TotalUsers = totalUsers;
                ViewBag.TotalCategories = totalCategories;
                ViewBag.SolvedQuestions = solvedQuestions;
                ViewBag.UnsolvedQuestions = unsolvedQuestions;

                ViewBag.Message = $"Toplam {totalQuestions} soru, {totalAnswers} cevap";

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ana sayfa yüklenirken hata");

                // Hata durumunda boş veriler gönder
                ViewBag.RecentQuestions = new List<Question>();
                ViewBag.Categories = new List<Category>();
                ViewBag.PopularQuestions = new List<Question>();

                ViewBag.TotalQuestions = 0;
                ViewBag.TotalAnswers = 0;
                ViewBag.TotalUsers = 0;
                ViewBag.TotalCategories = 0;
                ViewBag.SolvedQuestions = 0;
                ViewBag.UnsolvedQuestions = 0;

                ViewBag.Error = "Sistem yüklenirken bir hata oluştu. Lütfen daha sonra tekrar deneyin.";

                return View();
            }
        }

        // Hakkında sayfası
        public IActionResult About()
        {
            return View();
        }

        // Gizlilik politikası
        public IActionResult Privacy()
        {
            return View();
        }

        // İletişim sayfası
        public IActionResult Contact()
        {
            return View();
        }

        // Yardım sayfası
        public IActionResult Help()
        {
            return View();
        }

        // Site haritası
        public IActionResult Sitemap()
        {
            return View();
        }

        // Hata sayfası
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // AJAX: Canlı istatistikler (isteğe bağlı)
        [HttpGet]
        public JsonResult GetLiveStats()
        {
            try
            {
                var stats = new
                {
                    TotalQuestions = _context.Questions.Count(),
                    TotalAnswers = _context.Answers.Count(),
                    TotalUsers = _context.Users.Count(),
                    OnlineUsers = 1, // Basit versiyon
                    LatestQuestion = _context.Questions
                        .OrderByDescending(q => q.CreatedDate)
                        .Select(q => new {
                            Id = q.Id,
                            Title = q.Title,
                            Time = q.CreatedDate.ToString("HH:mm")
                        })
                        .FirstOrDefault()
                };

                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}