using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gentlemen.Data;
using Gentlemen.Models;
using System.Text.RegularExpressions;

namespace Gentlemen.Controllers
{
    public class BlogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BlogController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var blogs = await _context.Blogs
                .OrderByDescending(b => b.PublishDate)
                .ToListAsync();
            return View(blogs);
        }

        public async Task<IActionResult> Details(int id)
        {
            var blog = await _context.Blogs
                .FirstOrDefaultAsync(b => b.Id == id);

            if (blog == null)
            {
                return NotFound();
            }

            // Eğer slug boşsa, oluştur ve kaydet
            if (string.IsNullOrEmpty(blog.Slug))
            {
                blog.Slug = GenerateSlug(blog.Title);
                await _context.SaveChangesAsync();
            }

            // Slug tabanlı URL'ye yönlendir
            return RedirectToAction("DetailsBySlug", new { slug = blog.Slug });
        }

        [HttpGet]
        [Route("blog/{slug}")]
        public async Task<IActionResult> DetailsBySlug(string slug)
        {
            var blog = await _context.Blogs
                .FirstOrDefaultAsync(b => b.Slug == slug);

            if (blog == null)
            {
                return NotFound();
            }

            blog.ViewCount++;
            await _context.SaveChangesAsync();

            return View("Details", blog);
        }

        public async Task<IActionResult> Category(string category)
        {
            var blogs = await _context.Blogs
                .Where(b => b.Category == category)
                .OrderByDescending(b => b.PublishDate)
                .ToListAsync();
            return View("Index", blogs);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Blog blog)
        {
            if (ModelState.IsValid)
            {
                blog.PublishDate = DateTime.Now;
                blog.ViewCount = 0;
                
                // Slug oluştur
                if (string.IsNullOrEmpty(blog.Slug))
                {
                    blog.Slug = GenerateSlug(blog.Title);
                }

                _context.Add(blog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(blog);
        }

        // Slug oluşturma yardımcı metodu
        private string GenerateSlug(string title)
        {
            // Türkçe karakterleri değiştir
            string slug = title.ToLower()
                .Replace('ı', 'i')
                .Replace('ğ', 'g')
                .Replace('ü', 'u')
                .Replace('ş', 's')
                .Replace('ö', 'o')
                .Replace('ç', 'c');

            // Alfanumerik olmayan karakterleri tire ile değiştir
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            // Boşlukları tire ile değiştir
            slug = Regex.Replace(slug, @"\s+", "-");
            // Birden fazla tireyi tek tire ile değiştir
            slug = Regex.Replace(slug, @"-+", "-");
            // Baştaki ve sondaki tireleri kaldır
            slug = slug.Trim('-');

            return slug;
        }
    }
}
