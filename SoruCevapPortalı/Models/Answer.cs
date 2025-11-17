using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoruCevapPortalı.Models
{
    public class Answer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }

        public int VoteCount { get; set; } = 0;
        public bool IsBestAnswer { get; set; } = false;

        public int QuestionId { get; set; }
        public string? UserId { get; set; }

        [ForeignKey("QuestionId")]
        public virtual Question? Question { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}