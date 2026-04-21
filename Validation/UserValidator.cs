using FluentValidation;
using MedPal.API.DTOs;

namespace MedPal.API.Validation;

public class UserRegisterValidator : AbstractValidator<UserRegisterDTO>
{
    public UserRegisterValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido.")
            .Length(3, 100).WithMessage("El nombre debe tener entre 3 y 100 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es requerido.")
            .EmailAddress().WithMessage("El email no tiene un formato válido.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es requerida.")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
            .Matches("[A-Z]").WithMessage("La contraseña debe contener al menos una mayúscula.")
            .Matches("[0-9]").WithMessage("La contraseña debe contener al menos un número.");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("Las contraseñas no coinciden.");

        RuleFor(x => x.AcceptPrivacyTerms)
            .Equal(true).WithMessage("Debe aceptar los términos de privacidad.");

        RuleFor(x => x.Specialty)
            .MaximumLength(100).WithMessage("La especialidad no puede exceder 100 caracteres.");

        RuleFor(x => x.ProfessionalLicenseNumber)
            .MaximumLength(100).WithMessage("El número de licencia no puede exceder 100 caracteres.");
    }
}
