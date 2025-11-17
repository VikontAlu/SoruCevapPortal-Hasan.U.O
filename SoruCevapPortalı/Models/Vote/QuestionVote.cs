using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoruCevapPortalı.Models
{
    public class QuestionVote
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Oy Türü")]
        public bool IsUpVote { get; set; }

        [Display(Name = "Soru")]
        public int QuestionId { get; set; }

        [Display(Name = "Kullanıcı")]
        public string? UserId { get; set; }

        // Navigation Properties
        [ForeignKey("QuestionId")]
        public virtual Question? Question { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}