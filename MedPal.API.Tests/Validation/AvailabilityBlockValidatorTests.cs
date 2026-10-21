using FluentValidation.TestHelper;
using MedPal.API.DTOs;
using MedPal.API.Repositories;
using MedPal.API.Validation;
using Moq;
using Xunit;

namespace MedPal.API.Tests.Validation;

public class AvailabilityBlockValidatorTests
{
    private readonly Mock<IAvailabilityBlockRepository> _repoMock;
    private readonly AvailabilityBlockValidator _validator;

    public AvailabilityBlockValidatorTests()
    {
        _repoMock = new Mock<IAvailabilityBlockRepository>();
        // Por defecto: sin solapamiento con citas ni con otros bloqueos
        _repoMock
            .Setup(r => r.HasOverlapWithAppointmentsAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<DateOnly>(), It.IsAny<TimeOnly>(), It.IsAny<TimeOnly>()))
            .ReturnsAsync(false);
        _repoMock
            .Setup(r => r.HasOverlapWithBlocksAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<DateOnly>(), It.IsAny<TimeOnly>(), It.IsAny<TimeOnly>(), It.IsAny<int?>()))
            .ReturnsAsync(false);

        _validator = new AvailabilityBlockValidator(_repoMock.Object);
    }

    private static AvailabilityBlockWriteDTO ValidDto() => new()
    {
        ClinicId = 1,
        DoctorId = 1,
        Date = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
        StartTime = new TimeOnly(9, 0),
        EndTime = new TimeOnly(10, 0),
        Reason = "Capacitación"
    };

    [Fact]
    public async Task Block_ValidRequest_ShouldPass()
    {
        var result = await _validator.TestValidateAsync(ValidDto());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Block_ClinicWideWithoutDoctor_ShouldPass()
    {
        var dto = ValidDto();
        dto.DoctorId = null;

        var result = await _validator.TestValidateAsync(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Block_PastDate_ShouldFail()
    {
        var dto = ValidDto();
        dto.Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));

        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.Date)
              .WithErrorMessage("La fecha del bloqueo no puede estar en el pasado.");
    }

    [Fact]
    public async Task Block_EndTimeBeforeStartTime_ShouldFail()
    {
        var dto = ValidDto();
        dto.StartTime = new TimeOnly(11, 0);
        dto.EndTime = new TimeOnly(10, 0);

        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.EndTime);
    }

    [Fact]
    public async Task Block_OverlapsAppointment_ShouldFail()
    {
        // bloque 9:00-10:00 solapado con cita existente
        _repoMock
            .Setup(r => r.HasOverlapWithAppointmentsAsync(1, 1, It.IsAny<DateOnly>(), new TimeOnly(9, 0), new TimeOnly(10, 0)))
            .ReturnsAsync(true);

        var result = await _validator.TestValidateAsync(ValidDto());
        result.ShouldHaveValidationErrorFor(x => x)
              .WithErrorMessage("El bloqueo se solapa con una cita o con otro bloqueo en ese horario.");
    }

    [Fact]
    public async Task Block_OverlapsAnotherBlock_ShouldFail()
    {
        _repoMock
            .Setup(r => r.HasOverlapWithBlocksAsync(1, 1, It.IsAny<DateOnly>(), new TimeOnly(9, 0), new TimeOnly(10, 0), It.IsAny<int?>()))
            .ReturnsAsync(true);

        var result = await _validator.TestValidateAsync(ValidDto());
        result.ShouldHaveValidationErrorFor(x => x)
              .WithErrorMessage("El bloqueo se solapa con una cita o con otro bloqueo en ese horario.");
    }

    [Fact]
    public async Task Block_ClinicWideOverlapsAnyDoctorAppointment_ShouldFail()
    {
        // Bloqueo de clínica (DoctorId null) solapado con cita de CUALQUIER doctor
        _repoMock
            .Setup(r => r.HasOverlapWithAppointmentsAsync(1, null, It.IsAny<DateOnly>(), new TimeOnly(9, 0), new TimeOnly(10, 0)))
            .ReturnsAsync(true);

        var dto = ValidDto();
        dto.DoctorId = null;

        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x);
    }

    [Fact]
    public async Task Block_InvalidClinicId_ShouldFail()
    {
        var dto = ValidDto();
        dto.ClinicId = 0;

        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.ClinicId)
              .WithErrorMessage("El ID de la clínica debe ser mayor a 0.");
    }
}