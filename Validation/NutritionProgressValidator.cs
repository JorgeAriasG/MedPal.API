using FluentValidation;
using MedPal.API.DTOs;

namespace MedPal.API.Validation
{
    public class NutritionProgressValidator : AbstractValidator<NutritionProgressWriteDTO>
    {
        public NutritionProgressValidator()
        {
            RuleFor(x => x.PatientDetailsId)
                .NotEmpty().WithMessage("El ID de PatientDetails es requerido")
                .GreaterThan(0).WithMessage("El ID de PatientDetails debe ser mayor a 0");

            RuleFor(x => x.RecordedAt)
                .NotEmpty().WithMessage("La fecha de registro es requerida");

            RuleFor(x => x.Weight)
                .InclusiveBetween(0m, 500m).When(x => x.Weight.HasValue)
                .WithMessage("El peso debe estar entre 0 y 500 kg");

            RuleFor(x => x.BodyFatPercentage)
                .InclusiveBetween(0m, 100m).When(x => x.BodyFatPercentage.HasValue)
                .WithMessage("El porcentaje de grasa debe estar entre 0 y 100%");

            RuleFor(x => x.CaloriesConsumed)
                .InclusiveBetween(0m, 10000m).When(x => x.CaloriesConsumed.HasValue)
                .WithMessage("Las calorías consumidas deben estar entre 0 y 10000");

            RuleFor(x => x.Notes)
                .MaximumLength(2000).When(x => !string.IsNullOrEmpty(x.Notes))
                .WithMessage("Las notas no pueden exceder 2000 caracteres");
        }
    }
}
