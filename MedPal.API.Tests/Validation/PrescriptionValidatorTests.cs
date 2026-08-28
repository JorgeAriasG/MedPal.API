using FluentValidation.TestHelper;
using MedPal.API.DTOs;
using MedPal.API.Repositories;
using MedPal.API.Validation;
using Moq;
using Xunit;

namespace MedPal.API.Tests.Validation;

public class PrescriptionValidatorTests
{
    private readonly Mock<IPatientRepository> _patientRepoMock;
    private readonly PrescriptionValidator _validator;

    public PrescriptionValidatorTests()
    {
        _patientRepoMock = new Mock<IPatientRepository>();
        // Por defecto: paciente sin alergias
        _patientRepoMock
            .Setup(r => r.GetPatientAllergyNamesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string>());

        _validator = new PrescriptionValidator(_patientRepoMock.Object);
    }

    private static PrescriptionWriteDTO ValidPrescription() => new()
    {
        PatientId  = 1,
        Diagnosis  = "Infección respiratoria",
        ExpiresAt  = DateTime.UtcNow.AddDays(30),
        Items      = new List<PrescriptionItemDTO>
        {
            new() { MedicationName = "Amoxicilina", Dosage = "500mg", Frequency = "Cada 8h", Duration = "7 días", Instructions = "Con alimentos" }
        }
    };

    [Fact]
    public async Task Prescription_ValidRequest_NoAllergies_ShouldPass()
    {
        var result = await _validator.TestValidateAsync(ValidPrescription());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Prescription_MedicationMatchesAllergy_ShouldFail()
    {
        // Paciente es alérgico a amoxicilina
        _patientRepoMock
            .Setup(r => r.GetPatientAllergyNamesAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { "amoxicilina" });

        var result = await _validator.TestValidateAsync(ValidPrescription());

        result.ShouldHaveValidationErrorFor(x => x)
              .WithErrorMessage("La receta contiene uno o más medicamentos a los que el paciente es alérgico. Verifique el historial de alergias.");
    }

    [Fact]
    public async Task Prescription_DiagnosisEmpty_ShouldFail()
    {
        var dto = ValidPrescription();
        dto.Diagnosis = "";

        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.Diagnosis)
              .WithErrorMessage("El diagnóstico es requerido.");
    }

    [Fact]
    public async Task Prescription_ExpiresInPast_ShouldFail()
    {
        var dto = ValidPrescription();
        dto.ExpiresAt = DateTime.UtcNow.AddDays(-1);

        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.ExpiresAt)
              .WithErrorMessage("La fecha de expiración debe ser en el futuro.");
    }

    [Fact]
    public async Task Prescription_EmptyItems_ShouldFail()
    {
        var dto = ValidPrescription();
        dto.Items = new List<PrescriptionItemDTO>();

        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.Items)
              .WithErrorMessage("La receta debe contener al menos un medicamento.");
    }

    [Fact]
    public async Task Prescription_ItemMissingMedicationName_ShouldFail()
    {
        var dto = ValidPrescription();
        dto.Items[0].MedicationName = "";

        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor("Items[0].MedicationName")
              .WithErrorMessage("El nombre del medicamento es requerido.");
    }

    [Fact]
    public async Task Prescription_DifferentDrugFromAllergy_ShouldPass()
    {
        // Paciente es alérgico a penicilina, pero el medicamento es paracetamol → OK
        _patientRepoMock
            .Setup(r => r.GetPatientAllergyNamesAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { "penicilina" });

        var dto = ValidPrescription(); // Amoxicilina (que contiene "penicilina" → debería fallar en producción)
        dto.Items[0].MedicationName = "Paracetamol";

        var result = await _validator.TestValidateAsync(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
