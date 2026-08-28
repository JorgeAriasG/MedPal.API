using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using MedPal.API.Authorization;
using MedPal.API.Models;
using MedPal.API.Repositories;
using MedPal.API.Services;
using Microsoft.AspNetCore.Authorization;
using Moq;
using Xunit;

namespace MedPal.API.Tests.Authorization
{
    public class PatientAccessHandlerTests
    {
        private readonly Mock<IPatientRepository> _patientRepo;
        private readonly Mock<ITenantContextService> _tenantContext;
        private readonly PatientAccessHandler _handler;

        public PatientAccessHandlerTests()
        {
            _patientRepo = new Mock<IPatientRepository>();
            _tenantContext = new Mock<ITenantContextService>();
            _handler = new PatientAccessHandler(_patientRepo.Object, _tenantContext.Object);
        }

        private async Task<bool> AuthorizeAsync(ClaimsPrincipal principal, int patientId)
        {
            var requirement = new PatientAccessRequirement(patientId);
            var handlers = new[] { _handler };
            var context = new AuthorizationHandlerContext(new IAuthorizationRequirement[] { requirement }, principal, null);
            foreach (var h in handlers)
            {
                await h.HandleAsync(context);
            }
            return context.HasSucceeded;
        }

        private static ClaimsPrincipal StaffPrincipal(int accountId, int clinicId, int userId = 7)
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim("account_id", accountId.ToString()),
                new Claim("clinic_id", clinicId.ToString())
            }, "test");
            return new ClaimsPrincipal(identity);
        }

        private static ClaimsPrincipal PortalPrincipal(int patientId)
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim("patient_id", patientId.ToString())
            }, "test");
            return new ClaimsPrincipal(identity);
        }

        private static ClaimsPrincipal SuperAdminPrincipal()
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim("role", "SuperAdmin")
            }, "test");
            return new ClaimsPrincipal(identity);
        }

        private static Patient CreatePatient(int id, int? primaryAccountId = null, int clinicId = 1)
        {
            var patient = new Patient
            {
                Id = id,
                PatientClinics = new List<PatientClinic>
                {
                    new PatientClinic { ClinicId = clinicId, IsDeleted = false }
                },
                PatientAccounts = new List<PatientAccount>()
            };

            if (primaryAccountId.HasValue)
            {
                patient.PatientAccounts.Add(new PatientAccount
                {
                    PatientId = id,
                    AccountId = primaryAccountId.Value,
                    IsPrimaryAccount = true,
                    IsVerifiedByPatient = true,
                    ConsentToShareProfile = true,
                    IsDeleted = false
                });
            }

            return patient;
        }

        private void SetupStaffContext(int accountId, int clinicId)
        {
            _tenantContext.SetupGet(t => t.CurrentAccountId).Returns(accountId);
            _tenantContext.SetupGet(t => t.CurrentClinicId).Returns(clinicId);
            _tenantContext.SetupGet(t => t.IsSuperAdmin).Returns(false);
        }

        [Fact]
        public async Task StaffSameAccount_Succeeds()
        {
            var patient = CreatePatient(1, primaryAccountId: 1);
            _patientRepo.Setup(r => r.GetPatientByIdAsync(1)).ReturnsAsync(patient);
            SetupStaffContext(accountId: 1, clinicId: 1);

            Assert.True(await AuthorizeAsync(StaffPrincipal(1, 1), 1));
        }

        [Fact]
        public async Task StaffOfAnotherAccount_WithoutClinicLink_Fails()
        {
            var patient = CreatePatient(1, primaryAccountId: 2, clinicId: 9);
            _patientRepo.Setup(r => r.GetPatientByIdAsync(1)).ReturnsAsync(patient);
            SetupStaffContext(accountId: 1, clinicId: 1);

            Assert.False(await AuthorizeAsync(StaffPrincipal(1, 1), 1));
        }

        [Fact]
        public async Task StaffOfAnotherAccount_ButClinicLinked_Succeeds()
        {
            var patient = CreatePatient(1, primaryAccountId: 2, clinicId: 1);
            _patientRepo.Setup(r => r.GetPatientByIdAsync(1)).ReturnsAsync(patient);
            SetupStaffContext(accountId: 1, clinicId: 1);

            Assert.True(await AuthorizeAsync(StaffPrincipal(1, 1), 1));
        }

        [Fact]
        public async Task StaffCrossAccountVerifiedMembership_Succeeds()
        {
            var patient = CreatePatient(1, primaryAccountId: 2, clinicId: 5);
            patient.PatientAccounts!.Add(new PatientAccount
            {
                PatientId = 1,
                AccountId = 1,
                IsPrimaryAccount = false,
                IsVerifiedByPatient = true,
                ConsentToShareProfile = true,
                IsDeleted = false
            });
            _patientRepo.Setup(r => r.GetPatientByIdAsync(1)).ReturnsAsync(patient);
            SetupStaffContext(accountId: 1, clinicId: 5);

            Assert.True(await AuthorizeAsync(StaffPrincipal(1, 5), 1));
        }

        [Fact]
        public async Task StaffCrossAccountUnverifiedMembership_Fails()
        {
            var patient = CreatePatient(1, primaryAccountId: 2, clinicId: 6);
            patient.PatientAccounts!.Add(new PatientAccount
            {
                PatientId = 1,
                AccountId = 1,
                IsPrimaryAccount = false,
                IsVerifiedByPatient = false,
                ConsentToShareProfile = false,
                IsDeleted = false
            });
            _patientRepo.Setup(r => r.GetPatientByIdAsync(1)).ReturnsAsync(patient);
            SetupStaffContext(accountId: 1, clinicId: 5);

            Assert.False(await AuthorizeAsync(StaffPrincipal(1, 5), 1));
        }

        [Fact]
        public async Task StaffClinicLinkedToPatientWithoutAccount_Succeeds()
        {
            var patient = CreatePatient(1, clinicId: 3);
            _patientRepo.Setup(r => r.GetPatientByIdAsync(1)).ReturnsAsync(patient);
            SetupStaffContext(accountId: 1, clinicId: 3);

            Assert.True(await AuthorizeAsync(StaffPrincipal(1, 3), 1));
        }

        [Fact]
        public async Task StaffNotLinkedToPatientWithoutAccount_Fails()
        {
            var patient = CreatePatient(1, clinicId: 5);
            _patientRepo.Setup(r => r.GetPatientByIdAsync(1)).ReturnsAsync(patient);
            SetupStaffContext(accountId: 1, clinicId: 3);

            Assert.False(await AuthorizeAsync(StaffPrincipal(1, 3), 1));
        }

        [Fact]
        public async Task PortalPatient_OwnRecord_Succeeds()
        {
            var patient = CreatePatient(10);
            _patientRepo.Setup(r => r.GetPatientByIdAsync(10)).ReturnsAsync(patient);

            Assert.True(await AuthorizeAsync(PortalPrincipal(10), 10));
        }

        [Fact]
        public async Task PortalPatient_OtherRecord_Fails()
        {
            var patient = CreatePatient(10);
            _patientRepo.Setup(r => r.GetPatientByIdAsync(10)).ReturnsAsync(patient);

            Assert.False(await AuthorizeAsync(PortalPrincipal(99), 10));
        }

        [Fact]
        public async Task SuperAdmin_AnyPatient_Succeeds()
        {
            var patient = CreatePatient(1, primaryAccountId: 9);
            _patientRepo.Setup(r => r.GetPatientByIdAsync(1)).ReturnsAsync(patient);
            _tenantContext.SetupGet(t => t.IsSuperAdmin).Returns(true);

            Assert.True(await AuthorizeAsync(SuperAdminPrincipal(), 1));
        }

        [Fact]
        public async Task PatientNotFound_Fails()
        {
            _patientRepo.Setup(r => r.GetPatientByIdAsync(1)).ReturnsAsync((Patient?)null);
            SetupStaffContext(accountId: 1, clinicId: 1);

            Assert.False(await AuthorizeAsync(StaffPrincipal(1, 1), 1));
        }
    }
}