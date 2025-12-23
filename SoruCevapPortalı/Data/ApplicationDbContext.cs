using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SoruCevapPortalı.Models;

namespace SoruCevapPortalı.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // -------------------- DbSets --------------------
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Category> Categories { get; set; }

        public DbSet<QuestionVote> QuestionVotes { get; set; }
        public DbSet<AnswerVote> AnswerVotes { get; set; }

        public DbSet<Report> Reports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ... (Senin diğer ilişki kodların burada kalacak) ...

            // 👇 BURADAN AŞAĞISINI EKLE: Varsayılan Kategoriler
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Genel", Description = "Genel konular", CreatedDate = new DateTime(2025, 1, 1) },
                new Category { Id = 2, Name = "Yazılım", Description = "Yazılım ve kodlama soruları", CreatedDate = new DateTime(2025, 1, 1) },
                new Category { Id = 3, Name = "Donanım", Description = "Bilgisayar donanımları", CreatedDate = new DateTime(2025, 1, 1) },
                new Category { Id = 4, Name = "Oyun", Description = "Oyun dünyası ve tavsiyeler", CreatedDate = new DateTime(2025, 1, 1) }
            );

        // Question -> Answers (Bir sorunun çok cevabı olur)
        modelBuilder.Entity<Question>()
                .HasMany(q => q.Answers)
                .WithOne(a => a.Question)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Question -> QuestionVotes (Bir sorunun çok oyu olur)
            modelBuilder.Entity<Question>()
                .HasMany(q => q.Votes)
                .WithOne(v => v.Question)
                .HasForeignKey(v => v.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Answer -> AnswerVotes (Bir cevabın çok oyu olur)
            modelBuilder.Entity<Answer>()
                .HasMany(a => a.Votes)
                .WithOne(v => v.Answer)
                .HasForeignKey(v => v.AnswerId)
                .OnDelete(DeleteBehavior.Cascade);

            // ❗ USER FK — CASCADE KAPALI ❗
            // DÜZELTME: WithMany() içleri dolduruldu.
            // Bu sayede EF Core gizli 'ApplicationUserId1' sütunu oluşturmayacak.

            modelBuilder.Entity<Question>()
                .HasOne(q => q.ApplicationUser)
                .WithMany(u => u.Questions) // ✅ DÜZELTİLDİ: User'ın Questions listesine bağlandı
                .HasForeignKey(q => q.ApplicationUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Answer>()
                .HasOne(a => a.ApplicationUser)
                .WithMany(u => u.Answers)   // ✅ DÜZELTİLDİ: User'ın Answers listesine bağlandı
                .HasForeignKey(a => a.ApplicationUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Unique vote constraints (Bir kullanıcı bir soruya/cevaba sadece bir kez oy verebilir)
            modelBuilder.Entity<QuestionVote>()
                .HasIndex(v => new { v.QuestionId, v.ApplicationUserId })
                .IsUnique();

            modelBuilder.Entity<AnswerVote>()
                .HasIndex(v => new { v.AnswerId, v.ApplicationUserId })
                .IsUnique();


        }
    }
}