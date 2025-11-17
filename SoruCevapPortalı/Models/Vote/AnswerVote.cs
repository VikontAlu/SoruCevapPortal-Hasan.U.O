using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoruCevapPortalı.Models
{
    public class AnswerVote
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Oy Türü")]
        public bool IsUpVote { get; set; }

        [Display(Name = "Cevap")]
        public int AnswerId { get; set; }

        [Display(Name = "Kullanıcı")]
        public string? UserId { get; set; }

        // Navigation Properties
        [ForeignKey("AnswerId")]
        public virtual Answer? Answer { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}