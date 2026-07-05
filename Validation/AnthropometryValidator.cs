using FluentValidation;
using MedPal.API.DTOs;

namespace MedPal.API.Validation
{
    public class AnthropometryValidator : AbstractValidator<AnthropometryWriteDTO>
    {
        public AnthropometryValidator()
        {
            RuleFor(x => x.PatientDetailsId)
                .NotEmpty().WithMessage("El ID de PatientDetails es requerido")
                .GreaterThan(0).WithMessage("El ID de PatientDetails debe ser mayor a 0");

            RuleFor(x => x.RecordedAt)
                .NotEmpty().WithMessage("La fecha de medición es requerida");

            RuleFor(x => x.Weight)
                .InclusiveBetween(0m, 500m).When(x => x.Weight.HasValue)
                .WithMessage("El peso debe estar entre 0 y 500 kg");

            RuleFor(x => x.Height)
                .InclusiveBetween(0m, 3m).When(x => x.Height.HasValue)
                .WithMessage("La altura debe estar entre 0 y 3 m");

            RuleFor(x => x.Waist)
                .InclusiveBetween(0m, 300m).When(x => x.Waist.HasValue)
                .WithMessage("La cintura debe estar entre 0 y 300 cm");

            RuleFor(x => x.Hip)
                .InclusiveBetween(0m, 300m).When(x => x.Hip.HasValue)
                .WithMessage("La cadera debe estar entre 0 y 300 cm");

            RuleFor(x => x.Neck)
                .InclusiveBetween(0m, 100m).When(x => x.Neck.HasValue)
                .WithMessage("El cuello debe estar entre 0 y 100 cm");

            RuleFor(x => x.Notes)
                .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Notes))
                .WithMessage("Las notas no pueden exceder 500 caracteres");
        }
    }
}
