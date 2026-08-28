using System;
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
using Xunit;

namespace MedPal.API.Tests.Data
{
    public class PatientRepositoryTests
    {
        private static AppDbContext CreateContext(string name)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"PatientRepoTests_{name}_{Guid.NewGuid():N}")
                .Options;

            return new AppDbContext(options, new EncryptionProvider(new ConfigurationBuilder().Build()));
        }

        private static PatientRepository CreateRepository(AppDbContext context)
        {
            return new PatientRepository(
                context,
                Mock.Of<IMapper>(),
                Mock.Of<ITenantContextService>());
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

        private static Clinic NewClinic(int id, int? accountId) => new Clinic
        {
            Id = id,
            Name = $"Clinic {id}",
            Location = "Location",
            ContactInfo = "contact",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            AccountId = accountId,
            IsDeleted = false
        };

        private static PatientAccount NewMembership(
            int patientId, int accountId, bool primary = false, bool verified = true, bool? consent = true) => new PatientAccount
        {
            PatientId = patientId,
            AccountId = accountId,
            IsPrimaryAccount = primary,
            IsVerifiedByPatient = verified,
            ConsentToShareProfile = consent,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        private static PatientClinic NewClinicLink(int patientId, int clinicId) => new PatientClinic
        {
            PatientId = patientId,
            ClinicId = clinicId,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        [Fact]
        public async Task GetAllPatientsAsync_RosterScopedToEligibleMembershipOfOwningAccount()
        {
            using var context = CreateContext("roster-eligible");
            var p10 = NewPatient(10); var p11 = NewPatient(11); var p12 = NewPatient(12);
            var clinic1 = NewClinic(1, 100);
            p10.PatientAccounts!.Add(NewMembership(10, 100, primary: true));
            p11.PatientAccounts!.Add(NewMembership(11, 100, verified: false, consent: false));
            p12.PatientClinics!.Add(NewClinicLink(12, 1));

            context.Accounts.Add(NewAccount(100));
            context.Clinics.Add(clinic1);
            context.Patients.AddRange(p10, p11, p12);
            await context.SaveChangesAsync();

            var repo = CreateRepository(context);

            var roster = (await repo.GetAllPatientsAsync(1)).ToList();

            Assert.Equal(new[] { 10 }, roster.Select(p => p.Id));
        }

        [Fact]
        public async Task GetAllPatientsAsync_ExcludesPatientsOfOtherAccounts()
        {
            using var context = CreateContext("roster-other-account");
            var p20 = NewPatient(20);
            var clinic1 = NewClinic(1, 100);
            p20.PatientAccounts!.Add(NewMembership(20, 200, primary: true));

            context.Accounts.Add(NewAccount(100));
            context.Accounts.Add(NewAccount(200));
            context.Clinics.Add(clinic1);
            context.Patients.Add(p20);
            await context.SaveChangesAsync();

            var repo = CreateRepository(context);

            var roster = (await repo.GetAllPatientsAsync(1)).ToList();

            Assert.Empty(roster);
        }

        [Fact]
        public async Task GetAllPatientsAsync_TenantlessClinicFallsBackToClinicLinkRoster()
        {
            using var context = CreateContext("roster-tenantless");
            var p30 = NewPatient(30);
            var clinicNoAccount = NewClinic(1, null);
            p30.PatientClinics!.Add(NewClinicLink(30, 1));

            context.Clinics.Add(clinicNoAccount);
            context.Patients.Add(p30);
            await context.SaveChangesAsync();

            var repo = CreateRepository(context);

            var roster = (await repo.GetAllPatientsAsync(1)).ToList();

            Assert.Equal(new[] { 30 }, roster.Select(p => p.Id));
        }

        [Fact]
        public async Task GetAllPatientsAsync_SearchStillApplies()
        {
            using var context = CreateContext("roster-search");
            var p40 = NewPatient(40); p40.Name = "Zulema";
            var p41 = NewPatient(41); p41.Name = "Beto";
            var clinic1 = NewClinic(1, 100);
            p40.PatientAccounts!.Add(NewMembership(40, 100, primary: true));
            p41.PatientAccounts!.Add(NewMembership(41, 100, primary: true));

            context.Accounts.Add(NewAccount(100));
            context.Clinics.Add(clinic1);
            context.Patients.AddRange(p40, p41);
            await context.SaveChangesAsync();

            var repo = CreateRepository(context);

            var roster = (await repo.GetAllPatientsAsync(1, search: "zul")).ToList();

            Assert.Equal(new[] { 40 }, roster.Select(p => p.Id));
        }
    }
}