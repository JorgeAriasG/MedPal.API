// MedPal.API/Validation/CreatePatientValidator.cs
using FluentValidation;
using MedPal.API.DTOs;
using MedPal.API.Repositories;

namespace MedPal.API.Validation;

public class CreatePatientValidator : AbstractValidator<PatientWriteDTO>
{
    private readonly IPatientRepository _repository;

    public CreatePatientValidator(IPatientRepository repository)
    {
        _repository = repository;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MinimumLength(2).WithMessage("Name must be at least 2 characters")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MustAsync(BeUniqueEmail).When(x => !string.IsNullOrEmpty(x.Email)).WithMessage("Email already exists");

        RuleFor(x => x.Dob)
            .NotEmpty().WithMessage("Date of birth is required")
            .Must(BeValidAge).WithMessage("Patient must be at least 18 years old");

        RuleFor(x => x.Phone)
            .Matches(@"^\+?[0-9\-\s()]+$").When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage("Invalid phone format");

        RuleFor(x => x.ClinicIds)
            .NotEmpty().WithMessage("At least one clinic is required");
        RuleForEach(x => x.ClinicIds)
            .GreaterThan(0).WithMessage("Each ClinicId must be greater than 0");

        // CURP validation (optional, Mexican format: 18 alphanumeric chars)
        RuleFor(x => x.Curp)
            .Matches(@"^[A-Z0-9]{18}$").When(x => !string.IsNullOrEmpty(x.Curp))
            .WithMessage("CURP must be exactly 18 alphanumeric characters");

        // Middlename, Lastname, Address, Gender, EmergencyContact are optional
    }

    private bool BeValidAge(DateTime dateOfBirth)
    {
        var age = DateTime.Today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > DateTime.Today.AddYears(-age)) age--;
        return age >= 18;
    }

    private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
    {
        return !await _repository.EmailExistsAsync(email.ToLowerInvariant(), cancellationToken);
    }
}