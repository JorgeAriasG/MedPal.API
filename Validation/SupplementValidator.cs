using FluentValidation;
using MedPal.API.DTOs;

namespace MedPal.API.Validation
{
    public class SupplementValidator : AbstractValidator<SupplementWriteDTO>
    {
        public SupplementValidator()
        {
            RuleFor(x => x.PatientDetailsId)
                .NotEmpty().WithMessage("El ID de PatientDetails es requerido")
                .GreaterThan(0).WithMessage("El ID de PatientDetails debe ser mayor a 0");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre del suplemento es requerido")
                .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres");

            RuleFor(x => x.Dosage)
                .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Dosage))
                .WithMessage("La dosis no puede exceder 100 caracteres");

            RuleFor(x => x.Frequency)
                .MaximumLength(50).When(x => !string.IsNullOrEmpty(x.Frequency))
                .WithMessage("La frecuencia no puede exceder 50 caracteres");

            RuleFor(x => x.Notes)
                .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Notes))
                .WithMessage("Las notas no pueden exceder 500 caracteres");
        }
    }
}
