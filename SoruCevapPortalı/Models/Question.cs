using SoruCevapPortalı.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoruCevapPortalı.Models
{
    public class Question
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Soru başlığı zorunludur!")]
        [StringLength(500, ErrorMessage = "Soru başlığı 500 karakterden uzun olamaz!")]
        [Display(Name = "Soru Başlığı")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soru içeriği zorunludur!")]
        [Display(Name = "Soru İçeriği")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Güncellenme Tarihi")]
        public DateTime? UpdatedDate { get; set; }

        [Display(Name = "Görüntülenme Sayısı")]
        public int ViewCount { get; set; } = 0;

        [Display(Name = "Oy Sayısı")]
        public int VoteCount { get; set; } = 0;

        [Display(Name = "Çözüldü mü?")]
        public bool IsSolved { get; set; } = false;

        // Foreign Keys
        [Display(Name = "Kullanıcı")]
        public string? UserId { get; set; }

        [Display(Name = "Kategori")]
        public int CategoryId { get; set; }

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        public virtual ICollection<Answer> Answers { get; set; } = new HashSet<Answer>();
        public virtual ICollection<QuestionVote> Votes { get; set; } = new HashSet<QuestionVote>();
    }
}