using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MedPal.API.Data;
using MedPal.API.Models;
using MedPal.API.Repositories;
using MedPal.API.Repositories.Implementations;
using MedPal.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

namespace MedPal.API.Tests.Data
{
    public class ClinicRepositoryTests
    {
        private static AppDbContext CreateContext(string name)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"ClinicRepoTests_{name}_{Guid.NewGuid():N}")
                .Options;

            return new AppDbContext(options, new EncryptionProvider(new ConfigurationBuilder().Build()));
        }

        private static ClinicRepository CreateRepository(AppDbContext context)
        {
            return new ClinicRepository(
                context,
                Mock.Of<IMapper>(),
                Mock.Of<ITenantContextService>(),
                Mock.Of<IUserRepository>());
        }

        private static Account NewAccount(int id) => new Account
        {
            Id = id,
            Name = $"Account {id}",
            Description = "",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        private static Patient NewPatient(int id) => new Patient
        {
            Id = id,
            Name = "Patient",
            Middlename = "",
            Lastname = "Test",
            Dob = DateTime.UtcNow.AddYears(-30),
            Gender = "No especificado",
            Address = "Sin configurar",
            Phone = "",
            Email = $"patient{id}@clinicflow.test",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        private static Clinic NewClinic(int id, int? accountId, bool deleted = false) => new Clinic
        {
            Id = id,
            Name = $"Clinic {id}",
            Location = "Location",
            ContactInfo = "contact",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            AccountId = accountId,
            IsDeleted = deleted
        };

        private static async Task SeedAsync(
            AppDbContext context,
            IReadOnlyList<Account> accounts,
            IReadOnlyList<Clinic> clinics,
            IReadOnlyList<Patient> patients,
            IReadOnlyList<PatientAccount> memberships)
        {
            context.Accounts.AddRange(accounts);
            context.Clinics.AddRange(clinics);
            context.Patients.AddRange(patients);
            context.PatientAccounts.AddRange(memberships);
            await context.SaveChangesAsync();
        }

        private static PatientAccount NewMembership(int patientId, int accountId, bool primary = false, bool deleted = false) => new PatientAccount
        {
            PatientId = patientId,
            AccountId = accountId,
            IsPrimaryAccount = primary,
            IsVerifiedByPatient = true,
            ConsentToShareProfile = true,
            IsDeleted = deleted,
            CreatedAt = DateTime.UtcNow
        };

        [Fact]
        public async Task GetPatientClinicsAsync_ReturnsClinicsOfPrimaryAndSecondaryActiveMemberships()
        {
            using var context = CreateContext("memberships");
            await SeedAsync(context,
                new[] { NewAccount(100), NewAccount(200), NewAccount(300) },
                new[]
                {
                    NewClinic(1, 100),          // primary-account clinic -> eligible
                    NewClinic(2, 200),          // secondary-account clinic -> eligible
                    NewClinic(3, null),         // legacy clinic without account -> NEVER eligible
                    NewClinic(4, 100, deleted: true), // deleted clinic -> NEVER eligible
                    NewClinic(5, 300)           // clinic of unrelated account -> excluded
                },
                new[] { NewPatient(10) },
                new[]
                {
                    NewMembership(10, 100, primary: true),
                    NewMembership(10, 200)
                });

            var repo = CreateRepository(context);

            var clinics = (await repo.GetPatientClinicsAsync(10)).ToList();

            Assert.Equal(new[] { 1, 2 }, clinics.Select(c => c.Id).OrderBy(id => id));
            Assert.DoesNotContain(clinics, c => c.AccountId == null);
            Assert.DoesNotContain(clinics, c => c.IsDeleted);
        }

        [Fact]
        public async Task GetPatientClinicsAsync_ExcludesMembershipClinicWhenMembershipDeleted()
        {
            using var context = CreateContext("deletedmembership");
            await SeedAsync(context,
                new[] { NewAccount(100), NewAccount(200) },
                new[]
                {
                    NewClinic(1, 100),
                    NewClinic(2, 200)
                },
                new[] { NewPatient(11) },
                new[]
                {
                    NewMembership(11, 100, primary: true),
                    NewMembership(11, 200, deleted: true)
                });

            var repo = CreateRepository(context);

            var clinics = (await repo.GetPatientClinicsAsync(11)).ToList();

            Assert.Equal(new[] { 1 }, clinics.Select(c => c.Id));
        }

        [Fact]
        public async Task GetPatientClinicsAsync_NoMemberships_ReturnsEmpty()
        {
            using var context = CreateContext("nomemberships");
            await SeedAsync(context,
                new[] { NewAccount(100) },
                new[] { NewClinic(1, 100) },
                new[] { NewPatient(12) },
                System.Array.Empty<PatientAccount>());

            var repo = CreateRepository(context);

            var clinics = (await repo.GetPatientClinicsAsync(12)).ToList();

            Assert.Empty(clinics);
        }

        [Fact]
        public async Task GetPatientClinicsAsync_GhostPatientWithoutPrimaryMembership_ReturnsOnlyActiveLinks()
        {
            using var context = CreateContext("ghost");
            await SeedAsync(context,
                new[] { NewAccount(100), NewAccount(400) },
                new[]
                {
                    NewClinic(1, 100),
                    NewClinic(6, 400)
                },
                new[] { NewPatient(13) },
                new[]
                {
                    NewMembership(13, 400, primary: true)
                });

            var repo = CreateRepository(context);

            var clinics = (await repo.GetPatientClinicsAsync(13)).ToList();

            Assert.Equal(new[] { 6 }, clinics.Select(c => c.Id));
        }
    }
}