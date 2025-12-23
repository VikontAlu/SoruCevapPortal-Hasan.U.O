using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SoruCevapPortalı.Data;
using SoruCevapPortalı.Models;
using SoruCevapPortalı.Interfaces;  // ✅ EKLENDİ
using SoruCevapPortalı.Repositories; // ✅ EKLENDİ

var builder = WebApplication.CreateBuilder(args);

// 1. Veritabanı Bağlantısı
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 👇 REPOSITORY PATTERN SERVİSİ (BUNU EKLEMEK ŞART) 👇
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// 2. Identity ve Rol Ayarları
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 3; // Şifre "123" olabilsin diye
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// -----------------------------------------------------------
// 👇 BAŞLANGIÇTA OTOMATİK ADMİN OLUŞTURMA KODU (SEED DATA) 👇
// -----------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // 1. Rolleri Kontrol Et / Oluştur
        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new IdentityRole("Admin"));

        if (!await roleManager.RoleExistsAsync("User"))
            await roleManager.CreateAsync(new IdentityRole("User"));

        // 2. Admin Kullanıcısını Kontrol Et / Oluştur
        var adminEmail = "admin@admin.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Sistem",
                LastName = "Yöneticisi",
                EmailConfirmed = true,
                RegistrationDate = DateTime.Now
            };

            // Kullanıcıyı oluştur (Şifre: 123)
            var result = await userManager.CreateAsync(adminUser, "123");

            // Eğer başarılıysa Admin rolünü ver
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
    catch (Exception ex)
    {
        // Hata olursa konsola yazsın (Geliştirme aşamasında görmek için)
        Console.WriteLine("Seed Data Hatası: " + ex.Message);
    }
}
// -----------------------------------------------------------

// 3. Hata Yönetimi
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();