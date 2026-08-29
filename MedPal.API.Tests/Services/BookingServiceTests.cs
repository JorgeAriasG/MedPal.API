using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MedPal.API.Data;
using MedPal.API.DTOs;
using MedPal.API.Exceptions;
using MedPal.API.Infrastructure;
using MedPal.API.Mapping;
using MedPal.API.Models;
using MedPal.API.Repositories;
using MedPal.API.Repositories.Implementations;
using MedPal.API.Services;
using MedPal.API.Services.Implementations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MedPal.API.Tests.Services
{
    /// <summary>
    /// Pruebas de BookingService sobre SQLite en memoria (transacciones REALES:
    /// BeginTransaction/Complete/Rollback del IUnitOfWork sobre el mismo contexto).
    /// El IAppointmentService es mock: el foco es el flujo ghost → token dentro de
    /// una sola transacción y la garantía de rollback cuando algo falla.
    /// </summary>
    public class BookingServiceTests : IDisposable
    {
        private const string JwtKey = "test-secret-key-0123456789abcdef";

        private readonly SqliteConnection _connection;
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        private readonly IBookingLinkService _bookingLink;
        private readonly BookingService _serviceWithRealPatientRepo;
        private readonly BookingService _serviceWithMockedPatientRepo;
        private readonly Mock<IAppointmentService> _appointmentServiceMock;
        private readonly Mock<IClinicRepository> _clinicRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IRegistrationNotificationService> _notificationMock;

        public BookingServiceTests()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new AppDbContext(options, new EncryptionProvider(new ConfigurationBuilder().Build()));
            _context.Database.EnsureCreated();

            _mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();

            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["Jwt:Key"] = JwtKey,
                    ["Jwt:Issuer"] = "https://api.clinicflow.com.mx",
                    ["Jwt:Audience"] = "clinicflow",
                    ["Booking:PublicBaseUrl"] = "https://portal.clinicflow.com.mx/booking"
                })
                .Build();

            _bookingLink = new BookingLinkService(_config);

            _appointmentServiceMock = new Mock<IAppointmentService>();
            _appointmentServiceMock
                .Setup(x => x.CreateAppointmentAsync(It.IsAny<AppointmentWriteDTO>()))
                .ReturnsAsync(new AppointmentReadDTO { Id = 100 });

            _clinicRepoMock = new Mock<IClinicRepository>();
            _clinicRepoMock
                .Setup(x => x.GetClinicByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => new Clinic
                {
                    Id = id,
                    Name = $"Clinic {id}",
                    Location = "Test",
                    ContactInfo = "contact",
                    Open = new TimeOnly(8, 0),
                    Close = new TimeOnly(10, 0),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });

            _userRepoMock = new Mock<IUserRepository>();
            _userRepoMock
                .Setup(x => x.GetUserByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => new User
                {
                    Id = id,
                    Name = "Doctor",
                    Email = $"doctor{id}@clinicflow.test",
                    ClinicId = 1,
                    IsDeleted = false
                });
            _userRepoMock
                .Setup(x => x.GetByIdIgnoreTenantAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => new User
                {
                    Id = id,
                    Name = "Doctor",
                    Email = $"doctor{id}@clinicflow.test",
                    ClinicId = 1,
                    IsDeleted = false
                });

            _notificationMock = new Mock<IRegistrationNotificationService>();

            _serviceWithRealPatientRepo = CreateService(ctx => new PatientRepository(ctx, _mapper, Mock.Of<ITenantContextService>()));
            _serviceWithMockedPatientRepo = CreateService(_ => Mock.Of<IPatientRepository>());
        }

        private BookingService CreateService(Func<AppDbContext, IPatientRepository> patientRepoFactory)
        {
            var unitOfWork = new UnitOfWork(_context);

            var appointmentRepoMock = new Mock<IAppointmentRepository>();
            appointmentRepoMock
                .Setup(x => x.GetAllAppointmentsByIdAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<DateOnly?>()))
                .ReturnsAsync(new List<Appointment>());
            appointmentRepoMock
                .Setup(x => x.GetPublicOverlapAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateOnly>()))
                .ReturnsAsync(new List<Appointment>());

            return new BookingService(
                patientRepoFactory(_context),
                new PatientRegistrationTokenRepository(_context),
                appointmentRepoMock.Object,
                _appointmentServiceMock.Object,
                _bookingLink,
                _clinicRepoMock.Object,
                _userRepoMock.Object,
                _notificationMock.Object,
                unitOfWork,
                _config,
                Mock.Of<ILogger<BookingService>>());
        }

        private string CreateShareToken() => _bookingLink.Issue(1, 2);

        private static BookingCompleteDTO NewGhostDto(string sr) => new BookingCompleteDTO
        {
            Sr = sr,
            PatientName = "Ana María Rodríguez",
            PatientPhone = "5522334455",
            Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            Time = new TimeOnly(9, 0),
            DurationMinutes = 30,
            ConsentMedicalRecords = true,
            ConsentWhatsapp = true
        };

        [Fact]
        public async Task CompleteBooking_GhostFlow_CommitsPatientAndToken_AndSendsWhatsApp()
        {
            var sr = CreateShareToken();
            _context.Clinics.Add(new Clinic
            {
                Id = 1,
                Name = "Clinic 1",
                Location = "Test",
                ContactInfo = "contact",
                Open = new TimeOnly(8, 0),
                Close = new TimeOnly(10, 0),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                AccountId = null
            });
            await _context.SaveChangesAsync();

            var result = await _serviceWithRealPatientRepo.CompleteBookingAsync(null, sr, NewGhostDto(sr));

            Assert.Equal(100, result.AppointmentId);
            Assert.True(result.PendingRegistration);

            var patient = await _context.Patients.SingleAsync();
            Assert.Equal("525522334455", patient.Phone);

            var token = await _context.PatientRegistrationTokens.SingleAsync();
            Assert.Equal("pending", token.Status);
            Assert.True(token.ExpiresAt > DateTime.UtcNow);

            await _context.PatientClinics.SingleAsync();

            Mock.Get(_notificationMock.Object)
                .Verify(x => x.SendRegistrationLinkAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task CompleteBooking_AppointmentFailure_RollsBackGhostAndToken()
        {
            var sr = CreateShareToken();
            _context.Clinics.Add(new Clinic
            {
                Id = 1,
                Name = "Clinic 1",
                Location = "Test",
                ContactInfo = "contact",
                Open = new TimeOnly(8, 0),
                Close = new TimeOnly(10, 0),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            _appointmentServiceMock
                .Setup(x => x.CreateAppointmentAsync(It.IsAny<AppointmentWriteDTO>()))
                .ThrowsAsync(new InvalidOperationException("fallo simulado"));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _serviceWithRealPatientRepo.CompleteBookingAsync(null, sr, NewGhostDto(sr)));

            Assert.Equal(0, await _context.Patients.CountAsync());
            Assert.Equal(0, await _context.PatientRegistrationTokens.CountAsync());
            Assert.Equal(0, await _context.PatientClinics.CountAsync());

            Mock.Get(_notificationMock.Object)
                .Verify(x => x.SendRegistrationLinkAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task CompleteBooking_MissingShareAndSession_ThrowsUnauthorized()
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _serviceWithRealPatientRepo.CompleteBookingAsync(null, null, NewGhostDto("")));
        }

        [Fact]
        public async Task CompleteBooking_WithAuthPatientId_NoGhostNorToken()
        {
            var dto = NewGhostDto("");
            dto.ClinicId = 1;
            dto.DoctorId = 2;

            var result = await _serviceWithRealPatientRepo.CompleteBookingAsync(42, null, dto);

            Assert.Equal(100, result.AppointmentId);
            Assert.False(result.PendingRegistration);
            Assert.Equal(0, await _context.PatientRegistrationTokens.CountAsync());
            Assert.Equal(0, await _context.Patients.CountAsync());
        }

        [Fact]
        public async Task Availability_WithSr_ReturnsOpenSlots()
        {
            var sr = CreateShareToken();

            var slots = (await _serviceWithRealPatientRepo.GetPublicAvailabilityAsync(
                sr, null, null, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), null)).ToList();

            Assert.Equal(4, slots.Count); // 08:00 - 10:00 con slots de 30 min
            Assert.All(slots, s => Assert.True(s.IsAvailable));

            _userRepoMock.Verify(x => x.GetByIdIgnoreTenantAsync(2), Times.Once);
            _userRepoMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task CompleteBooking_Anonymous_ResolvesDoctorIgnoringTenant()
        {
            var sr = CreateShareToken();
            _context.Clinics.Add(new Clinic
            {
                Id = 1,
                Name = "Clinic 1",
                Location = "Test",
                ContactInfo = "contact",
                Open = new TimeOnly(8, 0),
                Close = new TimeOnly(10, 0),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            await _serviceWithRealPatientRepo.CompleteBookingAsync(null, sr, NewGhostDto(sr));

            _userRepoMock.Verify(x => x.GetByIdIgnoreTenantAsync(2), Times.Once);
            _userRepoMock.Verify(x => x.GetUserByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GenerateStaffLink_ValidUser_ReturnsBookingUrl()
        {
            var patientRepoMock = new Mock<IPatientRepository>();
            patientRepoMock.Setup(x => x.UserBelongsToClinicAsync(42, 1)).ReturnsAsync(true);

            var service = CreateService(_ => patientRepoMock.Object);

            var link = await service.GenerateStaffLinkAsync(42, new BookingLinkStaffDTO
            {
                ClinicId = 1,
                DoctorId = 2
            });

            Assert.StartsWith("https://portal.clinicflow.com.mx/booking?sr=", link.Url);
            Assert.NotNull(_bookingLink.Validate(link.Url.Split("sr=")[1]));
        }

        [Fact]
        public async Task GenerateStaffLink_UserNotInClinic_ThrowsForbidden()
        {
            var patientRepoMock = new Mock<IPatientRepository>();
            patientRepoMock.Setup(x => x.UserBelongsToClinicAsync(7, 1)).ReturnsAsync(false);

            var service = CreateService(_ => patientRepoMock.Object);

            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                service.GenerateStaffLinkAsync(7, new BookingLinkStaffDTO
                {
                    ClinicId = 1,
                    DoctorId = 2
                }));
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Dispose();
        }
    }
}