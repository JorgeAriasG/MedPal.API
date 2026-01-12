using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace MedPal.API.Services
{
    public class EncryptionProvider
    {
        private readonly string _key;

        public EncryptionProvider(IConfiguration configuration)
        {
            // Try to get key from env var or config, fallback to a dev key (INSECURE for PROD)
            _key = configuration["Encryption:Key"] ?? Environment.GetEnvironmentVariable("MEDPAL_ENCRYPTION_KEY");

            if (string.IsNullOrEmpty(_key))
            {
                // Fallback for Development ONLY - 32 bytes (256 bit)
                // WARNING: Never use this in production
                _key = "Dev_Key_Change_Me_In_Prod_1234567890_!!!";
            }

            if (_key.Length < 32)
            {
                throw new ArgumentException("Encryption Key must be at least 32 characters (256 bits).");
            }
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;

            using (Aes aes = Aes.Create())
            {
                // Use the first 32 chars of the key
                aes.Key = Encoding.UTF8.GetBytes(_key.Substring(0, 32));
                aes.GenerateIV();

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream())
                {
                    // Prepend IV to the stream so we can use it for decryption
                    ms.Write(aes.IV, 0, aes.IV.Length);

                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }
                    }

                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;

            try
            {
                var fullCipher = Convert.FromBase64String(cipherText);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(_key.Substring(0, 32));

                    // Read IV from the beginning (first 16 bytes for AES)
                    byte[] iv = new byte[aes.BlockSize / 8];
                    Array.Copy(fullCipher, 0, iv, 0, iv.Length);
                    aes.IV = iv;

                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    using (MemoryStream ms = new MemoryStream(fullCipher, iv.Length, fullCipher.Length - iv.Length))
                    {
                        using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader sr = new StreamReader(cs))
                            {
                                return sr.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch
            {
                // If decryption fails (e.g. data wasn't encrypted yet), return original
                // This helps with migration if mixed data exists, though ideally we migrate all.
                return cipherText;
            }
        }
    }
}
