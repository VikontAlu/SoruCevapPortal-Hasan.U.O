using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoruCevapPortalı.Models
{
    public class Report
    {
        [Key]
        public int Id { get; set; }

        public string ReporterId { get; set; } // Şikayet eden kişi

        public int? QuestionId { get; set; } // Şikayet edilen soru (varsa)
        public int? AnswerId { get; set; }   // Şikayet edilen cevap (varsa)

        [Required(ErrorMessage = "Lütfen bir sebep belirtin.")]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty; // Sebep (Spam, Hakaret vb.)

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public bool IsReviewed { get; set; } = false; // Admin inceledi mi?

        // Navigation
        [ForeignKey("ReporterId")]
        public virtual ApplicationUser Reporter { get; set; }
        public virtual Question? Question { get; set; }
        public virtual Answer? Answer { get; set; }
    }
}