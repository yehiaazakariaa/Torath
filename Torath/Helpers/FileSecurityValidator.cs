using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Torath.Helpers
{
    public static class FileSecurityValidator
    {
        // These are the official byte signatures for safe file types
        private static readonly Dictionary<string, List<byte[]>> _fileSignatures = new()
        {
            { ".pdf", new List<byte[]> { new byte[] { 0x25, 0x50, 0x44, 0x46 } } },
            { ".png", new List<byte[]> { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
            { ".jpg", new List<byte[]>
                {
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 }
                }
            },
            { ".jpeg", new List<byte[]>
                {
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 }
                }
            }
        };

        public static bool IsValidFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            // 1. Check if the extension is in our allowed list
            if (string.IsNullOrEmpty(ext) || !_fileSignatures.ContainsKey(ext))
                return false;

            // 2. Read the first few bytes of the file to check its true identity
            using var reader = new BinaryReader(file.OpenReadStream());
            var signatures = _fileSignatures[ext];
            var headerBytes = reader.ReadBytes(signatures.Max(m => m.Length));

            // 3. Compare the file's bytes to the official signature
            return signatures.Any(signature =>
                headerBytes.Take(signature.Length).SequenceEqual(signature));
        }
    }
}
