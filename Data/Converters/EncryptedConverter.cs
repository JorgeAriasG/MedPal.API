using MedPal.API.Services;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MedPal.API.Data.Converters
{
    public class EncryptedConverter : ValueConverter<string, string>
    {
        public EncryptedConverter(EncryptionProvider provider)
            : base(
                v => provider.Encrypt(v),
                v => provider.Decrypt(v))
        {
        }
    }
}
