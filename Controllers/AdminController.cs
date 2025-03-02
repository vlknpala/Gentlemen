using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gentlemen.Data;
using Gentlemen.Models;
using Gentlemen.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace Gentlemen.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<AdminController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminController(ApplicationDbContext context, IFileUploadService fileUploadService, ILogger<AdminController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _fileUploadService = fileUploadService;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
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
                if (model.Username == "admin" && model.Password == "123123")
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
            var adminUser = HttpContext.Session.GetString("AdminUser");
            return adminUser == "admin";
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

        public IActionResult CreateBlog()
        {
            if (!IsAdmin())
                return RedirectToAction("Login");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBlog(Blog blog, IFormFile Image)
        {
            if (!IsAdmin())
                return RedirectToAction("Login");

            if (ModelState.IsValid)
            {
                try
                {
                    if (Image != null && Image.Length > 0)
                    {
                        string imageUrl = await _fileUploadService.UploadFileAsync(Image);
                        blog.ImageUrl = imageUrl;
                    }

                    blog.PublishDate = DateTime.Now;
                    blog.ViewCount = 0;

                    _context.Blogs.Add(blog);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Blogs));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Blog kaydedilirken bir hata oluştu: " + ex.Message);
                }
            }
            return View(blog);
        }

        public async Task<IActionResult> EditBlog(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login");

            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null)
                return NotFound();

            return View(blog);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBlog(int id, Blog blog, IFormFile Image)
        {
            if (!IsAdmin())
                return RedirectToAction("Login");

            if (id != blog.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingBlog = await _context.Blogs.FindAsync(id);
                    if (existingBlog == null)
                        return NotFound();

                    if (Image != null && Image.Length > 0)
                    {
                        // Delete old image if exists
                        if (!string.IsNullOrEmpty(existingBlog.ImageUrl))
                        {
                            _fileUploadService.DeleteFile(existingBlog.ImageUrl);
                        }

                        string imageUrl = await _fileUploadService.UploadFileAsync(Image);
                        existingBlog.ImageUrl = imageUrl;
                    }

                    existingBlog.Title = blog.Title;
                    existingBlog.Content = blog.Content;
                    existingBlog.Category = blog.Category;
                    existingBlog.Author = blog.Author;

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Blogs));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Blog güncellenirken bir hata oluştu: " + ex.Message);
                }
            }
            return View(blog);
        }

        [HttpPost]
        public async Task<IActionResult> AddStyleTip([FromForm] StyleTip styleTip, IFormFile Image)
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
                    styleTip.ImageUrl = imageUrl;
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = $"Görsel yükleme hatası: {ex.Message}" });
                }

                // Stil ipucu bilgilerini ayarla
                styleTip.PublishDate = DateTime.Now;
                styleTip.Likes = 0;

                // Veritabanına kaydet
                try
                {
                    _context.StyleTips.Add(styleTip);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Yeni stil ipucu eklendi: {styleTip.Title}");
                    return Json(new { success = true, message = "Stil ipucu başarıyla eklendi." });
                }
                catch (Exception ex)
                {
                    // Görsel yüklendiyse sil
                    if (!string.IsNullOrEmpty(styleTip.ImageUrl))
                    {
                        _fileUploadService.DeleteFile(styleTip.ImageUrl);
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
        public async Task<IActionResult> ToggleStyleTipFeatured(int id, bool isFeatured)
        {
            if (!IsAdmin())
                return Json(new { success = false, message = "Yetkisiz erişim." });

            try
            {
                var styleTip = await _context.StyleTips.FindAsync(id);
                if (styleTip == null)
                    return Json(new { success = false, message = "Stil ipucu bulunamadı." });

                styleTip.IsFeatured = isFeatured;
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Stil ipucu öne çıkarma hatası: {ex.Message}");
                return Json(new { success = false, message = "İşlem sırasında bir hata oluştu." });
            }
        }

        public async Task<IActionResult> EditStyleTip(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login");

            var styleTip = await _context.StyleTips.FindAsync(id);
            if (styleTip == null)
                return NotFound();

            return View(styleTip);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStyleTip(int id, StyleTip styleTip, IFormFile Image)
        {
            if (!IsAdmin())
                return RedirectToAction("Login");

            if (id != styleTip.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingStyleTip = await _context.StyleTips.FindAsync(id);
                    if (existingStyleTip == null)
                        return NotFound();

                    if (Image != null && Image.Length > 0)
                    {
                        // Delete old image if exists
                        if (!string.IsNullOrEmpty(existingStyleTip.ImageUrl))
                        {
                            _fileUploadService.DeleteFile(existingStyleTip.ImageUrl);
                        }

                        string imageUrl = await _fileUploadService.UploadFileAsync(Image);
                        existingStyleTip.ImageUrl = imageUrl;
                    }

                    existingStyleTip.Title = styleTip.Title;
                    existingStyleTip.Content = styleTip.Content;
                    existingStyleTip.Category = styleTip.Category;
                    existingStyleTip.Author = styleTip.Author;
                    existingStyleTip.IsFeatured = styleTip.IsFeatured;

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(StyleTips));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Stil ipucu güncellenirken bir hata oluştu: " + ex.Message);
                }
            }
            return View(styleTip);
        }

        // Categories
        public async Task<IActionResult> Categories()
        {
            if (!IsAdmin())
                return RedirectToAction("Login");

            try 
            {
                var categories = await _context.Categories
                    .OrderByDescending(c => c.Id)
                    .ToListAsync();
                    
                return View(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Kategoriler listelenirken hata: {ex.Message}");
                TempData["ErrorMessage"] = "Kategoriler listelenirken bir hata oluştu.";
                return View(new List<Category>());
            }
        }

        public async Task<IActionResult> CategoryContents(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Contents)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpGet]
        public IActionResult CreateCategoryContent(int categoryId)
        {
            ViewBag.CategoryId = categoryId;
            return View(new CategoryContent { CategoryId = categoryId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategoryContent(CategoryContent content, IFormFile? Image)
        {
            if (ModelState.IsValid)
            {
                if (Image != null)
                {
                    content.ImageUrl = await _fileUploadService.UploadFileAsync(Image, "category-contents");
                }

                content.CreatedAt = DateTime.Now;
                _context.CategoryContents.Add(content);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(CategoryContents), new { id = content.CategoryId });
            }

            ViewBag.CategoryId = content.CategoryId;
            return View(content);
        }

        [HttpGet]
        public async Task<IActionResult> EditCategoryContent(int id)
        {
            var content = await _context.CategoryContents.FindAsync(id);
            if (content == null)
            {
                return NotFound();
            }

            return View(content);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategoryContent(CategoryContent content, IFormFile? Image)
        {
            if (ModelState.IsValid)
            {
                if (Image != null)
                {
                    content.ImageUrl = await _fileUploadService.UploadFileAsync(Image, "category-contents");
                }

                _context.Update(content);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(CategoryContents), new { id = content.CategoryId });
            }

            return View(content);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategoryContent(int id)
        {
            var content = await _context.CategoryContents.FindAsync(id);
            if (content == null)
            {
                return Json(new { success = false, message = "İçerik bulunamadı." });
            }

            _context.CategoryContents.Remove(content);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpGet]
        public IActionResult CreateCategory()
        {
            if (!IsAdmin())
                return RedirectToAction("Login");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(Category category, IFormFile? Image)
        {
            if (!IsAdmin())
            {
                _logger.LogWarning("Yetkisiz erişim denemesi - CreateCategory action");
                return RedirectToAction("Login");
            }

            try
            {
                if (!ModelState.IsValid)
                {
                    foreach (var modelError in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        _logger.LogError($"Model Validation Error: {modelError.ErrorMessage}");
                    }
                    return View(category);
                }

                _logger.LogInformation("Kategori ekleme işlemi başladı");

                if (Image == null)
                {
                    _logger.LogError("Görsel yüklenmedi");
                    ModelState.AddModelError("Image", "Lütfen bir görsel seçin.");
                    return View(category);
                }

                _logger.LogInformation($"Görsel boyutu: {Image.Length} bytes");
                _logger.LogInformation($"Görsel tipi: {Image.ContentType}");

                try
                {
                    category.ImageUrl = await _fileUploadService.UploadFileAsync(Image, "categories");
                    _logger.LogInformation($"Görsel başarıyla yüklendi: {category.ImageUrl}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Görsel yükleme hatası: {ex.Message}");
                    ModelState.AddModelError("Image", $"Görsel yüklenirken hata oluştu: {ex.Message}");
                    return View(category);
                }

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Kategori başarıyla eklendi. ID: {category.Id}");
                TempData["SuccessMessage"] = "Kategori başarıyla eklendi.";
                return RedirectToAction(nameof(Categories));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Kategori eklenirken hata: {ex.Message}");
                ModelState.AddModelError("", $"Kategori eklenirken bir hata oluştu: {ex.Message}");
                return View(category);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditCategory(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login");

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(Category category, IFormFile? Image)
        {
            if (!IsAdmin())
                return RedirectToAction("Login");

            try
            {
                if (ModelState.IsValid)
                {
                    if (Image != null)
                    {
                        // Eski görseli sil
                        if (!string.IsNullOrEmpty(category.ImageUrl))
                        {
                            _fileUploadService.DeleteFile(category.ImageUrl);
                        }
                        // Yeni görseli yükle
                        category.ImageUrl = await _fileUploadService.UploadFileAsync(Image, "categories");
                    }

                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Kategori başarıyla güncellendi.";
                    return RedirectToAction(nameof(Categories));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Kategori güncellenirken hata: {ex.Message}");
                ModelState.AddModelError("", "Kategori güncellenirken bir hata oluştu.");
            }
            
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login");

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            try
            {
                // Kategori görselini sil
                if (!string.IsNullOrEmpty(category.ImageUrl))
                {
                    _fileUploadService.DeleteFile(category.ImageUrl);
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Kategori başarıyla silindi.";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Kategori silinirken hata: {ex.Message}");
                TempData["ErrorMessage"] = "Kategori silinirken bir hata oluştu.";
            }

            return RedirectToAction(nameof(Categories));
        }
    }
}
