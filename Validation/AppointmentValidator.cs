using FluentValidation;
using MedPal.API.DTOs;
using MedPal.API.Repositories;
using System;

namespace MedPal.API.Validation;

public class AppointmentValidator : AbstractValidator<AppointmentWriteDTO>
{
    private readonly IAppointmentRepository _repository;

    public AppointmentValidator(IAppointmentRepository repository)
    {
        _repository = repository;

        RuleFor(x => x.PatientId)
            .GreaterThan(0).WithMessage("El ID del paciente debe ser mayor a 0.");

        RuleFor(x => x.ClinicId)
            .GreaterThan(0).WithMessage("El ID de la clínica debe ser mayor a 0.");

        RuleFor(x => x.UserId)
            .GreaterThan(0).When(x => x.UserId.HasValue).WithMessage("El ID del usuario debe ser mayor a 0.");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("La fecha es requerida.")
            .Must(BeAValidDate).WithMessage("La fecha de la cita no puede estar en el pasado.");

        RuleFor(x => x)
            .MustAsync(NotHaveOverlap).WithMessage("El doctor ya tiene una cita progamada en ese horario exacto.");

        RuleFor(x => x.Time)
            .NotEmpty().WithMessage("La hora es requerida.");

        RuleFor(x => x.Status)
            .MaximumLength(50).WithMessage("El estado no puede exceder 50 caracteres.");
            
        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Las notas no pueden exceder 500 caracteres.");

        RuleFor(x => x.DurationMinutes)
            .InclusiveBetween(15, 120)
            .WithMessage("La duración de la cita debe estar entre 15 y 120 minutos.");
    }

    private bool BeAValidDate(DateOnly date)
    {
        return date >= DateOnly.FromDateTime(DateTime.Today);
    }

    private async Task<bool> NotHaveOverlap(AppointmentWriteDTO dto, CancellationToken cancellationToken)
    {
        if (dto.UserId.HasValue)
        {
            return !await _repository.HasOverlapAsync(dto.UserId.Value, dto.Date, dto.Time, dto.DurationMinutes);
        }
        return true;
    }
}
