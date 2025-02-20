using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gentlemen.Data;
using Gentlemen.Models;
using Microsoft.Extensions.Logging;

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

        public async Task<IActionResult> Details(int id)
        {
            var outfit = await _context.Outfits
                .FirstOrDefaultAsync(o => o.Id == id);

            if (outfit == null)
            {
                return NotFound();
            }

            return View(outfit);
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

        [HttpPost]
        public async Task<IActionResult> Like(int id)
        {
            var outfit = await _context.Outfits.FindAsync(id);
            if (outfit == null)
            {
                return Json(new { success = false });
            }

            outfit.Likes++;
            await _context.SaveChangesAsync();
            return Json(new { success = true, likes = outfit.Likes });
        }
    }
} 