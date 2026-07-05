using FluentValidation;
using MedPal.API.DTOs;

namespace MedPal.API.Validation
{
    public class DietPlanValidator : AbstractValidator<DietPlanWriteDTO>
    {
        public DietPlanValidator()
        {
            RuleFor(x => x.PatientDetailsId)
                .NotEmpty().WithMessage("El ID del paciente es requerido")
                .GreaterThan(0).WithMessage("El ID del paciente debe ser mayor a 0");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre del plan es requerido")
                .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres");

            RuleFor(x => x.Description)
                .MaximumLength(2000).When(x => !string.IsNullOrEmpty(x.Description))
                .WithMessage("La descripción no puede exceder 2000 caracteres");

            RuleFor(x => x.DailyCalories)
                .InclusiveBetween(0m, 10000m).When(x => x.DailyCalories.HasValue)
                .WithMessage("Las calorías diarias deben estar entre 0 y 10000");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("El estado es requerido")
                .Must(s => s is "Draft" or "Active" or "Completed" or "Cancelled")
                .WithMessage("Estado inválido. Use: Draft, Active, Completed o Cancelled");

            RuleFor(x => x.EndDate)
                .GreaterThanOrEqualTo(x => x.StartDate)
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
                .WithMessage("La fecha de fin debe ser posterior a la fecha de inicio");

            RuleForEach(x => x.Meals).SetValidator(new DietPlanMealWriteValidator());
        }
    }

    public class DietPlanMealWriteValidator : AbstractValidator<DietPlanMealWriteDTO>
    {
        public DietPlanMealWriteValidator()
        {
            RuleFor(x => x.MealName)
                .NotEmpty().WithMessage("El nombre de la comida es requerido")
                .MaximumLength(100).WithMessage("El nombre de la comida no puede exceder 100 caracteres");

            RuleFor(x => x.MealOrder)
                .GreaterThanOrEqualTo(0).WithMessage("El orden de la comida debe ser un valor positivo");

            RuleForEach(x => x.Items).SetValidator(new DietPlanMealItemWriteValidator());
        }
    }

    public class DietPlanMealItemWriteValidator : AbstractValidator<DietPlanMealItemWriteDTO>
    {
        public DietPlanMealItemWriteValidator()
        {
            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0");

            RuleFor(x => x.Unit)
                .NotEmpty().WithMessage("La unidad es requerida")
                .MaximumLength(50).WithMessage("La unidad no puede exceder 50 caracteres");

            RuleFor(x => x.Notes)
                .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Notes))
                .WithMessage("Las notas no pueden exceder 500 caracteres");
        }
    }

    public class DietPlanStatusUpdateValidator : AbstractValidator<DietPlanStatusUpdateDTO>
    {
        public DietPlanStatusUpdateValidator()
        {
            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("El estado es requerido")
                .Must(s => s is "Draft" or "Active" or "Completed" or "Cancelled")
                .WithMessage("Estado inválido. Use: Draft, Active, Completed o Cancelled");
        }
    }
}
