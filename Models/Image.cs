using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gentlemen.Models
{
    public class Image
    {
        public int Id { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public string? Title { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // İlişkiler
        public int? OutfitId { get; set; }
        [ForeignKey("OutfitId")]
        public virtual Outfit? Outfit { get; set; }

        public int? BlogId { get; set; }
        [ForeignKey("BlogId")]
        public virtual Blog? Blog { get; set; }

        public int? StyleTipId { get; set; }
        [ForeignKey("StyleTipId")]
        public virtual StyleTip? StyleTip { get; set; }

        public int? CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        // Sıralama için
        public int DisplayOrder { get; set; } = 0;

        // Ana görsel mi?
        public bool IsMain { get; set; } = false;
    }
} 