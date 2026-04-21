// MedPal.API/Validation/UpdatePatientValidator.cs
using FluentValidation;
using MedPal.API.DTOs;
using MedPal.API.Repositories;

namespace MedPal.API.Validation;

public class UpdatePatientValidator : AbstractValidator<PatientWriteDTO>
{
    private readonly IPatientRepository _repository;

    public UpdatePatientValidator(IPatientRepository repository)
    {
        _repository = repository;

        // Assuming PatientWriteDTO has an Id property for updates
        // If not, this validator might need adjustment

        RuleFor(x => x.Name)
            .MinimumLength(2).When(x => !string.IsNullOrEmpty(x.Name)).WithMessage("Name must be at least 2 characters")
            .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Name)).WithMessage("Name cannot exceed 100 characters");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email)).WithMessage("Invalid email format")
            .MustAsync(BeUniqueEmailForUpdate).When(x => !string.IsNullOrEmpty(x.Email)).WithMessage("Email already exists");

        RuleFor(x => x.Dob)
            .Must(BeValidAge).When(x => x.Dob != default).WithMessage("Patient must be at least 18 years old");

        RuleFor(x => x.Phone)
            .Matches(@"^\+?[0-9\-\s()]+$").When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage("Invalid phone format");

        RuleFor(x => x.ClinicId)
            .GreaterThan(0).When(x => x.ClinicId > 0).WithMessage("ClinicId must be greater than 0");

        // Middlename, Lastname, Address, Gender, EmergencyContact are optional
    }

    private bool BeValidAge(DateTime dateOfBirth)
    {
        if (dateOfBirth == default) return true; // Optional in update
        var age = DateTime.Today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > DateTime.Today.AddYears(-age)) age--;
        return age >= 18;
    }

    private async Task<bool> BeUniqueEmailForUpdate(PatientWriteDTO request, string email, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(email)) return true; // Optional
        // Note: This assumes PatientWriteDTO has an Id property. If not, this needs adjustment
        // For now, assuming it exists or we need to modify the DTO
        return !await _repository.EmailExistsAsync(email.ToLowerInvariant(), cancellationToken);
    }
}