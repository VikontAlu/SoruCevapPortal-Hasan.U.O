using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoruCevapPortalı.Models
{
    public class Answer
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Cevap içeriği zorunludur!")]
        [StringLength(5000, ErrorMessage = "Cevap 5000 karakterden uzun olamaz!")]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }

        public int VoteCount { get; set; } = 0;

        // ✅ Kabul edilen / en iyi cevap bilgileri
        public bool IsAccepted { get; set; } = false;
        public bool IsBestAnswer { get; set; } = false;

        /* ---------------- FOREIGN KEYS ---------------- */

        [Required]
        public int QuestionId { get; set; }

        // ✅ Identity uyumlu kullanıcı FK
        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        /* ---------------- NAVIGATION ---------------- */

        [ForeignKey(nameof(QuestionId))]
        public virtual Question Question { get; set; } = null!;

        // ✅ Navigation ismi ApplicationUser
        [ForeignKey(nameof(ApplicationUserId))]
        public virtual ApplicationUser ApplicationUser { get; set; } = null!;

        // ✅ Cevabın oyları
        public virtual ICollection<AnswerVote> Votes { get; set; } = new List<AnswerVote>();

        /* ---------------- NOT MAPPED ---------------- */

        [NotMapped]
        public string ShortContent =>
            Content.Length > 200 ? Content.Substring(0, 200) + "..." : Content;

        [NotMapped]
        public string TimeAgo
        {
            get
            {
                var timeSpan = DateTime.Now - CreatedDate;

                if (timeSpan.TotalDays >= 30)
                    return $"{(int)(timeSpan.TotalDays / 30)} ay önce";
                else if (timeSpan.TotalDays >= 1)
                    return $"{(int)timeSpan.TotalDays} gün önce";
                else if (timeSpan.TotalHours >= 1)
                    return $"{(int)timeSpan.TotalHours} saat önce";
                else if (timeSpan.TotalMinutes >= 1)
                    return $"{(int)timeSpan.TotalMinutes} dakika önce";
                else
                    return "Az önce";
            }
        }
    }
}
