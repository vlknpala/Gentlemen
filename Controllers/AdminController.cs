using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gentlemen.Data;
using Gentlemen.Models;
using Gentlemen.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public IActionResult StyleTips()
        {
            if (!IsAdmin())
                return RedirectToAction("Login");

            var styleTips = _context.StyleTips
                .Include(s => s.CategoryObject)
                .OrderByDescending(s => s.PublishDate)
                .ToList();

            // Kategorileri ViewBag'e ekle
            ViewBag.Categories = _context.Categories.Where(c => c.IsActive).ToList();

            // Mevcut stil ipuçlarının CategoryId'lerini kontrol et ve güncelle
            foreach (var tip in styleTips)
            {
                if (tip.CategoryId == null && !string.IsNullOrEmpty(tip.Category))
                {
                    // Kategori adına göre kategori bul
                    var category = _context.Categories.FirstOrDefault(c => c.Title == tip.Category);
                    if (category != null)
                    {
                        tip.CategoryId = category.Id;
                        _context.Update(tip);
                    }
                }
            }

            if (_context.ChangeTracker.HasChanges())
            {
                _context.SaveChanges();
            }

            return View(styleTips);
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
                    // IsFeatured için olan hata mesajını yoksay
                    if (ModelState["IsFeatured"] != null && ModelState["IsFeatured"].Errors.Count > 0)
                    {
                        ModelState["IsFeatured"].Errors.Clear();
                    }

                    // Hala hata varsa
                if (!ModelState.IsValid)
                {
                    var errors = string.Join("; ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    return Json(new { success = false, message = $"Geçersiz veri: {errors}" });
                    }
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

                // Slug oluştur
                if (string.IsNullOrEmpty(outfit.Slug))
                {
                    outfit.Slug = GenerateSlug(outfit.Title);
                }
                else
                {
                    outfit.Slug = GenerateSlug(outfit.Slug);
                }

                // IsFeatured değerini form verilerinden manuel olarak al
                outfit.IsFeatured = Request.Form["IsFeatured"] == "on";

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
        public async Task<IActionResult> ToggleOutfitFeatured(int id, bool isFeatured)
        {
            if (!IsAdmin())
                return Json(new { success = false, message = "Yetkisiz erişim." });

            try
            {
                var outfit = await _context.Outfits.FindAsync(id);
                if (outfit == null)
                    return Json(new { success = false, message = "Kombin bulunamadı." });

                outfit.IsFeatured = isFeatured;
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Kombin öne çıkarma hatası: {ex.Message}");
                return Json(new { success = false, message = "İşlem sırasında bir hata oluştu." });
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

        [HttpGet]
        public async Task<IActionResult> GetOutfit(int id)
        {
            if (!IsAdmin())
                return Json(new { success = false, message = "Yetkisiz erişim." });

            var outfit = await _context.Outfits.FindAsync(id);
            if (outfit == null)
                return Json(new { success = false, message = "Kombin bulunamadı." });

            return Json(new { success = true, outfit = outfit });
        }

        [HttpPost]
        public async Task<IActionResult> EditOutfit([FromForm] Outfit outfit, IFormFile? Image)
        {
            if (!IsAdmin())
                return Json(new { success = false, message = "Yetkisiz erişim." });

            try
            {
                // IsFeatured için olan hata mesajını yoksay
                if (ModelState["IsFeatured"] != null && ModelState["IsFeatured"].Errors.Count > 0)
                {
                    ModelState["IsFeatured"].Errors.Clear();
                }

                var existingOutfit = await _context.Outfits.FindAsync(outfit.Id);
                if (existingOutfit == null)
                    return Json(new { success = false, message = "Kombin bulunamadı." });

                // Mevcut görsel URL'sini sakla
                string oldImageUrl = existingOutfit.ImageUrl;

                // Yeni görsel yüklendiyse
                if (Image != null && Image.Length > 0)
                {
                    try
                    {
                        string imageUrl = await _fileUploadService.UploadFileAsync(Image);
                        outfit.ImageUrl = imageUrl;

                        // Eski görseli sil
                        if (!string.IsNullOrEmpty(oldImageUrl))
                        {
                            _fileUploadService.DeleteFile(oldImageUrl);
                        }
                    }
                    catch (Exception ex)
                    {
                        return Json(new { success = false, message = $"Görsel yükleme hatası: {ex.Message}" });
                    }
                }
                else
                {
                    // Görsel değiştirilmediyse mevcut görseli kullan
                    outfit.ImageUrl = oldImageUrl;
                }

                // Değiştirilmeyen alanları güncelle
                outfit.CreatedAt = existingOutfit.CreatedAt;
                outfit.Likes = existingOutfit.Likes;

                // Slug kontrolü
                if (string.IsNullOrWhiteSpace(outfit.Slug))
                {
                    outfit.Slug = GenerateSlug(outfit.Title);
                }
                else
                {
                    outfit.Slug = GenerateSlug(outfit.Slug);
                }

                // IsFeatured değerini form verilerinden manuel olarak al
                outfit.IsFeatured = Request.Form["IsFeatured"] == "on";

                // Veritabanını güncelle
                _context.Entry(existingOutfit).State = EntityState.Detached;
                _context.Entry(outfit).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Kombin güncellendi: {outfit.Title}");
                return Json(new { success = true, message = "Kombin başarıyla güncellendi." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Kombin güncelleme hatası: {ex.Message}");
                return Json(new { success = false, message = "Beklenmeyen bir hata oluştu." });
            }
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
        public async Task<IActionResult> UploadMultipleImages(List<IFormFile> files)
        {
            if (!IsAdmin())
                return Json(new { success = false, message = "Yetkisiz erişim." });

            if (files == null || !files.Any())
                return Json(new { success = false, message = "Dosya seçilmedi." });

            try
            {
                List<string> filePaths = await _fileUploadService.UploadMultipleFilesAsync(files);
                return Json(new { success = true, filePaths = filePaths });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveImagesToEntity(string entityType, int entityId, List<string> imagePaths, List<bool> isMainList = null)
        {
            if (!IsAdmin())
                return Json(new { success = false, message = "Yetkisiz erişim." });

            if (string.IsNullOrEmpty(entityType) || entityId <= 0 || imagePaths == null || !imagePaths.Any())
                return Json(new { success = false, message = "Geçersiz parametreler." });

            try
            {
                // Mevcut ana görseli bul
                Image existingMainImage = null;
                
                switch (entityType.ToLower())
                {
                    case "outfit":
                        var outfit = await _context.Outfits.Include(o => o.Images).FirstOrDefaultAsync(o => o.Id == entityId);
                        if (outfit == null)
                            return Json(new { success = false, message = "Kombin bulunamadı." });
                        
                        if (outfit.Images == null)
                            outfit.Images = new List<Image>();
                        
                        existingMainImage = outfit.Images.FirstOrDefault(i => i.IsMain);
                        
                        for (int i = 0; i < imagePaths.Count; i++)
                        {
                            bool isMain = isMainList != null && i < isMainList.Count && isMainList[i];
                            
                            // Eğer bu görsel ana görsel olarak işaretlendiyse ve zaten bir ana görsel varsa
                            if (isMain && existingMainImage != null)
                            {
                                existingMainImage.IsMain = false;
                                existingMainImage = null;
                            }
                            
                            var image = new Image
                            {
                                ImageUrl = imagePaths[i],
                                OutfitId = entityId,
                                IsMain = isMain,
                                DisplayOrder = outfit.Images.Count + i
                            };
                            
                            outfit.Images.Add(image);
                            
                            // İlk görsel veya ana görsel olarak işaretlenen görsel, ana ImageUrl olarak da ayarlanır
                            if (isMain || outfit.Images.Count == 1)
                            {
                                outfit.ImageUrl = imagePaths[i];
                            }
                        }
                        break;
                        
                    case "blog":
                        var blog = await _context.Blogs.Include(b => b.Images).FirstOrDefaultAsync(b => b.Id == entityId);
                        if (blog == null)
                            return Json(new { success = false, message = "Blog yazısı bulunamadı." });
                        
                        if (blog.Images == null)
                            blog.Images = new List<Image>();
                        
                        existingMainImage = blog.Images.FirstOrDefault(i => i.IsMain);
                        
                        for (int i = 0; i < imagePaths.Count; i++)
                        {
                            bool isMain = isMainList != null && i < isMainList.Count && isMainList[i];
                            
                            if (isMain && existingMainImage != null)
                            {
                                existingMainImage.IsMain = false;
                                existingMainImage = null;
                            }
                            
                            var image = new Image
                            {
                                ImageUrl = imagePaths[i],
                                BlogId = entityId,
                                IsMain = isMain,
                                DisplayOrder = blog.Images.Count + i
                            };
                            
                            blog.Images.Add(image);
                            
                            if (isMain || blog.Images.Count == 1)
                            {
                                blog.ImageUrl = imagePaths[i];
                            }
                        }
                        break;
                        
                    case "styletip":
                        var styleTip = await _context.StyleTips.Include(s => s.Images).FirstOrDefaultAsync(s => s.Id == entityId);
                        if (styleTip == null)
                            return Json(new { success = false, message = "Stil ipucu bulunamadı." });
                        
                        if (styleTip.Images == null)
                            styleTip.Images = new List<Image>();
                        
                        existingMainImage = styleTip.Images.FirstOrDefault(i => i.IsMain);
                        
                        for (int i = 0; i < imagePaths.Count; i++)
                        {
                            bool isMain = isMainList != null && i < isMainList.Count && isMainList[i];
                            
                            if (isMain && existingMainImage != null)
                            {
                                existingMainImage.IsMain = false;
                                existingMainImage = null;
                            }
                            
                            var image = new Image
                            {
                                ImageUrl = imagePaths[i],
                                StyleTipId = entityId,
                                IsMain = isMain,
                                DisplayOrder = styleTip.Images.Count + i
                            };
                            
                            styleTip.Images.Add(image);
                            
                            if (isMain || styleTip.Images.Count == 1)
                            {
                                styleTip.ImageUrl = imagePaths[i];
                            }
                        }
                        break;
                        
                    default:
                        return Json(new { success = false, message = "Geçersiz entity tipi." });
                }
                
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Görseller başarıyla kaydedildi." });
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
        public async Task<IActionResult> DeleteEntityImage(int imageId)
        {
            if (!IsAdmin())
                return Json(new { success = false, message = "Yetkisiz erişim." });

            try
            {
                var image = await _context.Images.FindAsync(imageId);
                if (image == null)
                    return Json(new { success = false, message = "Görsel bulunamadı." });

                // Dosya sisteminden sil
                _fileUploadService.DeleteFile(image.ImageUrl);

                // Ana görsel mi kontrol et
                bool isMain = image.IsMain;
                string entityType = null;
                int entityId = 0;

                if (image.OutfitId.HasValue)
                {
                    entityType = "outfit";
                    entityId = image.OutfitId.Value;
                }
                else if (image.BlogId.HasValue)
                {
                    entityType = "blog";
                    entityId = image.BlogId.Value;
                }
                else if (image.StyleTipId.HasValue)
                {
                    entityType = "styletip";
                    entityId = image.StyleTipId.Value;
                }

                // Veritabanından sil
                _context.Images.Remove(image);

                // Eğer silinen görsel ana görsel ise, başka bir görseli ana görsel olarak ayarla
                if (isMain && !string.IsNullOrEmpty(entityType))
                {
                    Image newMainImage = null;
                    
                    switch (entityType)
                    {
                        case "outfit":
                            var outfit = await _context.Outfits.Include(o => o.Images).FirstOrDefaultAsync(o => o.Id == entityId);
                            if (outfit != null && outfit.Images != null && outfit.Images.Any())
                            {
                                newMainImage = outfit.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault();
                                if (newMainImage != null)
                                {
                                    newMainImage.IsMain = true;
                                    outfit.ImageUrl = newMainImage.ImageUrl;
                                }
                                else
                                {
                                    outfit.ImageUrl = "";
                                }
                            }
                            break;
                            
                        case "blog":
                            var blog = await _context.Blogs.Include(b => b.Images).FirstOrDefaultAsync(b => b.Id == entityId);
                            if (blog != null && blog.Images != null && blog.Images.Any())
                            {
                                newMainImage = blog.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault();
                                if (newMainImage != null)
                                {
                                    newMainImage.IsMain = true;
                                    blog.ImageUrl = newMainImage.ImageUrl;
                                }
                                else
                                {
                                    blog.ImageUrl = null;
                                }
                            }
                            break;
                            
                        case "styletip":
                            var styleTip = await _context.StyleTips.Include(s => s.Images).FirstOrDefaultAsync(s => s.Id == entityId);
                            if (styleTip != null && styleTip.Images != null && styleTip.Images.Any())
                            {
                                newMainImage = styleTip.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault();
                                if (newMainImage != null)
                                {
                                    newMainImage.IsMain = true;
                                    styleTip.ImageUrl = newMainImage.ImageUrl;
                                }
                                else
                                {
                                    styleTip.ImageUrl = null;
                                }
                            }
                            break;
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Görsel başarıyla silindi." });
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

                    // Generate slug if not provided
                    if (string.IsNullOrEmpty(blog.Slug))
                    {
                        blog.Slug = GenerateSlug(blog.Title);
                    }
                    else
                    {
                        blog.Slug = GenerateSlug(blog.Slug);
                    }

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

                    // Generate slug if not provided
                    if (string.IsNullOrEmpty(blog.Slug))
                    {
                        existingBlog.Slug = GenerateSlug(blog.Title);
                    }
                    else
                    {
                        existingBlog.Slug = GenerateSlug(blog.Slug);
                    }

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
                // Görsel yükleme (geriye dönük uyumluluk için)
                if (Image != null)
                {
                    styleTip.ImageUrl = await _fileUploadService.UploadFileAsync(Image, "styletips");
                }

                // Slug oluştur
                if (string.IsNullOrEmpty(styleTip.Slug))
                {
                    styleTip.Slug = GenerateSlug(styleTip.Title);
                }

                // Yayın tarihi
                styleTip.PublishDate = DateTime.Now;

                // Kategori ilişkisi
                if (styleTip.CategoryId.HasValue)
                {
                    var category = await _context.Categories.FindAsync(styleTip.CategoryId);
                    if (category != null)
                    {
                        styleTip.Category = category.Title;
                    }
                }

                    _context.StyleTips.Add(styleTip);
                    await _context.SaveChangesAsync();

                _logger.LogInformation($"Yeni stil ipucu eklendi: {styleTip.Title}");
                return Json(new { success = true, message = "Stil ipucu başarıyla eklendi.", styleTipId = styleTip.Id });
                }
                catch (Exception ex)
                {
                _logger.LogError($"Stil ipucu ekleme hatası: {ex.Message}");
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

            var styleTip = await _context.StyleTips
                .Include(s => s.CategoryObject)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (styleTip == null)
                return NotFound();

            // Kategorileri ViewBag'e ekle
            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();

            return View(styleTip);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStyleTip(int id, StyleTip styleTip, IFormFile Image)
        {
            if (!IsAdmin())
                return Json(new { success = false, message = "Yetkisiz erişim." });

                try
                {
                    var existingStyleTip = await _context.StyleTips.FindAsync(id);
                    if (existingStyleTip == null)
                {
                    return Json(new { success = false, message = "Stil ipucu bulunamadı." });
                }

                // Mevcut görsel URL'sini sakla
                string oldImageUrl = existingStyleTip.ImageUrl ?? "";

                // Yeni görsel yüklendiyse
                    if (Image != null && Image.Length > 0)
                    {
                    try
                    {
                        // Eski görseli sil (eğer varsa)
                        if (!string.IsNullOrEmpty(oldImageUrl))
                        {
                            _fileUploadService.DeleteFile(oldImageUrl);
                        }

                        // Yeni görseli yükle
                        styleTip.ImageUrl = await _fileUploadService.UploadFileAsync(Image, "styletips");
                    }
                    catch (Exception ex)
                    {
                        return Json(new { success = false, message = $"Görsel yükleme hatası: {ex.Message}" });
                    }
                }
                else
                {
                    // Görsel değiştirilmediyse mevcut görseli kullan
                    styleTip.ImageUrl = oldImageUrl;
                }

                // Slug kontrolü
                if (string.IsNullOrWhiteSpace(styleTip.Slug))
                {
                    styleTip.Slug = GenerateSlug(styleTip.Title);
                }

                // Kategori ilişkisi
                if (styleTip.CategoryId.HasValue)
                {
                    var category = await _context.Categories.FindAsync(styleTip.CategoryId);
                    if (category != null)
                    {
                        styleTip.Category = category.Title;
                    }
                }

                // Değiştirilmeyen alanları güncelle
                styleTip.PublishDate = existingStyleTip.PublishDate;
                styleTip.Likes = existingStyleTip.Likes;
                styleTip.CreatedAt = existingStyleTip.CreatedAt;

                // Entity'yi güncelle
                _context.Entry(existingStyleTip).CurrentValues.SetValues(styleTip);
                    await _context.SaveChangesAsync();

                _logger.LogInformation($"Stil ipucu güncellendi: {styleTip.Title}");
                return Json(new { success = true, message = "Stil ipucu başarıyla güncellendi." });
                }
                catch (Exception ex)
                {
                _logger.LogError($"Stil ipucu güncelleme hatası: {ex.Message}");
                return Json(new { success = false, message = "Beklenmeyen bir hata oluştu." });
            }
        }

        public async Task<IActionResult> Categories()
        {
            var categories = await _context.Categories.ToListAsync();
            return View(categories);
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory([FromForm] Category category, IFormFile Image)
        {
            if (Image != null)
            {
                var imageUrl = await _fileUploadService.UploadFileAsync(Image, "categories");
                category.ImageUrl = imageUrl;
            }

            category.Slug = category.Title.ToLower()
                .Replace(" ", "-")
                .Replace("ı", "i")
                .Replace("ğ", "g")
                .Replace("ü", "u")
                .Replace("ş", "s")
                .Replace("ö", "o")
                .Replace("ç", "c");

            category.CreatedAt = DateTime.Now;
            category.IsActive = true;

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Kategori başarıyla eklendi." });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCategory(int id, [FromForm] Category category, IFormFile Image)
        {
            var existingCategory = await _context.Categories.FindAsync(id);
            if (existingCategory == null)
            {
                return Json(new { success = false, message = "Kategori bulunamadı." });
            }

            if (Image != null)
            {
                var imageUrl = await _fileUploadService.UploadFileAsync(Image, "categories");
                existingCategory.ImageUrl = imageUrl;
            }

            existingCategory.Title = category.Title;
            existingCategory.Description = category.Description;
            existingCategory.IsActive = category.IsActive;
            existingCategory.Slug = category.Title.ToLower()
                .Replace(" ", "-")
                .Replace("ı", "i")
                .Replace("ğ", "g")
                .Replace("ü", "u")
                .Replace("ş", "s")
                .Replace("ö", "o")
                .Replace("ç", "c");

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Kategori başarıyla güncellendi." });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return Json(new { success = false, message = "Kategori bulunamadı." });
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Kategori başarıyla silindi." });
        }

        [HttpGet]
        public async Task<IActionResult> GetCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            return Json(category);
        }

        // Tüm stil ipuçları için slug oluştur
        [HttpGet("admin/generate-slugs")]
        public async Task<IActionResult> GenerateSlugsForStyleTips()
        {
            try
            {
                var tips = await _context.StyleTips.ToListAsync();
                int updatedCount = 0;

                foreach (var tip in tips)
                {
                    if (string.IsNullOrEmpty(tip.Slug))
                    {
                        tip.Slug = GenerateSlug(tip.Title);
                        updatedCount++;
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"{updatedCount} stil ipucu için slug oluşturuldu.";
                return RedirectToAction("StyleTips");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Slug oluşturulurken hata: {ex.Message}";
                return RedirectToAction("StyleTips");
            }
        }

        // Tüm kombinler için slug oluştur
        [HttpGet("admin/generate-outfit-slugs")]
        public async Task<IActionResult> GenerateSlugsForOutfits()
        {
            try
            {
                var outfits = await _context.Outfits.ToListAsync();
                int updatedCount = 0;

                foreach (var outfit in outfits)
                {
                    if (string.IsNullOrEmpty(outfit.Slug))
                    {
                        outfit.Slug = GenerateSlug(outfit.Title);
                        updatedCount++;
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"{updatedCount} kombin için slug oluşturuldu.";
                return RedirectToAction("Outfits");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Slug oluşturulurken hata: {ex.Message}";
                return RedirectToAction("Outfits");
            }
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

        [HttpGet("admin/generate-blog-slugs")]
        public async Task<IActionResult> GenerateSlugsForBlogs()
        {
            if (!IsAdmin())
                return RedirectToAction("Login");

            var blogs = await _context.Blogs.ToListAsync();
            int count = 0;

            foreach (var blog in blogs)
            {
                if (string.IsNullOrEmpty(blog.Slug))
                {
                    blog.Slug = GenerateSlug(blog.Title);
                    count++;
                }
            }

            await _context.SaveChangesAsync();

            TempData["Message"] = $"{count} blog yazısı için slug oluşturuldu.";
            return RedirectToAction("Blogs");
        }

        [HttpPost]
        public async Task<IActionResult> ReorderImages(List<int> imageIds)
        {
            if (!IsAdmin())
                return Json(new { success = false, message = "Yetkisiz erişim." });

            if (imageIds == null || !imageIds.Any())
                return Json(new { success = false, message = "Görsel listesi boş." });

            try
            {
                for (int i = 0; i < imageIds.Count; i++)
                {
                    var image = await _context.Images.FindAsync(imageIds[i]);
                    if (image != null)
                    {
                        image.DisplayOrder = i;
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Görsel sıralaması güncellendi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SetMainImage(int imageId)
        {
            if (!IsAdmin())
                return Json(new { success = false, message = "Yetkisiz erişim." });

            try
            {
                var image = await _context.Images.FindAsync(imageId);
                if (image == null)
                    return Json(new { success = false, message = "Görsel bulunamadı." });

                // Entity tipini ve ID'sini belirle
                string entityType = null;
                int entityId = 0;

                if (image.OutfitId.HasValue)
                {
                    entityType = "outfit";
                    entityId = image.OutfitId.Value;
                }
                else if (image.BlogId.HasValue)
                {
                    entityType = "blog";
                    entityId = image.BlogId.Value;
                }
                else if (image.StyleTipId.HasValue)
                {
                    entityType = "styletip";
                    entityId = image.StyleTipId.Value;
                }
                else
                {
                    return Json(new { success = false, message = "Görsel herhangi bir içerikle ilişkilendirilmemiş." });
                }

                // Mevcut ana görseli bul ve işaretini kaldır
                switch (entityType)
                {
                    case "outfit":
                        var outfit = await _context.Outfits.Include(o => o.Images).FirstOrDefaultAsync(o => o.Id == entityId);
                        if (outfit != null)
                        {
                            foreach (var img in outfit.Images)
                            {
                                img.IsMain = (img.Id == imageId);
                            }
                            outfit.ImageUrl = image.ImageUrl;
                        }
                        break;
                        
                    case "blog":
                        var blog = await _context.Blogs.Include(b => b.Images).FirstOrDefaultAsync(b => b.Id == entityId);
                        if (blog != null)
                        {
                            foreach (var img in blog.Images)
                            {
                                img.IsMain = (img.Id == imageId);
                            }
                            blog.ImageUrl = image.ImageUrl;
                        }
                        break;
                        
                    case "styletip":
                        var styleTip = await _context.StyleTips.Include(s => s.Images).FirstOrDefaultAsync(s => s.Id == entityId);
                        if (styleTip != null)
                        {
                            foreach (var img in styleTip.Images)
                            {
                                img.IsMain = (img.Id == imageId);
                            }
                            styleTip.ImageUrl = image.ImageUrl;
                        }
                        break;
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Ana görsel güncellendi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetEntityImages(string entityType, int entityId)
        {
            if (!IsAdmin())
                return Json(new { success = false, message = "Yetkisiz erişim." });

            try
            {
                List<Image> images = new List<Image>();
                
                switch (entityType.ToLower())
                {
                    case "outfit":
                        var outfit = await _context.Outfits.Include(o => o.Images).FirstOrDefaultAsync(o => o.Id == entityId);
                        if (outfit != null && outfit.Images != null)
                        {
                            images = outfit.Images.OrderBy(i => i.DisplayOrder).ToList();
                        }
                        break;
                        
                    case "blog":
                        var blog = await _context.Blogs.Include(b => b.Images).FirstOrDefaultAsync(b => b.Id == entityId);
                        if (blog != null && blog.Images != null)
                        {
                            images = blog.Images.OrderBy(i => i.DisplayOrder).ToList();
                        }
                        break;
                        
                    case "styletip":
                        var styleTip = await _context.StyleTips.Include(s => s.Images).FirstOrDefaultAsync(s => s.Id == entityId);
                        if (styleTip != null && styleTip.Images != null)
                        {
                            images = styleTip.Images.OrderBy(i => i.DisplayOrder).ToList();
                        }
                        break;
                        
                    default:
                        return Json(new { success = false, message = "Geçersiz entity tipi." });
                }
                
                var result = images.Select(i => new
                {
                    id = i.Id,
                    url = i.ImageUrl,
                    isMain = i.IsMain,
                    displayOrder = i.DisplayOrder,
                    title = i.Title,
                    description = i.Description
                }).ToList();
                
                return Json(new { success = true, images = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
