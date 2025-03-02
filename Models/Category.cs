using System.ComponentModel.DataAnnotations;

namespace Gentlemen.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kategori adı zorunludur.")]
        [StringLength(50, ErrorMessage = "Kategori adı en fazla 50 karakter olabilir.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Açıklama zorunludur.")]
        [StringLength(200, ErrorMessage = "Açıklama en fazla 200 karakter olabilir.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Görsel URL zorunludur.")]
        public string ImageUrl { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kategori tipi zorunludur.")]
        public string Type { get; set; } = string.Empty; // "Business", "Casual", "Special"

        public bool IsActive { get; set; } = true;

        public ICollection<CategoryContent> Contents { get; set; } = new List<CategoryContent>();
    }
} 