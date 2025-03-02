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
        // Get featured categories
        ViewBag.FeaturedCategories = await _context.FeaturedCategories
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();

        // Get latest style tips
        ViewBag.LatestStyleTips = await _context.StyleTips
            .Where(s => s.IsFeatured)
            .OrderByDescending(s => s.PublishDate)
            .Take(3)
            .ToListAsync();

        // Get featured outfits
        ViewBag.FeaturedOutfits = await _context.Outfits
            .Where(o => o.IsFeatured)
            .OrderByDescending(o => o.CreatedAt)
            .Take(4)
            .ToListAsync();

        // Get latest blog posts
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
