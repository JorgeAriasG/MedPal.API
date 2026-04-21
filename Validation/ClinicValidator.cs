using FluentValidation;
using MedPal.API.DTOs;

namespace MedPal.API.Validation;

public class ClinicValidator : AbstractValidator<ClinicWriteDTO>
{
    public ClinicValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la clínica es requerido.")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres.");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("La ubicación (dirección) es requerida.")
            .MaximumLength(500).WithMessage("La ubicación no puede exceder 500 caracteres.");

        RuleFor(x => x.ContactInfo)
            .NotEmpty().WithMessage("La información de contacto es requerida.")
            .MaximumLength(100).WithMessage("La información de contacto no puede exceder 100 caracteres.");

        RuleFor(x => x.Open)
            .NotEmpty().WithMessage("El horario de apertura es requerido.");

        RuleFor(x => x.Close)
            .NotEmpty().WithMessage("El horario de cierre es requerido.")
            .GreaterThan(x => x.Open).WithMessage("El horario de cierre debe ser posterior a la apertura.");
    }
}
