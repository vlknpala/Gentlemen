using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Gentlemen.Models
{
    public class Outfit
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Başlık alanı zorunludur.")]
        [StringLength(100, ErrorMessage = "Başlık en fazla 100 karakter olabilir.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Açıklama alanı zorunludur.")]
        public string Description { get; set; } = string.Empty;

        // SEO-friendly URL için slug
        public string? Slug { get; set; }

        // Geriye dönük uyumluluk için ana görsel URL'si
        public string ImageUrl { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sezon alanı zorunludur.")]
        public string Season { get; set; } = string.Empty;

        [Required(ErrorMessage = "Stil alanı zorunludur.")]
        public string Style { get; set; } = string.Empty;

        [Required(ErrorMessage = "Durum alanı zorunludur.")]
        public string Occasion { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tahmini fiyat alanı zorunludur.")]
        [Display(Name = "Tahmini Fiyat")]
        [Range(0, double.MaxValue, ErrorMessage = "Fiyat 0'dan büyük olmalıdır.")]
        public decimal EstimatedPrice { get; set; }

        [Required(ErrorMessage = "En az bir kıyafet parçası eklemelisiniz.")]
        [Display(Name = "Kıyafet Parçaları")]
        public string[] ClothingItems { get; set; } = Array.Empty<string>();

        public int Likes { get; set; }

        [Display(Name = "Öne Çıkan")]
        public bool IsFeatured { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Çoklu fotoğraf ilişkisi
        public virtual ICollection<Image>? Images { get; set; }
    }
}
