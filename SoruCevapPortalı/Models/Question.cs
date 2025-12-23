using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; // ✅ BU EKLENDİ (ValidateNever için şart)

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

        // ❗ Kullanıcı ID'si formdan gelmez, Controller doldurur.
        public string ApplicationUserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kategori seçilmelidir!")]
        [Display(Name = "Kategori")]
        public int CategoryId { get; set; }

        // ------------- NAVIGATION -------------
        // Bu özellikler formdan gelmediği için doğrulama hatası veriyordu.
        // [ValidateNever] ekleyerek "bunları kontrol etme" diyoruz.

        [ValidateNever] // ✅ EKLENDİ
        [ForeignKey(nameof(ApplicationUserId))]
        public virtual ApplicationUser ApplicationUser { get; set; } = null!;

        [ValidateNever] // ✅ EKLENDİ
        [ForeignKey(nameof(CategoryId))]
        public virtual Category Category { get; set; } = null!;

        // Soruya ait cevaplar
        [ValidateNever] // ✅ EKLENDİ
        public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

        // Soruya ait oylar
        [ValidateNever] // ✅ EKLENDİ
        public virtual ICollection<QuestionVote> Votes { get; set; } = new List<QuestionVote>();
    }
}