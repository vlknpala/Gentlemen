using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gentlemen.Data;
using Gentlemen.Models;

namespace Gentlemen.Controllers
{
    public class StyleTipController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StyleTipController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var tips = await _context.StyleTips
                .OrderByDescending(t => t.PublishDate)
                .ToListAsync();
            return View(tips);
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

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StyleTip tip)
        {
            if (ModelState.IsValid)
            {
                tip.PublishDate = DateTime.Now;
                tip.Likes = 0;
                _context.Add(tip);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
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