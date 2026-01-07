using System;
using System.ComponentModel.DataAnnotations;

namespace MedPal.API.Validation
{
    /// <summary>
    /// Validates that a DateTime value is in the future
    /// Used for temporal role assignments and other future-dated operations
    /// </summary>
    public class FutureDateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                // Null values are valid (use [Required] for mandatory fields)
                return ValidationResult.Success;
            }

            if (value is DateTime dateTime)
            {
                if (dateTime <= DateTime.UtcNow)
                {
                    return new ValidationResult(
                        ErrorMessage ?? "La fecha debe ser futura.",
                        new[] { validationContext.MemberName ?? string.Empty }
                    );
                }

                return ValidationResult.Success;
            }

            return new ValidationResult(
                "El valor debe ser una fecha válida.",
                new[] { validationContext.MemberName ?? string.Empty }
            );
        }
    }
}
