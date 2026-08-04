using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Torath.Helpers;
using Torath.Services;

namespace Torath.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] // File management is restricted exclusively to Admins
    public class FilesController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FilesController(IFileService fileService)
        {
            _fileService = fileService;
        }

        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("No file uploaded.");

                if (!file.ContentType.StartsWith("image/"))
                    return BadRequest("Only image files are allowed.");

                if (!FileSecurityValidator.IsValidFile(file))
                    return BadRequest("Invalid or malicious file detected. Fake images are blocked.");

                var url = await _fileService.UploadFileAsync(file, "images");
                return Ok(new { url });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("upload-pdf")]
        public async Task<IActionResult> UploadPdf(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("No file uploaded.");

                if (file.ContentType != "application/pdf")
                    return BadRequest("Only PDF files are allowed.");

                if (!FileSecurityValidator.IsValidFile(file))
                    return BadRequest("Invalid or malicious file detected. Fake PDFs are blocked.");

                var url = await _fileService.UploadFileAsync(file, "pdfs");
                return Ok(new { url });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("delete-file")]
        public IActionResult DeleteFile([FromQuery] string fileUrl)
        {
            var result = _fileService.DeleteFile(fileUrl);
            if (result)
            {
                return Ok(new { message = "File deleted successfully from the server filesystem." });
            }
            return NotFound(new { message = "File could not be found or could not be deleted." });
        }
    }
}