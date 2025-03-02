using System.ComponentModel.DataAnnotations;

namespace Gentlemen.Models
{
    public enum OutfitCategory
    {
        GunlukKombinler,
        IsKombinleri,
        OzelGunKombinleri
    }

    public class FeaturedCategory
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Başlık zorunludur")]
        [StringLength(100)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Açıklama zorunludur")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Görsel URL zorunludur")]
        public string ImageUrl { get; set; }

        [Required(ErrorMessage = "Kategori seçimi zorunludur")]
        public OutfitCategory Category { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
} 