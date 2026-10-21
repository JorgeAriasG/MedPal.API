using FluentValidation;
using MedPal.API.DTOs;
using MedPal.API.Repositories;
using System;

namespace MedPal.API.Validation;

public class AvailabilityBlockValidator : AbstractValidator<AvailabilityBlockWriteDTO>
{
    private readonly IAvailabilityBlockRepository _repository;

    public AvailabilityBlockValidator(IAvailabilityBlockRepository repository)
    {
        _repository = repository;

        RuleFor(x => x.ClinicId)
            .GreaterThan(0).WithMessage("El ID de la clínica debe ser mayor a 0.");

        RuleFor(x => x.DoctorId)
            .GreaterThan(0).When(x => x.DoctorId.HasValue).WithMessage("El ID del doctor debe ser mayor a 0.");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("La fecha es requerida.")
            .Must(BeAValidDate).WithMessage("La fecha del bloqueo no puede estar en el pasado.");

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("La hora de inicio es requerida.");

        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("La hora de fin es requerida.")
            .GreaterThan(x => x.StartTime).WithMessage("La hora de fin debe ser posterior a la hora de inicio.");

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("El motivo no puede exceder 500 caracteres.");

        RuleFor(x => x)
            .MustAsync(NotHaveOverlap).WithMessage("El bloqueo se solapa con una cita o con otro bloqueo en ese horario.");
    }

    private bool BeAValidDate(DateOnly date)
    {
        return date >= DateOnly.FromDateTime(DateTime.Today);
    }

    private async Task<bool> NotHaveOverlap(AvailabilityBlockWriteDTO dto, CancellationToken cancellationToken)
    {
        if (await _repository.HasOverlapWithAppointmentsAsync(dto.ClinicId, dto.DoctorId, dto.Date, dto.StartTime, dto.EndTime))
            return false;

        return !await _repository.HasOverlapWithBlocksAsync(dto.ClinicId, dto.DoctorId, dto.Date, dto.StartTime, dto.EndTime);
    }
}