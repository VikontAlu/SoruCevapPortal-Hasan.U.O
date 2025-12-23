using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoruCevapPortalı.Data;
using SoruCevapPortalı.Models;
using System.Diagnostics;

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

        // Ana sayfa
        public IActionResult Index()
        {
            try
            {
                // ✅ 1. İSTATİSTİKLERİ GERİ GETİRDİK (Kutular için gerekli)
                ViewBag.TotalQuestions = _context.Questions.Count();
                ViewBag.TotalAnswers = _context.Answers.Count();
                ViewBag.TotalUsers = _context.Users.Count();
                ViewBag.TotalCategories = _context.Categories.Count();

                // 2. Son 10 soruyu getir
                var recentQuestions = _context.Questions
                    .Include(q => q.ApplicationUser)
                    .Include(q => q.Category)
                    .Include(q => q.Answers)
                    .OrderByDescending(q => q.CreatedDate)
                    .Take(10)
                    .ToList();

                // 3. Kategorileri getir
                var categories = _context.Categories
                    .Include(c => c.Questions)
                    .OrderBy(c => c.Name)
                    .Take(8)
                    .ToList();

                // 4. Popüler soruları getir
                var popularQuestions = _context.Questions
                    .Include(q => q.ApplicationUser)
                    .Include(q => q.Category)
                    .Include(q => q.Answers)
                    .OrderByDescending(q => q.Answers.Count)
                    .ThenByDescending(q => q.ViewCount)
                    .Take(5)
                    .ToList();

                // Verileri View'a gönder
                ViewBag.RecentQuestions = recentQuestions;
                ViewBag.Categories = categories;
                ViewBag.PopularQuestions = popularQuestions;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ana sayfa yüklenirken hata");
                return View();
            }
        }

        public IActionResult About() => View();
        public IActionResult Privacy() => View();
        public IActionResult Contact() => View();
        public IActionResult Help() => View();
        public IActionResult Sitemap() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Admin Düzeltme Metodu
        public async Task<IActionResult> FixAdmin([FromServices] UserManager<ApplicationUser> userManager, [FromServices] RoleManager<IdentityRole> roleManager)
        {
            var adminEmail = "admin@admin.com";
            var password = "Admin123!";
            var logs = new List<string>();

            try
            {
                var oldUser = await userManager.FindByEmailAsync(adminEmail);
                if (oldUser != null)
                {
                    await userManager.DeleteAsync(oldUser);
                    logs.Add("🗑️ Eski hatalı admin kullanıcısı silindi.");
                }

                if (!await roleManager.RoleExistsAsync("Admin"))
                {
                    await roleManager.CreateAsync(new IdentityRole("Admin"));
                    logs.Add("✅ Admin rolü oluşturuldu.");
                }

                if (!await roleManager.RoleExistsAsync("User"))
                    await roleManager.CreateAsync(new IdentityRole("User"));

                var newAdmin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "Sistem",
                    LastName = "Yöneticisi",
                    RegistrationDate = DateTime.Now
                };

                var result = await userManager.CreateAsync(newAdmin, password);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                    logs.Add($"🎉 YENİ Admin oluşturuldu. (Email: {adminEmail} - Şifre: {password})");
                }
                else
                {
                    logs.Add("❌ Hata: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            catch (Exception ex)
            {
                logs.Add($"💥 Kritik Hata: {ex.Message}");
            }

            return Content(string.Join("\n", logs));
        }
    }
}