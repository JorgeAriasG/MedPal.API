using System.Text.RegularExpressions;

namespace MedPal.API.Utils
{
    public static partial class PhoneNormalizer
    {
        /// <summary>
        /// Normaliza un teléfono mexicano a formato E.164 (ej: +521234567890).
        /// Maneja: espacios, guiones, paréntesis, prefijos 00/044/0, y el quirk de Telcel.
        /// </summary>
        public static string? ToE164(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return null;

            var digits = DigitsOnly().Replace(phone, string.Empty);

            if (digits.Length < 10)
                return null;

            // Quitar prefijo 00 (marcar desde el extranjero)
            if (digits.StartsWith("00") && digits.Length > 12)
                digits = digits[2..];

            // Quitar prefijo 044 (móvil local México)
            if (digits.StartsWith("044") && digits.Length == 13)
                digits = digits[3..];

            // Quitar prefijo 0 (discado local)
            if (digits.StartsWith("0") && digits.Length > 10)
                digits = digits[1..];

            // Tiene countryCode 52 + 10 dígitos = 12 → E.164 directo
            if (digits.Length == 12 && digits.StartsWith("52"))
                return $"+{digits}";

            // Telcel quirk: 521 + 10 dígitos = 13 → quitar el 1 extra
            if (digits.Length == 13 && digits.StartsWith("521"))
                return $"+52{digits[3..]}";

            // 10 dígitos → agregar 52 (México)
            if (digits.Length == 10)
                return $"+52{digits}";

            // Ya tiene countryCode 52 con largo inesperado → devolver tal cual
            if (digits.Length > 12 && digits.StartsWith("52"))
                return $"+{digits}";

            return null;
        }

        [GeneratedRegex(@"[^\d]")]
        private static partial Regex DigitsOnly();
    }
}
