using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Gentlemen.Models;
using Gentlemen.Data;
using Microsoft.EntityFrameworkCore;

namespace Gentlemen.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _context.Categories
            .ToListAsync();

        ViewBag.Categories = categories;

        // Son eklenen stil ipuçlarını al
        ViewBag.LatestStyleTips = await _context.StyleTips
            .OrderByDescending(t => t.PublishDate)
            .Take(3)
            .ToListAsync();

        // Öne çıkan kombinleri al
        ViewBag.FeaturedOutfits = await _context.Outfits
            .Where(o => o.IsFeatured)
            .OrderByDescending(o => o.CreatedAt)
            .Take(2)
            .ToListAsync();

        // Son blog yazılarını al
        ViewBag.LatestBlogPosts = await _context.Blogs
            .OrderByDescending(b => b.PublishDate)
            .Take(3)
            .ToListAsync();

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
