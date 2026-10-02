using Microsoft.AspNetCore.Http;

namespace TalukdarSalesAPI.Helpers
{
    public static class UploadValidator
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const long MaxBytes = 5 * 1024 * 1024;

        public static bool IsValidImage(IFormFile file)
        {
            if (file == null || file.Length == 0 || file.Length > MaxBytes)
                return false;
            var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            return ext != null && AllowedExtensions.Contains(ext);
        }
    }
}
