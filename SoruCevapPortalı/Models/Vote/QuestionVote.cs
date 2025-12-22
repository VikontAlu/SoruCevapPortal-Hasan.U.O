using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoruCevapPortalı.Models
{
    public class QuestionVote
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int QuestionId { get; set; }

        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        public bool IsUpVote { get; set; }

        // Navigation
        [ForeignKey("QuestionId")]
        public virtual Question Question { get; set; } = null!;
    }
}
