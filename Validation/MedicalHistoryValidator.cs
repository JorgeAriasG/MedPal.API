using FluentValidation;
using MedPal.API.DTOs;
using System;

namespace MedPal.API.Validation;

public class MedicalHistoryValidator : AbstractValidator<MedicalHistoryWriteDTO>
{
    public MedicalHistoryValidator()
    {
        RuleFor(x => x.PatientDetailsId)
            .GreaterThan(0).WithMessage("El ID de detalles del paciente debe ser mayor a 0.");

        RuleFor(x => x.SpecialtyType)
            .NotEmpty().WithMessage("El tipo de especialidad es requerido.")
            .MaximumLength(50).WithMessage("La especialidad no puede exceder 50 caracteres.");

        RuleFor(x => x.Diagnosis)
            .NotEmpty().WithMessage("El diagnóstico es requerido.")
            .MaximumLength(1000).WithMessage("El diagnóstico no puede exceder los 1000 caracteres.");

        RuleFor(x => x.DiagnosisDate)
            .NotEmpty().WithMessage("La fecha del diagnóstico es requerida.")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("La fecha del diagnóstico no puede estar en el futuro.");

        RuleFor(x => x.ClinicalNotes)
            .MaximumLength(2000).WithMessage("Las notas clínicas no pueden exceder 2000 caracteres.");
            
        // El FollowUpDate si viene provisto puede ser en el futuro
        RuleFor(x => x.FollowUpDate)
            .GreaterThanOrEqualTo(x => x.DiagnosisDate).When(x => x.FollowUpDate.HasValue)
            .WithMessage("La fecha de seguimiento debe ser posterior a la fecha del diagnóstico.");
    }
}
