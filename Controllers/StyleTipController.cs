using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gentlemen.Data;
using Gentlemen.Models;
using Microsoft.Extensions.Logging;

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

        public async Task<IActionResult> Details(int id)
        {
            var tip = await _context.StyleTips
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tip == null)
            {
                return NotFound();
            }

            return View(tip);
        }

        [HttpPost]
        public async Task<IActionResult> Like(int id)
        {
            var tip = await _context.StyleTips.FindAsync(id);
            if (tip == null)
            {
                return Json(new { success = false });
            }

            tip.Likes++;
            await _context.SaveChangesAsync();
            return Json(new { success = true, likes = tip.Likes });
        }

        public async Task<IActionResult> Featured()
        {
            var featuredTips = await _context.StyleTips
                .Where(t => t.IsFeatured)
                .OrderByDescending(t => t.PublishDate)
                .Take(5)
                .ToListAsync();
            return View("Index", featuredTips);
        }

        public async Task<IActionResult> Category(string category)
        {
            var tips = await _context.StyleTips
                .Where(t => t.Category == category)
                .OrderByDescending(t => t.PublishDate)
                .ToListAsync();
            return View("Index", tips);
        }
    }
} 