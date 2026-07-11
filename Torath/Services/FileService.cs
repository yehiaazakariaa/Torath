namespace Torath.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FileService(IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor)
        {
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file was uploaded.");

            // 1. Define the path: wwwroot/uploads/{folderName}
            var webRootPath = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
            var uploadsFolder = Path.Combine(webRootPath, "uploads", folderName);

            // 2. Create the folder if it doesn't exist
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // 3. Generate a unique file name (e.g., 550e8400-e29b..._cover.jpg)
            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // 4. Save the file to the hard drive
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // 5. Generate the URL to return to the database (e.g., https://localhost:7231/uploads/covers/filename.jpg)
            var request = _httpContextAccessor.HttpContext!.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            return $"{baseUrl}/uploads/{folderName}/{uniqueFileName}";
        }



        public bool DeleteFile(string? fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl)) return false;

            try
            {
                // 1. Parse the URL to get the relative path
                var uri = new Uri(fileUrl);

                // 2. Decode the URL to convert %20 back into normal spaces!
                var decodedPath = System.Net.WebUtility.UrlDecode(uri.AbsolutePath);
                var relativePath = decodedPath.TrimStart('/');

                // 3. Combine with web root path to find the physical file on disk
                var webRootPath = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");

                // Fix path separators for Windows vs Linux
                relativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);
                var physicalPath = Path.Combine(webRootPath, relativePath);

                // 4. Delete the file if it exists
                if (File.Exists(physicalPath))
                {
                    File.Delete(physicalPath);
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }
    }
}
