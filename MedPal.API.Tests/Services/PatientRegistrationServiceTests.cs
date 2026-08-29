using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MedPal.API.Data;
using MedPal.API.DTOs;
using MedPal.API.Infrastructure;
using MedPal.API.Mapping;
using MedPal.API.Models;
using MedPal.API.Repositories;
using MedPal.API.Repositories.Implementations;
using MedPal.API.Services;
using MedPal.API.Services.Implementations;
using MedPal.API.Utils;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MedPal.API.Tests.Services
{
    /// <summary>
    /// Pruebas de PatientRegistrationService sobre SQLite en memoria con repos REALES de
    /// tokens/patients/auth (transacciones y ejecución SQL condicional reales). El foco es
    /// la garantía de atomicidad: si el alta de determinaciones del paciente falla después de
    /// consumir el token, el token NO queda quemado (rollback).
    /// </summary>
    public class PatientRegistrationServiceTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly Moq.Mock<IPatientTokenService> _tokenServiceMock;
        private readonly Moq.Mock<IRegistrationNotificationService> _notificationMock;
        private readonly PatientRegistrationService _service;

        public PatientRegistrationServiceTests()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new AppDbContext(options, new EncryptionProvider(new ConfigurationBuilder().Build()));
            _context.Database.EnsureCreated();

            _mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();

            _tokenServiceMock = new Moq.Mock<IPatientTokenService>();
            _tokenServiceMock.Setup(x => x.GeneratePatientToken(It.IsAny<Patient>(), It.IsAny<string>()))
                .Returns("jwt-test");

            _notificationMock = new Moq.Mock<IRegistrationNotificationService>();

            _service = new PatientRegistrationService(
                new PatientAuthRepository(_context),
                new PatientRepository(_context, _mapper, Mock.Of<ITenantContextService>()),
                new PatientRegistrationTokenRepository(_context),
                _tokenServiceMock.Object,
                _notificationMock.Object,
                new UnitOfWork(_context),
                Mock.Of<ILogger<PatientRegistrationService>>());
        }

        /// <summary>
        /// Crea un paciente ghost real (con PatientDetails) y un token pending real.
        /// Devuelve el token raw para usarlo contra el servicio.
        /// </summary>
        private async Task<string> SeedGhostAndTokenAsync(int resendCount = 0, DateTime? expiresAt = null)
        {
            var patientRepo = new PatientRepository(_context, _mapper, Mock.Of<ITenantContextService>());
            var patient = await patientRepo.AddPatientAsync(NewGhost("Ana", "Rodríguez"));

            var rawToken = TokenGenerator.GenerateRawToken();
            var tokenRepo = new PatientRegistrationTokenRepository(_context);
            await tokenRepo.CreateAsync(new PatientRegistrationToken
            {
                PatientId = patient.Id,
                TokenHash = TokenGenerator.Sha256Hex(rawToken),
                Status = "pending",
                ResendCount = resendCount,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt ?? DateTime.UtcNow.AddHours(72)
            });

            await _context.SaveChangesAsync();
            return rawToken;
        }

        private static Patient NewGhost(string name, string lastname) => new Patient
        {
            Name = name,
            Middlename = "",
            Lastname = lastname,
            Dob = DateTime.UtcNow.AddYears(-30),
            Gender = "No especificado",
            Address = "Sin configurar",
            Phone = "525522334455",
            Email = $"pendiente_{Guid.NewGuid():N}@clinicflow.temp",
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        private static CompletePatientRegistrationDTO NewDto(string rawToken) => new CompletePatientRegistrationDTO
        {
            Token = rawToken,
            Email = "PACIENTE@CLINICFLOW.COM",
            Password = "supersecreto1"
        };

        [Fact]
        public async Task Complete_Success_ConsumesToken_ReturnsJwt_AndUpdatesPatient()
        {
            var rawToken = await SeedGhostAndTokenAsync();
            var patient = await _context.Patients.SingleAsync();

            var response = await _service.CompletePatientRegistrationAsync(NewDto(rawToken));

            Assert.Equal("jwt-test", response.Token);
            Assert.Equal("paciente@clinicflow.com", response.Email);

            var stored = await _context.PatientRegistrationTokens.AsNoTracking().SingleAsync();
            Assert.Equal("used", stored.Status);
            Assert.NotNull(stored.UsedAt);

            var auth = await _context.PatientAuths.SingleAsync();
            Assert.Equal("paciente@clinicflow.com", auth.Email);
            Assert.NotEmpty(auth.PasswordHash);

            var updated = await _context.Patients.SingleAsync(p => p.Id == patient.Id);
            Assert.Equal("paciente@clinicflow.com", updated.Email);
        }

        [Fact]
        public async Task Complete_FailureAfterConsume_LeavesTokenPending_AndNoAuth()
        {
            var rawToken = await SeedGhostAndTokenAsync();

            var authRepoMock = new Moq.Mock<IPatientAuthRepository>();
            authRepoMock.Setup(x => x.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            authRepoMock.Setup(x => x.CreateAsync(It.IsAny<PatientAuth>()))
                .ThrowsAsync(new InvalidOperationException("fallo simulado al crear credenciales"));

            var service = new PatientRegistrationService(
                authRepoMock.Object,
                new PatientRepository(_context, _mapper, Mock.Of<ITenantContextService>()),
                new PatientRegistrationTokenRepository(_context),
                _tokenServiceMock.Object,
                _notificationMock.Object,
                new UnitOfWork(_context),
                Mock.Of<ILogger<PatientRegistrationService>>());

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.CompletePatientRegistrationAsync(NewDto(rawToken)));

            var stored = await _context.PatientRegistrationTokens.AsNoTracking().SingleAsync();
            Assert.Equal("pending", stored.Status);
            Assert.Null(stored.UsedAt);
            Assert.Equal(0, await _context.PatientAuths.CountAsync());
        }

        [Fact]
        public async Task Complete_ExpiredToken_RevokesAndThrows()
        {
            var rawToken = await SeedGhostAndTokenAsync(expiresAt: DateTime.UtcNow.AddMinutes(-1));

            var ex = await Assert.ThrowsAsync<ValidationException>(() =>
                _service.CompletePatientRegistrationAsync(NewDto(rawToken)));

            Assert.StartsWith("Token expirado", ex.Message);
            var stored = await _context.PatientRegistrationTokens.AsNoTracking().SingleAsync();
            Assert.Equal("revoked", stored.Status);
        }

        [Fact]
        public async Task Complete_DoubleUse_Throws()
        {
            var rawToken = await SeedGhostAndTokenAsync();
            await _service.CompletePatientRegistrationAsync(NewDto(rawToken));

            var ex = await Assert.ThrowsAsync<ValidationException>(() =>
                _service.CompletePatientRegistrationAsync(NewDto(rawToken)));

            Assert.StartsWith("Token inválido o ya utilizado", ex.Message);
        }

        [Fact]
        public async Task Complete_UnknownOrAlreadyUsedToken_Throws()
        {
            var ex = await Assert.ThrowsAsync<ValidationException>(() =>
                _service.CompletePatientRegistrationAsync(NewDto("invalid-token")));

            Assert.StartsWith("Token inválido o ya utilizado", ex.Message);
        }

        [Fact]
        public async Task Resend_RevokesPrevious_EmitsNewAndIncrements_AndSends()
        {
            var rawOld = await SeedGhostAndTokenAsync();
            var patient = await _context.Patients.SingleAsync();

            var message = await _service.ResendRegistrationAsync(new ResendRegistrationDTO
            {
                Phone = "5522334455"
            });

            Assert.Equal("Se ha enviado un nuevo mensaje de confirmación.", message);

            var tokens = await _context.PatientRegistrationTokens
                .AsNoTracking()
                .OrderBy(t => t.Id)
                .ToListAsync();
            Assert.Equal(2, tokens.Count);
            Assert.Equal("revoked", tokens[0].Status);
            Assert.Equal("pending", tokens[1].Status);
            Assert.Equal(1, tokens[1].ResendCount);
            Assert.NotEqual(TokenGenerator.Sha256Hex(rawOld), tokens[1].TokenHash);

            Mock.Get(_notificationMock.Object)
                .Verify(x => x.SendRegistrationLinkAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            Assert.Equal(patient.Id, tokens[1].PatientId);
        }

        [Fact]
        public async Task Resend_OverLimit_Throws()
        {
            await SeedGhostAndTokenAsync(resendCount: 3);

            var ex = await Assert.ThrowsAsync<ValidationException>(() =>
                _service.ResendRegistrationAsync(new ResendRegistrationDTO { Phone = "5522334455" }));

            Assert.Equal("Límite de reenvíos alcanzado.", ex.Message);
        }

        [Fact]
        public async Task Resend_UnknownPhone_ThrowsKeyNotFound()
        {
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.ResendRegistrationAsync(new ResendRegistrationDTO { Phone = "9999999999" }));
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Dispose();
        }
    }
}