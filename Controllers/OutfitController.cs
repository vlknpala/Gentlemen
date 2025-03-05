using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gentlemen.Data;
using Gentlemen.Models;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Gentlemen.Controllers
{
    public class OutfitController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OutfitController> _logger;

        public OutfitController(ApplicationDbContext context, ILogger<OutfitController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var outfits = await _context.Outfits
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                _logger.LogInformation($"Toplam {outfits.Count} kombin listeleniyor.");
                return View(outfits);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Kombinler listelenirken hata oluştu: {ex.Message}");
                return View(new List<Outfit>());
            }
        }

        // Eski ID tabanlı Details metodu - yönlendirme için kullanılacak
        [HttpGet("Outfit/Details/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var outfit = await _context.Outfits
                .FirstOrDefaultAsync(o => o.Id == id);

            if (outfit == null)
            {
                return NotFound();
            }

            // Eğer slug boşsa, oluştur ve kaydet
            if (string.IsNullOrEmpty(outfit.Slug))
            {
                outfit.Slug = GenerateSlug(outfit.Title);
                await _context.SaveChangesAsync();
            }

            // Slug tabanlı URL'ye yönlendir
            return RedirectToAction("DetailsBySlug", new { slug = outfit.Slug });
        }

        // Yeni slug tabanlı Details metodu
        [HttpGet("kombinler/{slug}")]
        public async Task<IActionResult> DetailsBySlug(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }

            var outfit = await _context.Outfits
                .FirstOrDefaultAsync(o => o.Slug == slug);

            if (outfit == null)
            {
                return NotFound();
            }

            // Get related outfits (same season or style, but not the same outfit)
            var relatedOutfits = await _context.Outfits
                .Where(o => o.Id != outfit.Id && (o.Season == outfit.Season || o.Style == outfit.Style))
                .OrderByDescending(o => o.CreatedAt)
                .Take(3)
                .ToListAsync();

            ViewBag.RelatedOutfits = relatedOutfits;

            return View("Details", outfit);
        }

        [HttpGet]
        public IActionResult Create()
        {
            // Admin kontrolü
            if (HttpContext.Session.GetString("AdminUser") == null)
            {
                return RedirectToAction("Login", "Admin");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Outfit outfit)
        {
            // Admin kontrolü
            if (HttpContext.Session.GetString("AdminUser") == null)
            {
                return RedirectToAction("Login", "Admin");
            }

            if (ModelState.IsValid)
            {
                outfit.CreatedAt = DateTime.Now;

                // Slug oluştur
                if (string.IsNullOrEmpty(outfit.Slug))
                {
                    outfit.Slug = GenerateSlug(outfit.Title);
                }

                _context.Add(outfit);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(outfit);
        }

        public async Task<IActionResult> Season(string season)
        {
            var outfits = await _context.Outfits
                .Where(o => o.Season == season)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
            return View("Index", outfits);
        }

        public async Task<IActionResult> Style(string style)
        {
            var outfits = await _context.Outfits
                .Where(o => o.Style == style)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
            return View("Index", outfits);
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
