using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Gentlemen.Models
{
    public class Blog
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Başlık alanı zorunludur.")]
        [StringLength(200, ErrorMessage = "Başlık en fazla 200 karakter olabilir.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "İçerik alanı zorunludur.")]
        public string Content { get; set; } = string.Empty;

        // Geriye dönük uyumluluk için ana görsel URL'si
        public string? ImageUrl { get; set; }

        public string? Slug { get; set; }

        [Required(ErrorMessage = "Yayın tarihi zorunludur.")]
        [Display(Name = "Yayın Tarihi")]
        public DateTime PublishDate { get; set; }

        [Required(ErrorMessage = "Yazar alanı zorunludur.")]
        public string Author { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kategori alanı zorunludur.")]
        public string Category { get; set; } = string.Empty;

        public int ViewCount { get; set; }

        // Çoklu fotoğraf ilişkisi
        public virtual ICollection<Image>? Images { get; set; }
    }
}
