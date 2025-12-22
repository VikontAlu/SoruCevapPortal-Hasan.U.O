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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Question -> Answers
            modelBuilder.Entity<Question>()
                .HasMany(q => q.Answers)
                .WithOne(a => a.Question)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Question -> QuestionVotes
            modelBuilder.Entity<Question>()
                .HasMany(q => q.Votes)
                .WithOne(v => v.Question)
                .HasForeignKey(v => v.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Answer -> AnswerVotes
            modelBuilder.Entity<Answer>()
                .HasMany(a => a.Votes)
                .WithOne(v => v.Answer)
                .HasForeignKey(v => v.AnswerId)
                .OnDelete(DeleteBehavior.Cascade);

            // ❗ USER FK — CASCADE KAPALI ❗
            modelBuilder.Entity<Question>()
                .HasOne(q => q.ApplicationUser)
                .WithMany()
                .HasForeignKey(q => q.ApplicationUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Answer>()
                .HasOne(a => a.ApplicationUser)
                .WithMany()
                .HasForeignKey(a => a.ApplicationUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Unique vote constraints
            modelBuilder.Entity<QuestionVote>()
                .HasIndex(v => new { v.QuestionId, v.ApplicationUserId })
                .IsUnique();

            modelBuilder.Entity<AnswerVote>()
                .HasIndex(v => new { v.AnswerId, v.ApplicationUserId })
                .IsUnique();
        }
    }
}
