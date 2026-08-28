// MedPal.API.Tests/Validation/PatientValidatorTests.cs
using System.Collections.Generic;
using FluentValidation.TestHelper;
using MedPal.API.DTOs;
using MedPal.API.Repositories;
using MedPal.API.Validation;
using Moq;
using Xunit;

namespace MedPal.API.Tests.Validation;

public class PatientValidatorTests
{
    private readonly Mock<IPatientRepository> _repositoryMock;
    private readonly CreatePatientValidator _createValidator;
    private readonly UpdatePatientValidator _updateValidator;

    public PatientValidatorTests()
    {
        _repositoryMock = new Mock<IPatientRepository>();
        // Default setup: email doesn't exist unless specifically overridden
        _repositoryMock.Setup(r => r.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _createValidator = new CreatePatientValidator(_repositoryMock.Object);
        _updateValidator = new UpdatePatientValidator(_repositoryMock.Object);
    }

    #region CreatePatientValidator Tests

    [Fact]
    public async Task CreatePatient_ValidRequest_ShouldPass()
    {
        // Arrange
        _repositoryMock.Setup(r => r.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var request = new PatientWriteDTO
        {
            Name = "John Doe",
            Middlename = "M",
            Lastname = "Smith",
            Email = "john@example.com",
            Dob = new DateTime(1990, 1, 1),
            Phone = "+1234567890",
            Address = "123 Main St",
            Gender = "Male",
            EmergencyContact = "Jane Doe",
            ClinicIds = new List<int> { 1 }
        };

        // Act
        var result = await _createValidator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task CreatePatient_NameEmpty_ShouldFail()
    {
        // Arrange
        var request = new PatientWriteDTO { Name = "" };

        // Act
        var result = await _createValidator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name).WithErrorMessage("Name is required");
    }

    [Fact]
    public async Task CreatePatient_NameTooShort_ShouldFail()
    {
        // Arrange
        var request = new PatientWriteDTO { Name = "A" };

        // Act
        var result = await _createValidator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name).WithErrorMessage("Name must be at least 2 characters");
    }

    [Fact]
    public async Task CreatePatient_NameTooLong_ShouldFail()
    {
        // Arrange
        var request = new PatientWriteDTO { Name = new string('A', 101) };

        // Act
        var result = await _createValidator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name).WithErrorMessage("Name cannot exceed 100 characters");
    }

    [Fact]
    public async Task CreatePatient_EmailInvalidFormat_ShouldFail()
    {
        // Arrange
        var request = new PatientWriteDTO { Email = "invalid" };

        // Act
        var result = await _createValidator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage("Invalid email format");
    }

    [Fact]
    public async Task CreatePatient_EmailExists_ShouldFail()
    {
        // Arrange
        _repositoryMock.Setup(r => r.EmailExistsAsync("existing@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var request = new PatientWriteDTO { Email = "existing@example.com" };

        // Act
        var result = await _createValidator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage("Email already exists");
    }

    [Fact]
    public async Task CreatePatient_DobEmpty_ShouldFail()
    {
        // Arrange
        var request = new PatientWriteDTO { Dob = default };

        // Act
        var result = await _createValidator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Dob).WithErrorMessage("Date of birth is required");
    }

    [Fact]
    public async Task CreatePatient_Under18_ShouldFail()
    {
        // Arrange
        var request = new PatientWriteDTO { Dob = DateTime.Today.AddYears(-17) };

        // Act
        var result = await _createValidator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Dob).WithErrorMessage("Patient must be at least 18 years old");
    }

    [Fact]
    public async Task CreatePatient_InvalidPhone_ShouldFail()
    {
        // Arrange
        var request = new PatientWriteDTO { Phone = "invalid" };

        // Act
        var result = await _createValidator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Phone).WithErrorMessage("Invalid phone format");
    }

    [Fact]
    public async Task CreatePatient_EmptyClinicIds_ShouldFail()
    {
        // Arrange
        var request = new PatientWriteDTO { ClinicIds = new List<int>() };

        // Act
        var result = await _createValidator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ClinicIds).WithErrorMessage("At least one clinic is required");
    }

    [Fact]
    public async Task CreatePatient_InvalidClinicId_ShouldFail()
    {
        // Arrange
        var request = new PatientWriteDTO { ClinicIds = new List<int> { 0 } };

        // Act
        var result = await _createValidator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor("ClinicIds[0]").WithErrorMessage("Each ClinicId must be greater than 0");
    }

    #endregion

    #region UpdatePatientValidator Tests

    [Fact]
    public async Task UpdatePatient_ValidRequest_ShouldPass()
    {
        // Arrange
        _repositoryMock.Setup(r => r.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var request = new PatientWriteDTO
        {
            Name = "Jane Doe",
            Email = "jane@example.com",
            Dob = new DateTime(1995, 1, 1),
            Phone = "+0987654321",
            ClinicIds = new List<int> { 2 }
        };

        // Act
        var result = await _updateValidator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task UpdatePatient_NameTooShort_ShouldFail()
    {
        // Arrange
        var request = new PatientWriteDTO { Name = "A" };

        // Act
        var result = await _updateValidator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name).WithErrorMessage("Name must be at least 2 characters");
    }

    [Fact]
    public async Task UpdatePatient_EmailInvalidFormat_ShouldFail()
    {
        // Arrange
        var request = new PatientWriteDTO { Email = "invalid" };

        // Act
        var result = await _updateValidator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage("Invalid email format");
    }

    [Fact]
    public async Task UpdatePatient_Under18_ShouldFail()
    {
        // Arrange
        var request = new PatientWriteDTO { Dob = DateTime.Today.AddYears(-17) };

        // Act
        var result = await _updateValidator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Dob).WithErrorMessage("Patient must be at least 18 years old");
    }

    [Fact]
    public async Task UpdatePatient_InvalidPhone_ShouldFail()
    {
        // Arrange
        var request = new PatientWriteDTO { Phone = "invalid" };

        // Act
        var result = await _updateValidator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Phone).WithErrorMessage("Invalid phone format");
    }

    #endregion
}