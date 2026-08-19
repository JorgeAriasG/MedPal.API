using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MedPal.API.Services
{
    public interface IAttachmentStorageService
    {
        /// <summary>
        /// Guarda un archivo bajo Storage:AttachmentsPath y devuelve la ruta relativa.
        /// </summary>
        Task<string> SaveAsync(int medicalHistoryId, string fileName, Stream content, CancellationToken cancellationToken = default);

        /// <summary>
        /// Resuelve una ruta relativa almacenada a la ruta absoluta en disco.
        /// </summary>
        string GetFullPath(string relativePath);

        /// <summary>
        /// Elimina el archivo físico si existe (no lanza si falta).
        /// </summary>
        void Delete(string relativePath);
    }
}
