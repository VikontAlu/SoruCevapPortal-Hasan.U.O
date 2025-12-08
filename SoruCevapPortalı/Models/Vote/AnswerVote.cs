using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoruCevapPortalı.Models
{
    public class AnswerVote
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AnswerId { get; set; }

        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        
        public bool IsUpVote { get; set; }

        // Navigation
        [ForeignKey("AnswerId")]
        public virtual Answer Answer { get; set; } = null!;
    }
}
