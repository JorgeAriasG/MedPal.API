using FluentValidation;
using MedPal.API.DTOs;

namespace MedPal.API.Validation
{
    /// <summary>
    /// Validador para contactos de emergencia
    /// </summary>
    public class EmergencyContactValidator : AbstractValidator<EmergencyContactWriteDTO>
    {
        public EmergencyContactValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("El nombre es requerido")
                .Length(1, 255).WithMessage("El nombre debe tener entre 1 y 255 caracteres");

            RuleFor(x => x.Relationship)
                .NotEmpty().WithMessage("La relación es requerida")
                .Length(1, 100).WithMessage("La relación debe tener entre 1 y 100 caracteres");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("El teléfono es requerido")
                .Matches(@"^[\d\s\-\+\(\)]{7,20}$").WithMessage("Formato de teléfono inválido (7-20 caracteres numéricos)");

            RuleFor(x => x.Email)
                .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
                .WithMessage("Email inválido")
                .MaximumLength(255).WithMessage("El email no puede exceder 255 caracteres");

            RuleFor(x => x.Address)
                .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Address))
                .WithMessage("La dirección no puede exceder 500 caracteres");

            RuleFor(x => x.Priority)
                .InclusiveBetween(1, 10).WithMessage("Prioridad debe estar entre 1 y 10");
        }
    }
}
