using System;
using System.Security.Cryptography;
using System.Text;

namespace MedPal.API.Utils
{
    /// <summary>
    /// Generación de tokens crudos de registro y su hash SHA-256 (T02c).
    /// Nunca se almacena el token crudo; solo su hash.
    /// </summary>
    public static class TokenGenerator
    {
        /// <summary>
        /// Token URL-safe de 32 bytes codificado en base64 sin padding ni caracteres conflictivos.
        /// </summary>
        public static string GenerateRawToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');
        }

        /// <summary>
        /// Hash SHA-256 en hex minúsculas (64 caracteres) usado para almacenamiento y lookups.
        /// </summary>
        public static string Sha256Hex(string input)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
}