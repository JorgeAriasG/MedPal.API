using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MedPal.API.Services.Implementations
{
    public class AttachmentStorageService : IAttachmentStorageService
    {
        private readonly string _basePath;
        private readonly ILogger<AttachmentStorageService> _logger;

        public AttachmentStorageService(IConfiguration configuration, ILogger<AttachmentStorageService> logger)
        {
            _logger = logger;
            _basePath = configuration["Storage:AttachmentsPath"] ?? Path.Combine(Path.GetTempPath(), "medpal-attachments");
        }

        public async Task<string> SaveAsync(int medicalHistoryId, string fileName, Stream content, CancellationToken cancellationToken = default)
        {
            var safeName = $"{Guid.NewGuid():N}_{SanitizeFileName(fileName)}";
            var relativePath = Path.Combine(medicalHistoryId.ToString(), safeName);

            var fullPath = GetFullPath(relativePath);
            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var output = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true);
            await content.CopyToAsync(output, 81920, cancellationToken);

            return relativePath;
        }

        public string GetFullPath(string relativePath)
        {
            return Path.Combine(_basePath, relativePath);
        }

        public void Delete(string relativePath)
        {
            try
            {
                var fullPath = GetFullPath(relativePath);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo eliminar el adjunto en disco: {Path}", relativePath);
            }
        }

        private static string SanitizeFileName(string fileName)
        {
            var name = Path.GetFileName(fileName);
            var invalid = Path.GetInvalidFileNameChars();
            foreach (var c in invalid)
            {
                name = name.Replace(c, '_');
            }
            return string.IsNullOrWhiteSpace(name) ? "file" : name;
        }
    }
}
