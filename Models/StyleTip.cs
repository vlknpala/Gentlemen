using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gentlemen.Models
{
    public class StyleTip
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Başlık alanı zorunludur.")]
        [StringLength(200, ErrorMessage = "Başlık en fazla 200 karakter olabilir.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "İçerik alanı zorunludur.")]
        public string Content { get; set; } = string.Empty;

        // Kategori string olarak saklanır (geriye dönük uyumluluk için)
        public string Category { get; set; } = string.Empty;

        // Kategori ilişkisi
        [Display(Name = "Kategori")]
        public int? CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category? CategoryObject { get; set; }

        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Yayın tarihi zorunludur.")]
        [Display(Name = "Yayın Tarihi")]
        public DateTime PublishDate { get; set; }

        [Required(ErrorMessage = "Yazar alanı zorunludur.")]
        public string Author { get; set; } = string.Empty;

        [Display(Name = "Öne Çıkan")]
        public bool IsFeatured { get; set; }

        public int Likes { get; set; }
    }
}
