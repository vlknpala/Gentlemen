using System.ComponentModel.DataAnnotations;

namespace Gentlemen.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public string Slug { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // İlişkili stil ipuçları
        public virtual ICollection<StyleTip> StyleTips { get; set; }
    }
} 