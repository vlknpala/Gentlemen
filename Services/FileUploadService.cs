using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Gentlemen.Services
{
    public interface IFileUploadService
    {
        Task<string> UploadFileAsync(IFormFile file);
        void DeleteFile(string filePath);
    }

    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string _uploadsFolder;

        public FileUploadService(IWebHostEnvironment environment)
        {
            _environment = environment;
            _uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            
            // Ensure uploads folder exists
            if (!Directory.Exists(_uploadsFolder))
            {
                Directory.CreateDirectory(_uploadsFolder);
            }
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("No file was provided");
            }

            // Validate file type
            var allowedTypes = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedTypes.Contains(extension))
            {
                throw new ArgumentException("Invalid file type. Only image files are allowed.");
            }

            // Create unique filename
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(_uploadsFolder, uniqueFileName);

            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // Return the relative path for web access
                return $"/uploads/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                throw new Exception($"File upload failed: {ex.Message}");
            }
        }

        public void DeleteFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return;

            try
            {
                // Convert web path to physical path
                var fileName = Path.GetFileName(filePath.TrimStart('/'));
                var physicalPath = Path.Combine(_uploadsFolder, fileName);

                if (File.Exists(physicalPath))
                {
                    File.Delete(physicalPath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"File deletion failed: {ex.Message}");
            }
        }
    }
} 