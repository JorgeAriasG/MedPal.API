using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using MedPal.API.DTOs;
using MedPal.API.Models;
using MedPal.API.Repositories;
using MedPal.API.Services.Implementations;
using Moq;
using Xunit;

namespace MedPal.API.Tests.Services
{
    public class AppointmentServiceTests
    {
        private readonly Mock<IAppointmentRepository> _mockAppointmentRepo;
        private readonly Mock<IPatientRepository> _mockPatientRepo;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IValidator<AppointmentWriteDTO>> _mockValidator;
        private readonly AppointmentService _appointmentService;

        public AppointmentServiceTests()
        {
            _mockAppointmentRepo = new Mock<IAppointmentRepository>();
            _mockPatientRepo = new Mock<IPatientRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockValidator = new Mock<IValidator<AppointmentWriteDTO>>();

            _appointmentService = new AppointmentService(
                _mockAppointmentRepo.Object,
                _mockPatientRepo.Object,
                _mockMapper.Object,
                _mockValidator.Object);
        }

        [Fact]
        public async Task GetAllAppointmentsByIdAsync_ShouldReturnMappedAppointments()
        {
            // Arrange
            int clinicId = 1;
            var appointments = new List<Appointment> { new Appointment { Id = 1 }, new Appointment { Id = 2 } };
            var readDtos = new List<AppointmentReadDTO> { new AppointmentReadDTO { Id = 1 }, new AppointmentReadDTO { Id = 2 } };

            _mockAppointmentRepo.Setup(r => r.GetAllAppointmentsByIdAsync(clinicId, It.IsAny<int?>(), It.IsAny<DateOnly?>()))
                .ReturnsAsync(appointments);

            _mockMapper.Setup(m => m.Map<IEnumerable<AppointmentReadDTO>>(appointments))
                .Returns(readDtos);

            // Act
            var result = await _appointmentService.GetAllAppointmentsByIdAsync(clinicId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _mockAppointmentRepo.Verify(r => r.GetAllAppointmentsByIdAsync(clinicId, It.IsAny<int?>(), It.IsAny<DateOnly?>()), Times.Once);
        }

        [Fact]
        public async Task GetAppointmentByIdAsync_WhenExists_ShouldReturnMappedDTO()
        {
            // Arrange
            int appointmentId = 1;
            var appointment = new Appointment { Id = appointmentId };
            var readDto = new AppointmentReadDTO { Id = appointmentId };

            _mockAppointmentRepo.Setup(r => r.GetAppointmentByIdAsync(appointmentId))
                .ReturnsAsync(appointment);

            _mockMapper.Setup(m => m.Map<AppointmentReadDTO>(appointment))
                .Returns(readDto);

            // Act
            var result = await _appointmentService.GetAppointmentByIdAsync(appointmentId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(appointmentId, result.Id);
        }

        [Fact]
        public async Task GetAppointmentByIdAsync_WhenNotExists_ShouldReturnNull()
        {
            // Arrange
            int appointmentId = 99;
            _mockAppointmentRepo.Setup(r => r.GetAppointmentByIdAsync(appointmentId))
                .ReturnsAsync((Appointment)null);

            // Act
            var result = await _appointmentService.GetAppointmentByIdAsync(appointmentId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateAppointmentAsync_ValidRequest_ShouldCreateAndReturnDTO()
        {
            // Arrange
            var writeDto = new AppointmentWriteDTO { PatientId = 1, ClinicId = 1 };
            var appointment = new Appointment { PatientId = 1, ClinicId = 1 };
            var readDto = new AppointmentReadDTO { Id = 1, ClinicId = 1 };

            _mockValidator.Setup(v => v.ValidateAsync(writeDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockMapper.Setup(m => m.Map<Appointment>(writeDto))
                .Returns(appointment);

            _mockAppointmentRepo.Setup(r => r.AddAppointmentAsync(appointment))
                .ReturnsAsync(appointment);

            _mockMapper.Setup(m => m.Map<AppointmentReadDTO>(appointment))
                .Returns(readDto);

            // Act
            var result = await _appointmentService.CreateAppointmentAsync(writeDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            _mockAppointmentRepo.Verify(r => r.AddAppointmentAsync(appointment), Times.Once);
            _mockAppointmentRepo.Verify(r => r.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAppointmentAsync_InvalidRequest_ShouldThrowValidationException()
        {
            // Arrange
            var writeDto = new AppointmentWriteDTO();
            var validationFailure = new ValidationFailure("PatientId", "Patient is required");
            
            _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<AppointmentWriteDTO>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ValidationException(new[] { validationFailure }));

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _appointmentService.CreateAppointmentAsync(writeDto));
            _mockAppointmentRepo.Verify(r => r.AddAppointmentAsync(It.IsAny<Appointment>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAppointmentAsync_ExistingId_ShouldUpdateAndReturnDTO()
        {
            // Arrange
            int appointmentId = 1;
            var writeDto = new AppointmentWriteDTO { ClinicId = 2 };
            var existingAppointment = new Appointment { Id = appointmentId, ClinicId = 1 };
            var readDto = new AppointmentReadDTO { Id = appointmentId, ClinicId = 2 };

            _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<AppointmentWriteDTO>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockAppointmentRepo.Setup(r => r.GetAppointmentByIdAsync(appointmentId))
                .ReturnsAsync(existingAppointment);

            _mockMapper.Setup(m => m.Map(writeDto, existingAppointment))
                .Callback((AppointmentWriteDTO src, Appointment dest) => dest.ClinicId = src.ClinicId.Value);

            _mockMapper.Setup(m => m.Map<AppointmentReadDTO>(existingAppointment))
                .Returns(readDto);

            // Act
            var result = await _appointmentService.UpdateAppointmentAsync(appointmentId, writeDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.ClinicId);
            _mockAppointmentRepo.Verify(r => r.UpdateAppointment(existingAppointment), Times.Once);
            _mockAppointmentRepo.Verify(r => r.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAppointmentAsync_NonExistingId_ShouldReturnNull()
        {
            // Arrange
            int appointmentId = 99;
            var writeDto = new AppointmentWriteDTO();

            _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<AppointmentWriteDTO>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockAppointmentRepo.Setup(r => r.GetAppointmentByIdAsync(appointmentId))
                .ReturnsAsync((Appointment)null);

            // Act
            var result = await _appointmentService.UpdateAppointmentAsync(appointmentId, writeDto);

            // Assert
            Assert.Null(result);
            _mockAppointmentRepo.Verify(r => r.UpdateAppointment(It.IsAny<Appointment>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAppointmentAsync_ExistingId_ShouldReturnTrue()
        {
            // Arrange
            int appointmentId = 1;
            var appointment = new Appointment { Id = appointmentId };

            _mockAppointmentRepo.Setup(r => r.GetAppointmentByIdAsync(appointmentId))
                .ReturnsAsync(appointment);

            // Act
            var result = await _appointmentService.DeleteAppointmentAsync(appointmentId);

            // Assert
            Assert.True(result);
            _mockAppointmentRepo.Verify(r => r.UpdateAppointment(appointment), Times.Once);
            _mockAppointmentRepo.Verify(r => r.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAppointmentAsync_NonExistingId_ShouldReturnFalse()
        {
            // Arrange
            int appointmentId = 99;

            _mockAppointmentRepo.Setup(r => r.GetAppointmentByIdAsync(appointmentId))
                .ReturnsAsync((Appointment)null);

            // Act
            var result = await _appointmentService.DeleteAppointmentAsync(appointmentId);

            // Assert
            Assert.False(result);
            _mockAppointmentRepo.Verify(r => r.UpdateAppointment(It.IsAny<Appointment>()), Times.Never);
        }
    }
}
