using FluentValidation;
using MedPal.API.DTOs;
using MedPal.API.Repositories;
using System;
using System.Linq;

namespace MedPal.API.Validation;

public class PrescriptionValidator : AbstractValidator<PrescriptionWriteDTO>
{
    private readonly IPatientRepository _patientRepository;

    public PrescriptionValidator(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;

        RuleFor(x => x.PatientId)
            .GreaterThan(0).WithMessage("El ID del paciente debe ser mayor a 0.");

        RuleFor(x => x.Diagnosis)
            .NotEmpty().WithMessage("El diagnóstico es requerido.")
            .MaximumLength(500).WithMessage("El diagnóstico no puede exceder 500 caracteres.");

        RuleFor(x => x.ExpiresAt)
            .NotEmpty().WithMessage("La fecha de expiración es requerida.")
            .GreaterThan(DateTime.UtcNow).WithMessage("La fecha de expiración debe ser en el futuro.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("La receta debe contener al menos un medicamento.");

        RuleForEach(x => x.Items).SetValidator(new PrescriptionItemValidator());

        // Regla de negocio: validar que ningún medicamento coincida con una alergia registrada del paciente
        RuleFor(x => x)
            .MustAsync(NotContainAllergenicMedicationsAsync)
            .WithMessage("La receta contiene uno o más medicamentos a los que el paciente es alérgico. Verifique el historial de alergias.");
    }

    private async Task<bool> NotContainAllergenicMedicationsAsync(
        PrescriptionWriteDTO dto,
        CancellationToken cancellationToken)
    {
        if (dto.Items == null || !dto.Items.Any() || dto.PatientId <= 0)
            return true;

        var patientAllergies = await _patientRepository
            .GetPatientAllergyNamesAsync(dto.PatientId, cancellationToken);

        if (!patientAllergies.Any())
            return true;

        // Detectar si algún medicamento coincide (parcialmente) con una alergia registrada
        var allergyList = patientAllergies.ToList();
        var hasConflict = dto.Items.Any(item =>
            allergyList.Any(allergy =>
                item.MedicationName.ToLower().Contains(allergy) ||
                allergy.Contains(item.MedicationName.ToLower())));

        return !hasConflict;
    }
}

public class PrescriptionItemValidator : AbstractValidator<PrescriptionItemDTO>
{
    public PrescriptionItemValidator()
    {
        RuleFor(x => x.MedicationName)
            .NotEmpty().WithMessage("El nombre del medicamento es requerido.")
            .MaximumLength(200).WithMessage("El medicamento no puede exceder 200 caracteres.");

        RuleFor(x => x.Dosage)
            .NotEmpty().WithMessage("La dosis es requerida.")
            .MaximumLength(100).WithMessage("La dosis no puede exceder 100 caracteres.");

        RuleFor(x => x.Frequency)
            .NotEmpty().WithMessage("La frecuencia es requerida.")
            .MaximumLength(100).WithMessage("La frecuencia no puede exceder 100 caracteres.");

        RuleFor(x => x.Duration)
            .NotEmpty().WithMessage("La duración es requerida.")
            .MaximumLength(100).WithMessage("La duración no puede exceder 100 caracteres.");

        RuleFor(x => x.Instructions)
            .MaximumLength(500).WithMessage("Las instrucciones no pueden exceder 500 caracteres.");
    }
}
