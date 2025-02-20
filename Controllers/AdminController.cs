using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gentlemen.Data;
using Gentlemen.Models;
using Gentlemen.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Gentlemen.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ApplicationDbContext context, IFileUploadService fileUploadService, ILogger<AdminController> logger)
        {
            _context = context;
            _fileUploadService = fileUploadService;
            _logger = logger;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(AdminLoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Sabit admin bilgileri
                if (model.Username == "centilmen01" && model.Password == "centilmen01")
                {
                    // Session'a admin bilgisini kaydet
                    HttpContext.Session.SetString("AdminUser", model.Username);
                    return RedirectToAction("Dashboard");
                }
                
                ModelState.AddModelError("", "Geçersiz kullanıcı adı veya şifre.");
            }
            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("AdminUser") != null;
        }

        public IActionResult Dashboard()
        {
            if (!IsAdmin())
                return RedirectToAction("Login");

            ViewBag.BlogCount = _context.Blogs.Count();
            ViewBag.OutfitCount = _context.Outfits.Count();
            ViewBag.StyleTipCount = _context.StyleTips.Count();
            
            return View();
        }

        public IActionResult Blogs()
        {
            if (!IsAdmin())
                return RedirectToAction("Login");

            var blogs = _context.Blogs.OrderByDescending(b => b.PublishDate).ToList();
            return View(blogs);
        }

        public async Task<IActionResult> Outfits()
        {
            if (!IsAdmin())
                return RedirectToAction("Login");

            var outfits = await _context.Outfits
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            // Sayfa yüklendiğinde kaç kombin olduğunu logla
            _logger.LogInformation($"Toplam {outfits.Count} kombin listeleniyor.");

            return View(outfits);
        }

        [HttpPost]
        public async Task<IActionResult> AddOutfit([FromForm] Outfit outfit, IFormFile Image)
        {
            if (!IsAdmin())
                return Json(new { success = false, message = "Yetkisiz erişim." });

            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join("; ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    return Json(new { success = false, message = $"Geçersiz veri: {errors}" });
                }

                if (Image == null || Image.Length == 0)
                {
                    return Json(new { success = false, message = "Lütfen bir görsel seçin." });
                }

                // Görsel yükleme
                try
                {
                    string imageUrl = await _fileUploadService.UploadFileAsync(Image);
                    outfit.ImageUrl = imageUrl;
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = $"Görsel yükleme hatası: {ex.Message}" });
                }

                // Kombin bilgilerini ayarla
                outfit.CreatedAt = DateTime.Now;
                outfit.Likes = 0;
                outfit.IsFeatured = false;

                // Veritabanına kaydet
                try
                {
                    _context.Outfits.Add(outfit);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Yeni kombin eklendi: {outfit.Title}");
                    return Json(new { success = true, message = "Kombin başarıyla eklendi." });
                }
                catch (Exception ex)
                {
                    // Görsel yüklendiyse sil
                    if (!string.IsNullOrEmpty(outfit.ImageUrl))
                    {
                        _fileUploadService.DeleteFile(outfit.ImageUrl);
                    }

                    _logger.LogError($"Veritabanı hatası: {ex.Message}");
                    return Json(new { success = false, message = "Veritabanı hatası oluştu." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Beklenmeyen hata: {ex.Message}");
                return Json(new { success = false, message = "Beklenmeyen bir hata oluştu." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteOutfit(int id)
        {
            if (!IsAdmin())
                return Json(new { success = false, message = "Yetkisiz erişim." });

            var outfit = await _context.Outfits.FindAsync(id);
            if (outfit == null)
                return Json(new { success = false, message = "Kombin bulunamadı." });

            if (!string.IsNullOrEmpty(outfit.ImageUrl))
            {
                _fileUploadService.DeleteFile(outfit.ImageUrl);
            }

            _context.Outfits.Remove(outfit);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        public IActionResult StyleTips()
        {
            if (!IsAdmin())
                return RedirectToAction("Login");

            var styleTips = _context.StyleTips.OrderByDescending(s => s.PublishDate).ToList();
            return View(styleTips);
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (!IsAdmin())
                return Json(new { success = false, message = "Yetkisiz erişim." });

            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Dosya seçilmedi." });

            try
            {
                string filePath = await _fileUploadService.UploadFileAsync(file);
                return Json(new { success = true, filePath = filePath });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult DeleteImage(string imagePath)
        {
            if (!IsAdmin())
                return Json(new { success = false, message = "Yetkisiz erişim." });

            try
            {
                _fileUploadService.DeleteFile(imagePath);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult DeleteBlog(int id)
        {
            if (!IsAdmin())
                return Json(new { success = false, message = "Yetkisiz erişim." });

            var blog = _context.Blogs.Find(id);
            if (blog != null)
            {
                if (!string.IsNullOrEmpty(blog.ImageUrl))
                {
                    _fileUploadService.DeleteFile(blog.ImageUrl);
                }
                _context.Blogs.Remove(blog);
                _context.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Blog bulunamadı." });
        }

        [HttpPost]
        public IActionResult DeleteStyleTip(int id)
        {
            if (!IsAdmin())
                return Json(new { success = false, message = "Yetkisiz erişim." });

            var styleTip = _context.StyleTips.Find(id);
            if (styleTip != null)
            {
                if (!string.IsNullOrEmpty(styleTip.ImageUrl))
                {
                    _fileUploadService.DeleteFile(styleTip.ImageUrl);
                }
                _context.StyleTips.Remove(styleTip);
                _context.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Stil ipucu bulunamadı." });
        }
    }
} 