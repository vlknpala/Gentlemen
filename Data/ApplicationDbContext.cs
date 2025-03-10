using Microsoft.EntityFrameworkCore;
using Gentlemen.Models;

namespace Gentlemen.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Outfit> Outfits { get; set; }
        public DbSet<StyleTip> StyleTips { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Image> Images { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Blog entity configuration
            modelBuilder.Entity<Blog>(entity =>
            {
                entity.ToTable("Blogs");
                entity.Property(b => b.Title)
                    .IsRequired()
                    .HasMaxLength(200);
                entity.Property(b => b.Content)
                    .IsRequired();
                entity.Property(b => b.Author)
                    .IsRequired();
                entity.Property(b => b.Category)
                    .IsRequired();
            });

            // Outfit entity configuration
            modelBuilder.Entity<Outfit>(entity =>
            {
                entity.ToTable("Outfits");
                entity.Property(o => o.Title)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(o => o.Description)
                    .IsRequired();
                entity.Property(o => o.EstimatedPrice)
                    .HasPrecision(18, 2);
                entity.Property(o => o.Season)
                    .IsRequired();
                entity.Property(o => o.Style)
                    .IsRequired();
                entity.Property(o => o.Slug)
                    .IsRequired(false);
            });

            // StyleTip entity configuration
            modelBuilder.Entity<StyleTip>(entity =>
            {
                entity.ToTable("StyleTips");
                entity.Property(s => s.Title)
                    .IsRequired()
                    .HasMaxLength(200);
                entity.Property(s => s.Content)
                    .IsRequired();
                entity.Property(s => s.Category);
                entity.Property(s => s.Author)
                    .IsRequired();
                entity.Property(s => s.Slug)
                    .IsRequired(false);

                entity.HasOne(s => s.CategoryObject)
                    .WithMany(c => c.StyleTips)
                    .HasForeignKey(s => s.CategoryId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // User entity configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasIndex(u => u.Email)
                    .IsUnique();
                entity.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(u => u.Password)
                    .IsRequired();
                entity.Property(u => u.FirstName)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(u => u.LastName)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(u => u.PhoneNumber)
                    .IsRequired()
                    .HasMaxLength(20);
            });

            // Image entity configuration
            modelBuilder.Entity<Image>(entity =>
            {
                entity.ToTable("Images");
                entity.Property(i => i.ImageUrl)
                    .IsRequired();
                entity.Property(i => i.CreatedAt)
                    .IsRequired();
            });

            // Varsayılan kategorileri ekle
            modelBuilder.Entity<Category>().HasData(
                new Category
                {
                    Id = 1,
                    Title = "İş Stili",
                    Description = "Profesyonel ve şık görünüm için öneriler",
                    ImageUrl = "/images/business-style.jpg",
                    Slug = "is-stili",
                    IsActive = true
                },
                new Category
                {
                    Id = 2,
                    Title = "Günlük Stil",
                    Description = "Rahat ve trend günlük kombinler",
                    ImageUrl = "/images/casual-style.jpg",
                    Slug = "gunluk-stil",
                    IsActive = true
                },
                new Category
                {
                    Id = 3,
                    Title = "Özel Günler",
                    Description = "Özel anlar için şık seçimler",
                    ImageUrl = "/images/special-occasions.jpg",
                    Slug = "ozel-gunler",
                    IsActive = true
                }
            );
        }
    }
}
