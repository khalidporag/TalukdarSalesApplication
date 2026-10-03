using TalukdarSales.Web.Helpers;

namespace TalukdarSales.Web.Services
{
    /// <summary>Saves validated image uploads under wwwroot/images/{folder}.</summary>
    public class ImageStore
    {
        private readonly IWebHostEnvironment _env;
        public ImageStore(IWebHostEnvironment env) => _env = env;

        public static bool IsValid(IFormFile file) => file == null || UploadValidator.IsValidImage(file);

        /// <returns>The stored file name, or "" when no file was supplied.</returns>
        public async Task<string> SaveAsync(IFormFile file, string folder)
        {
            if (file == null)
                return "";
            var name = Guid.NewGuid() + Path.GetExtension(file.FileName).ToLowerInvariant();
            var dir = Path.Combine(_env.WebRootPath, "images", folder);
            Directory.CreateDirectory(dir);
            await using var stream = new FileStream(Path.Combine(dir, name), FileMode.Create);
            await file.CopyToAsync(stream);
            return name;
        }
    }
}
