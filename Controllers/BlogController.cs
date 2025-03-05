using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gentlemen.Data;
using Gentlemen.Models;

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

            blog.ViewCount++;
            await _context.SaveChangesAsync();

            return View(blog);
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
                _context.Add(blog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(blog);
        }
    }
}
