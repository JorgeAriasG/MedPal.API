using FluentValidation;
using MedPal.API.DTOs;

namespace MedPal.API.Validation
{
    public class BodyCompositionValidator : AbstractValidator<BodyCompositionWriteDTO>
    {
        public BodyCompositionValidator()
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
                .InclusiveBetween(0m, 300m).When(x => x.Height.HasValue)
                .WithMessage("La altura debe estar entre 0 y 300 cm");

            RuleFor(x => x.Bmi)
                .InclusiveBetween(0m, 100m).When(x => x.Bmi.HasValue)
                .WithMessage("El IMC debe estar entre 0 y 100");

            RuleFor(x => x.BodyFatPercentage)
                .InclusiveBetween(0m, 100m).When(x => x.BodyFatPercentage.HasValue)
                .WithMessage("El porcentaje de grasa debe estar entre 0 y 100%");

            RuleFor(x => x.MuscleMass)
                .InclusiveBetween(0m, 300m).When(x => x.MuscleMass.HasValue)
                .WithMessage("La masa muscular debe estar entre 0 y 300 kg");

            RuleFor(x => x.Bmr)
                .InclusiveBetween(0m, 5000m).When(x => x.Bmr.HasValue)
                .WithMessage("La TMB debe estar entre 0 y 5000 kcal");
        }
    }
}
