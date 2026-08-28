using FluentValidation.TestHelper;
using MedPal.API.DTOs;
using MedPal.API.Repositories;
using MedPal.API.Validation;
using Moq;
using Xunit;

namespace MedPal.API.Tests.Validation;

public class AppointmentValidatorTests
{
    private readonly Mock<IAppointmentRepository> _repoMock;
    private readonly AppointmentValidator _validator;

    public AppointmentValidatorTests()
    {
        _repoMock = new Mock<IAppointmentRepository>();
        // Por defecto: sin solapamiento
        _repoMock
            .Setup(r => r.HasOverlapAsync(It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<TimeOnly>(), It.IsAny<int>(), It.IsAny<int?>()))
            .ReturnsAsync(false);

        _validator = new AppointmentValidator(_repoMock.Object);
    }

    [Fact]
    public async Task Appointment_ValidRequest_ShouldPass()
    {
        var dto = new AppointmentWriteDTO
        {
            PatientId   = 1,
            UserId      = 1,
            ClinicId    = 1,
            Date        = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            Time        = new TimeOnly(10, 0),
            DurationMinutes = 30,
            Notes       = "Consulta de seguimiento"
        };

        var result = await _validator.TestValidateAsync(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Appointment_PastDate_ShouldFail()
    {
        var dto = new AppointmentWriteDTO
        {
            PatientId = 1, UserId = 1, ClinicId = 1,
            Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
            Time = new TimeOnly(10, 0),
            DurationMinutes = 30
        };

        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.Date)
              .WithErrorMessage("La fecha de la cita no puede estar en el pasado.");
    }

    [Fact]
    public async Task Appointment_DurationTooShort_ShouldFail()
    {
        var dto = new AppointmentWriteDTO
        {
            PatientId = 1, UserId = 1, ClinicId = 1,
            Date = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            Time = new TimeOnly(10, 0),
            DurationMinutes = 10  // <15 → debe fallar
        };

        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.DurationMinutes)
              .WithErrorMessage("La duración de la cita debe estar entre 15 y 120 minutos.");
    }

    [Fact]
    public async Task Appointment_DurationTooLong_ShouldFail()
    {
        var dto = new AppointmentWriteDTO
        {
            PatientId = 1, UserId = 1, ClinicId = 1,
            Date = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            Time = new TimeOnly(10, 0),
            DurationMinutes = 130  // >120 → debe fallar
        };

        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.DurationMinutes)
              .WithErrorMessage("La duración de la cita debe estar entre 15 y 120 minutos.");
    }

    [Fact]
    public async Task Appointment_DoctorHasOverlap_ShouldFail()
    {
        _repoMock
            .Setup(r => r.HasOverlapAsync(It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<TimeOnly>(), It.IsAny<int>(), It.IsAny<int?>()))
            .ReturnsAsync(true);

        var dto = new AppointmentWriteDTO
        {
            PatientId = 1, UserId = 1, ClinicId = 1,
            Date = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            Time = new TimeOnly(10, 0),
            DurationMinutes = 30
        };

        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x)
              .WithErrorMessage("El doctor ya tiene una cita progamada en ese horario exacto.");
    }

    [Fact]
    public async Task Appointment_InvalidPatientId_ShouldFail()
    {
        var dto = new AppointmentWriteDTO { PatientId = 0, DurationMinutes = 30 };
        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.PatientId)
              .WithErrorMessage("El ID del paciente debe ser mayor a 0.");
    }
}
