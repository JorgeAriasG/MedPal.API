using FluentValidation;
using MedPal.API.DTOs;

namespace MedPal.API.Validation
{
    public class FoodItemValidator : AbstractValidator<FoodItemWriteDTO>
    {
        public FoodItemValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre del alimento es requerido")
                .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres");

            RuleFor(x => x.Category)
                .NotEmpty().WithMessage("La categoría es requerida")
                .MaximumLength(50).WithMessage("La categoría no puede exceder 50 caracteres");

            RuleFor(x => x.ServingSize)
                .GreaterThan(0).WithMessage("La porción debe ser mayor a 0");

            RuleFor(x => x.ServingUnit)
                .NotEmpty().WithMessage("La unidad de porción es requerida");

            RuleFor(x => x.Calories)
                .GreaterThanOrEqualTo(0).WithMessage("Las calorías deben ser un valor positivo");

            RuleFor(x => x.Protein)
                .GreaterThanOrEqualTo(0).WithMessage("La proteína debe ser un valor positivo");

            RuleFor(x => x.Carbs)
                .GreaterThanOrEqualTo(0).WithMessage("Los carbohidratos deben ser un valor positivo");

            RuleFor(x => x.Fat)
                .GreaterThanOrEqualTo(0).WithMessage("La grasa debe ser un valor positivo");

            RuleFor(x => x.Brand)
                .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Brand))
                .WithMessage("La marca no puede exceder 100 caracteres");
        }
    }
}
