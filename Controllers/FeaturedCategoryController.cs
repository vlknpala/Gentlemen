using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gentlemen.Data;
using Gentlemen.Models;
using Gentlemen.Services;

namespace Gentlemen.Controllers
{
    public class FeaturedCategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileUploadService _fileUploadService;

        public FeaturedCategoryController(ApplicationDbContext context, IFileUploadService fileUploadService)
        {
            _context = context;
            _fileUploadService = fileUploadService;
        }

        // GET: Admin/FeaturedCategories
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Admin");

            var categories = await _context.FeaturedCategories
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
            return View(categories);
        }

        // POST: Admin/AddFeaturedCategory
        [HttpPost]
        public async Task<IActionResult> Add([FromForm] FeaturedCategory category, IFormFile Image)
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
                string imageUrl;
                try
                {
                    imageUrl = await _fileUploadService.UploadFileAsync(Image);
                    category.ImageUrl = imageUrl;
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = $"Görsel yükleme hatası: {ex.Message}" });
                }

                // Kategori bilgilerini ayarla
                category.CreatedAt = DateTime.Now;
                category.IsActive = true;
                category.DisplayOrder = await _context.FeaturedCategories.CountAsync(); // Yeni eklenen en sona gelsin

                // Veritabanına kaydet
                try
                {
                    await _context.FeaturedCategories.AddAsync(category);
                    await _context.SaveChangesAsync();

                    // Başarılı kayıt sonrası detaylı bilgi dön
                    return Json(new { 
                        success = true, 
                        message = "Kategori başarıyla eklendi.",
                        data = new {
                            id = category.Id,
                            title = category.Title,
                            description = category.Description,
                            imageUrl = category.ImageUrl,
                            category = category.Category,
                            isActive = category.IsActive
                        }
                    });
                }
                catch (Exception ex)
                {
                    // Görsel yüklenmiş ama veritabanı kaydı başarısız olduysa görseli sil
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        _fileUploadService.DeleteFile(imageUrl);
                    }
                    return Json(new { success = false, message = $"Veritabanı hatası: {ex.Message}" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Beklenmeyen bir hata oluştu: {ex.Message}" });
            }
        }

        // POST: Admin/DeleteFeaturedCategory/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin())
                return Json(new { success = false, message = "Yetkisiz erişim." });

            var category = await _context.FeaturedCategories.FindAsync(id);
            if (category == null)
                return Json(new { success = false, message = "Kategori bulunamadı." });

            try
            {
                if (!string.IsNullOrEmpty(category.ImageUrl))
                {
                    _fileUploadService.DeleteFile(category.ImageUrl);
                }

                _context.FeaturedCategories.Remove(category);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Silme işlemi sırasında bir hata oluştu." });
            }
        }

        // POST: Admin/UpdateFeaturedCategoryOrder
        [HttpPost]
        public async Task<IActionResult> UpdateOrder([FromBody] List<OrderUpdateModel> updates)
        {
            if (!IsAdmin())
                return Json(new { success = false, message = "Yetkisiz erişim." });

            try
            {
                foreach (var update in updates)
                {
                    var category = await _context.FeaturedCategories.FindAsync(update.Id);
                    if (category != null)
                    {
                        category.DisplayOrder = update.Order;
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Sıralama güncellenirken bir hata oluştu." });
            }
        }

        // POST: Admin/UpdateFeaturedCategoryStatus
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, bool isActive)
        {
            if (!IsAdmin())
                return Json(new { success = false, message = "Yetkisiz erişim." });

            try
            {
                var category = await _context.FeaturedCategories.FindAsync(id);
                if (category == null)
                    return Json(new { success = false, message = "Kategori bulunamadı." });

                category.IsActive = isActive;
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Durum güncellenirken bir hata oluştu." });
            }
        }

        private bool IsAdmin()
        {
            var adminUser = HttpContext.Session.GetString("AdminUser");
            return adminUser == "admin";
        }
    }

    public class OrderUpdateModel
    {
        public int Id { get; set; }
        public int Order { get; set; }
    }
} 