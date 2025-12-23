using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SoruCevapPortalı.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(100)]
        [Display(Name = "Ad")]
        public string? FirstName { get; set; }

        [StringLength(100)]
        [Display(Name = "Soyad")]
        public string? LastName { get; set; }

        [Display(Name = "Kayıt Tarihi")]
        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        [Display(Name = "Profil Resmi")]
        public string? ProfilePicture { get; set; }

        // ✅ YENİ EKLENEN ALANLAR (Puan Sistemi ve Ayarlar İçin)

        [Display(Name = "Puan (Reputation)")]
        public int Reputation { get; set; } = 0;

        [StringLength(100)]
        [Display(Name = "Meslek / Unvan")]
        public string? JobTitle { get; set; }

        [Display(Name = "Hakkında")]
        public string? AboutMe { get; set; } // Controller tarafında AboutMe kullandığımız için ismini güncelledik.

        // -------------------- Navigation Properties --------------------
        // (İlişkili tablolarla bağlantılar)

        public virtual ICollection<Question> Questions { get; set; } = new HashSet<Question>();
        public virtual ICollection<Answer> Answers { get; set; } = new HashSet<Answer>();
        public virtual ICollection<QuestionVote> QuestionVotes { get; set; } = new HashSet<QuestionVote>();
        public virtual ICollection<AnswerVote> AnswerVotes { get; set; } = new HashSet<AnswerVote>();
    }
}