using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using MedPal.API.Controllers;
using MedPal.API.DTOs;
using MedPal.API.Models;
using MedPal.API.Repositories;
using MedPal.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Moq;

namespace MedPal.API.Tests.Controllers
{
    public class ClinicControllerTests
    {
        private static ClinicController CreateController(
            Mock<IClinicRepository> repo,
            Mock<IMapper> mapper,
            IConfiguration configuration)
        {
            var controller = new ClinicController(
                repo.Object,
                mapper.Object,
                Mock.Of<IUserService>(),
                Mock.Of<ISubscriptionService>(),
                configuration);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity("test")) }
            };

            return controller;
        }

        private static Mock<IMapper> CreateMapper()
        {
            var mapper = new Mock<IMapper>();
            mapper
                .Setup(m => m.Map<IEnumerable<ClinicReadDTO>>(It.IsAny<object>()))
                .Returns((object src) =>
                    ((IEnumerable<Clinic>)src)
                    .Select(c => new ClinicReadDTO { Id = c.Id, Name = c.Name })
                    .ToList());
            return mapper;
        }

        private static IConfiguration ConfigurationWithClinicFlagEnabled(bool enabled)
        {
            var data = new Dictionary<string, string?>
            {
                ["Discovery:AllowAnonymousPublicClinics"] = enabled.ToString()
            };
            return new ConfigurationBuilder().AddInMemoryCollection(data).Build();
        }

        private void SetPatientPrincipal(ClinicController controller, string patientId)
        {
            var identity = new ClaimsIdentity(
                System.Array.Empty<Claim>()
                    .Concat(patientId == null ? new Claim[0] : new[] { new Claim("patient_id", patientId) }),
                "test");
            controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(identity);
        }

        private static Clinic NewClinic(int id) => new Clinic
        {
            Id = id,
            Name = $"Clinic {id}",
            Location = "Location",
            ContactInfo = "contact",
            CreatedAt = System.DateTime.UtcNow,
            UpdatedAt = System.DateTime.UtcNow
        };

        [Fact]
        public async Task GetPatientClinics_HasPatientIdClaim_ReturnsEligibleClinics()
        {
            var repo = new Mock<IClinicRepository>();
            repo.Setup(r => r.GetPatientClinicsAsync(42)).ReturnsAsync(new List<Clinic> { NewClinic(1) });
            var mapper = CreateMapper();
            var controller = CreateController(repo, mapper, ConfigurationWithClinicFlagEnabled(false));
            SetPatientPrincipal(controller, "42");

            var result = await controller.GetPatientClinics();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var data = Assert.IsAssignableFrom<IEnumerable<ClinicReadDTO>>(ok.Value);
            var clinics = data.ToList();
            Assert.Single(clinics);
            Assert.Equal(1, clinics[0].Id);
        }

        [Fact]
        public async Task GetPatientClinics_UsesPatientIdFromToken()
        {
            var repo = new Mock<IClinicRepository>();
            repo.Setup(r => r.GetPatientClinicsAsync(99)).ReturnsAsync(new List<Clinic>());
            var controller = CreateController(repo, CreateMapper(), ConfigurationWithClinicFlagEnabled(false));
            SetPatientPrincipal(controller, "99");

            await controller.GetPatientClinics();

            repo.Verify(r => r.GetPatientClinicsAsync(99), Times.Once);
        }

        [Fact]
        public async Task GetPatientClinics_MissingPatientIdClaim_ReturnsUnauthorized()
        {
            var repo = new Mock<IClinicRepository>();
            var controller = CreateController(repo, CreateMapper(), ConfigurationWithClinicFlagEnabled(false));

            var result = await controller.GetPatientClinics();

            Assert.IsType<UnauthorizedResult>(result.Result);
            repo.Verify(r => r.GetPatientClinicsAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetPatientClinics_InvalidPatientIdClaim_ReturnsUnauthorized()
        {
            var repo = new Mock<IClinicRepository>();
            var controller = CreateController(repo, CreateMapper(), ConfigurationWithClinicFlagEnabled(false));
            SetPatientPrincipal(controller, "not-a-number");

            var result = await controller.GetPatientClinics();

            Assert.IsType<UnauthorizedResult>(result.Result);
            repo.Verify(r => r.GetPatientClinicsAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetPatientClinics_NoMemberships_ReturnsEmptyList()
        {
            var repo = new Mock<IClinicRepository>();
            repo.Setup(r => r.GetPatientClinicsAsync(7)).ReturnsAsync(new List<Clinic>());
            var controller = CreateController(repo, CreateMapper(), ConfigurationWithClinicFlagEnabled(false));
            SetPatientPrincipal(controller, "7");

            var result = await controller.GetPatientClinics();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Empty(Assert.IsAssignableFrom<IEnumerable<ClinicReadDTO>>(ok.Value));
        }

        [Fact]
        public async Task GetAllClinics_FlagDisabled_ReturnsNotFound_AndNeverQueriesRepository()
        {
            var repo = new Mock<IClinicRepository>();
            var controller = CreateController(repo, CreateMapper(), ConfigurationWithClinicFlagEnabled(false));

            var result = await controller.GetAllClinics();

            Assert.IsType<NotFoundResult>(result.Result);
            repo.Verify(r => r.GetAllClinicsAsync(), Times.Never);
        }

        [Fact]
        public async Task GetAllClinics_FlagEnabled_PreservesLegacyBehavior()
        {
            var repo = new Mock<IClinicRepository>();
            repo.Setup(r => r.GetAllClinicsAsync())
                .ReturnsAsync(new List<Clinic> { NewClinic(1), NewClinic(2) });
            var controller = CreateController(repo, CreateMapper(), ConfigurationWithClinicFlagEnabled(true));

            var result = await controller.GetAllClinics();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(2, Assert.IsAssignableFrom<IEnumerable<ClinicReadDTO>>(ok.Value).Count());
            repo.Verify(r => r.GetAllClinicsAsync(), Times.Once);
        }
    }
}