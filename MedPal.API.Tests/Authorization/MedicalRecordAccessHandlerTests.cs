using System.Security.Claims;
using System.Threading.Tasks;
using MedPal.API.Authorization;
using MedPal.API.Models;
using MedPal.API.Repositories;
using MedPal.API.Repositories.Authorization;
using MedPal.API.Services;
using Microsoft.AspNetCore.Authorization;
using Moq;
using Xunit;

namespace MedPal.API.Tests.Authorization
{
    public class MedicalRecordAccessHandlerTests
    {
        private readonly Mock<IMedicalHistoryRepository> _historyRepo;
        private readonly Mock<IPermissionRepository> _permissionRepo;
        private readonly Mock<IPatientConsentService> _consentService;
        private readonly MedicalRecordAccessHandler _handler;

        public MedicalRecordAccessHandlerTests()
        {
            _historyRepo = new Mock<IMedicalHistoryRepository>();
            _permissionRepo = new Mock<IPermissionRepository>();
            _consentService = new Mock<IPatientConsentService>();
            _handler = new MedicalRecordAccessHandler(_historyRepo.Object, _permissionRepo.Object, _consentService.Object);
        }

        private async Task<bool> AuthorizeAsync(ClaimsPrincipal principal, int recordId, bool allowSelf = true)
        {
            var requirement = new MedicalRecordAccessRequirement(recordId, allowSelf);
            var context = new AuthorizationHandlerContext(new IAuthorizationRequirement[] { requirement }, principal, null);
            await _handler.HandleAsync(context);
            return context.HasSucceeded;
        }

        private static ClaimsPrincipal StaffPrincipal(int userId)
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }, "test");
            return new ClaimsPrincipal(identity);
        }

        private static ClaimsPrincipal PatientPrincipal(int userId)
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim("patient_id", "1")
            }, "test");
            return new ClaimsPrincipal(identity);
        }

        private static MedicalHistory CreateHistory(int creatorUserId, int? patientUserId = null)
        {
            var history = new MedicalHistory
            {
                Id = 1,
                PatientDetailsId = 10,
                HealthcareProfessionalId = creatorUserId,
                CreatedByUserId = creatorUserId,
                PatientDetails = patientUserId.HasValue
                    ? new PatientDetails
                    {
                        Id = 10,
                        Patient = new Patient { Id = 1, UserId = patientUserId.Value }
                    }
                    : null
            };
            return history;
        }

        private void SetupRecord(MedicalHistory history)
        {
            _historyRepo.Setup(r => r.GetMedicalHistoryByIdAsync(history.Id)).ReturnsAsync(history);
        }

        private void SetupAdminPermissions(bool isAdmin)
        {
            _permissionRepo.Setup(p => p.UserHasPermissionAsync(It.IsAny<int>(), "MedicalRecords.ViewAll", null)).ReturnsAsync(isAdmin);
            _consentService.Setup(c => c.IsConsentForDoctorValidAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(false);
        }

        [Fact]
        public async Task CreatorOfRecord_AlwaysHasAccess()
        {
            var history = CreateHistory(creatorUserId: 7);
            SetupRecord(history);
            SetupAdminPermissions(isAdmin: false);

            Assert.True(await AuthorizeAsync(StaffPrincipal(7), 1));
        }

        [Fact]
        public async Task AdminWithViewAll_HasAccess()
        {
            var history = CreateHistory(creatorUserId: 7);
            SetupRecord(history);
            SetupAdminPermissions(isAdmin: true);

            Assert.True(await AuthorizeAsync(StaffPrincipal(99), 1));
        }

        [Fact]
        public async Task Patient_OwnRecord_ReadFlow_Succeeds()
        {
            var history = CreateHistory(creatorUserId: 7, patientUserId: 100);
            SetupRecord(history);
            SetupAdminPermissions(isAdmin: false);

            Assert.True(await AuthorizeAsync(PatientPrincipal(100), 1, allowSelf: true));
        }

        [Fact]
        public async Task Patient_OwnRecord_WriteFlow_Denied()
        {
            var history = CreateHistory(creatorUserId: 7, patientUserId: 100);
            SetupRecord(history);
            SetupAdminPermissions(isAdmin: false);

            Assert.False(await AuthorizeAsync(PatientPrincipal(100), 1, allowSelf: false));
        }

        [Fact]
        public async Task ConsentValid_GrantsAccess()
        {
            var history = CreateHistory(creatorUserId: 7);
            SetupRecord(history);
            _permissionRepo.Setup(p => p.UserHasPermissionAsync(It.IsAny<int>(), "MedicalRecords.ViewAll", null)).ReturnsAsync(false);
            _consentService.Setup(c => c.IsConsentForDoctorValidAsync(10, 88)).ReturnsAsync(true);

            Assert.True(await AuthorizeAsync(StaffPrincipal(88), 1));
        }

        [Fact]
        public async Task NoMatchingRule_DeniesAccess()
        {
            var history = CreateHistory(creatorUserId: 7);
            SetupRecord(history);
            SetupAdminPermissions(isAdmin: false);

            Assert.False(await AuthorizeAsync(StaffPrincipal(88), 1));
        }

        [Fact]
        public async Task RecordNotFound_DeniesAccess()
        {
            _historyRepo.Setup(h => h.GetMedicalHistoryByIdAsync(1)).ReturnsAsync((MedicalHistory?)null);
            SetupAdminPermissions(isAdmin: true);

            Assert.False(await AuthorizeAsync(StaffPrincipal(88), 1));
        }
    }
}