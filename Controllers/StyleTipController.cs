using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gentlemen.Data;
using Gentlemen.Models;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Gentlemen.Controllers
{
    public class StyleTipController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StyleTipController> _logger;

        public StyleTipController(ApplicationDbContext context, ILogger<StyleTipController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var tips = await _context.StyleTips
                    .Include(t => t.CategoryObject)
                    .OrderByDescending(t => t.PublishDate)
                    .ToListAsync();

                _logger.LogInformation($"Toplam {tips.Count} stil ipucu listeleniyor.");
                return View(tips);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Stil ipuçları listelenirken hata oluştu: {ex.Message}");
                return View(new List<StyleTip>());
            }
        }

        // Eski ID tabanlı Details metodu - yönlendirme için kullanılacak
        [HttpGet("styletip/details/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var tip = await _context.StyleTips
                .Include(t => t.CategoryObject)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tip == null)
            {
                return NotFound();
            }

            // Eğer slug boşsa, oluştur ve kaydet
            if (string.IsNullOrEmpty(tip.Slug))
            {
                tip.Slug = GenerateSlug(tip.Title);
                await _context.SaveChangesAsync();
            }

            // Slug tabanlı URL'ye yönlendir
            return RedirectToAction("DetailsBySlug", new { slug = tip.Slug });
        }

        // Yeni slug tabanlı Details metodu
        [HttpGet("stil-onerileri/{slug}")]
        public async Task<IActionResult> DetailsBySlug(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }

            var tip = await _context.StyleTips
                .Include(t => t.CategoryObject)
                .FirstOrDefaultAsync(t => t.Slug == slug);

            if (tip == null)
            {
                return NotFound();
            }

            return View("Details", tip);
        }

        public async Task<IActionResult> Featured()
        {
            var featuredTips = await _context.StyleTips
                .Include(t => t.CategoryObject)
                .Where(t => t.IsFeatured)
                .OrderByDescending(t => t.PublishDate)
                .Take(5)
                .ToListAsync();
            return View("Index", featuredTips);
        }

        public async Task<IActionResult> Category(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return RedirectToAction("Index");
            }

            // Önce kategoriyi bul
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Slug == slug);

            if (category == null)
            {
                return NotFound();
            }

            // Kategoriye ait stil ipuçlarını getir
            var tips = await _context.StyleTips
                .Include(t => t.CategoryObject)
                .Where(t => t.CategoryId == category.Id || t.Category == category.Title)
                .OrderByDescending(t => t.PublishDate)
                .ToListAsync();

            ViewBag.CategoryName = category.Title;
            return View("Index", tips);
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
