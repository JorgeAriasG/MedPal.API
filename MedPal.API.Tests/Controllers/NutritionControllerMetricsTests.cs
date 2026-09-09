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
using Moq;
using FluentAssertions;

namespace MedPal.API.Tests.Controllers
{
    public class NutritionControllerMetricsTests
    {
        private static NutritionController CreateController(
            IAnthropometryRepository anthropometryRepo,
            IMapper mapper)
        {
            var controller = new NutritionController(
                Mock.Of<IFoodItemRepository>(),
                Mock.Of<IBodyCompositionRepository>(),
                anthropometryRepo,
                Mock.Of<IDietPlanRepository>(),
                Mock.Of<INutritionProgressRepository>(),
                Mock.Of<ISupplementRepository>(),
                Mock.Of<INutritionService>(),
                mapper);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity("test"))
                }
            };

            return controller;
        }

        private static AnthropometryWriteDTO CreateWriteDto() => new AnthropometryWriteDTO
        {
            PatientDetailsId = 7,
            Weight = 70,
            Height = 175,
            Waist = 80,
            Hip = 104
        };

        [Fact]
        public async Task CreateAnthropometryRecord_ComputesMetricsUsingCentimeters()
        {
            var anthropometryRepo = new Mock<IAnthropometryRepository>();

            var mapper = new Mock<IMapper>();
            mapper
                .Setup(m => m.Map<AnthropometryRecord>(It.IsAny<AnthropometryWriteDTO>()))
                .Returns((AnthropometryWriteDTO dto) => new AnthropometryRecord
                {
                    PatientDetailsId = dto.PatientDetailsId,
                    RecordedAt = dto.RecordedAt,
                    Weight = dto.Weight,
                    Height = dto.Height,
                    Waist = dto.Waist,
                    Hip = dto.Hip
                });
            mapper
                .Setup(m => m.Map<AnthropometryReadDTO>(It.IsAny<AnthropometryRecord>()))
                .Returns((AnthropometryRecord entity) => new AnthropometryReadDTO
                {
                    Id = entity.Id,
                    Bmi = entity.Bmi,
                    WaistHeightRatio = entity.WaistHeightRatio,
                    WaistHipRatio = entity.WaistHipRatio
                });

            anthropometryRepo
                .Setup(r => r.AddAsync(It.IsAny<AnthropometryRecord>()))
                .ReturnsAsync((AnthropometryRecord record) =>
                {
                    record.Id = 1;
                    return record;
                });
            anthropometryRepo.Setup(r => r.CompleteAsync()).ReturnsAsync(1);

            var controller = CreateController(anthropometryRepo.Object, mapper.Object);

            var action = await controller.CreateAnthropometryRecord(CreateWriteDto());

            var created = action.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            var dto = created.Value.Should().BeOfType<AnthropometryReadDTO>().Subject;

            dto.Bmi.Should().Be(22.9m);            // 70 / (1.75)^2
            dto.WaistHeightRatio.Should().Be(0.46m); // 80 / 175
            dto.WaistHipRatio.Should().Be(0.769m);   // 80 / 104
        }

        [Fact]
        public async Task UpdateAnthropometryRecord_ComputesMetricsUsingCentimeters()
        {
            var anthropometryRepo = new Mock<IAnthropometryRepository>();
            var entity = new AnthropometryRecord
            {
                Id = 5,
                PatientDetailsId = 7,
                Weight = 70,
                Height = 175,
                Waist = 80,
                Hip = 104
            };
            anthropometryRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(entity);
            anthropometryRepo.Setup(r => r.CompleteAsync()).ReturnsAsync(1);

            var mapper = new Mock<IMapper>();
            var controller = CreateController(anthropometryRepo.Object, mapper.Object);

            var action = await controller.UpdateAnthropometryRecord(5, CreateWriteDto());

            action.Should().BeOfType<NoContentResult>();
            entity.Bmi.Should().Be(22.9m);
            entity.WaistHeightRatio.Should().Be(0.46m);
            entity.WaistHipRatio.Should().Be(0.769m);
        }

        [Fact]
        public async Task CreateAndUpdate_ProduceConsistentMetrics_ForSameInputs()
        {
            // Guard: create y update deben llegar al mismo resultado clínico.
            var createRepo = new Mock<IAnthropometryRepository>();
            var updateRepo = new Mock<IAnthropometryRepository>();

            var createMapper = new Mock<IMapper>();
            createMapper
                .Setup(m => m.Map<AnthropometryRecord>(It.IsAny<AnthropometryWriteDTO>()))
                .Returns((AnthropometryWriteDTO dto) => new AnthropometryRecord
                {
                    Weight = dto.Weight,
                    Height = dto.Height,
                    Waist = dto.Waist,
                    Hip = dto.Hip
                });

            AnthropometryRecord captured = null!;
            createRepo
                .Setup(r => r.AddAsync(It.IsAny<AnthropometryRecord>()))
                .ReturnsAsync((AnthropometryRecord record) =>
                {
                    captured = record;
                    return record;
                });
            createRepo.Setup(r => r.CompleteAsync()).ReturnsAsync(1);

            var updateEntity = new AnthropometryRecord
            {
                Id = 5,
                Weight = 70,
                Height = 175,
                Waist = 80,
                Hip = 104
            };
            updateRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(updateEntity);
            updateRepo.Setup(r => r.CompleteAsync()).ReturnsAsync(1);

            var createController = CreateController(createRepo.Object, createMapper.Object);
            var updateController = CreateController(updateRepo.Object, new Mock<IMapper>().Object);

            await createController.CreateAnthropometryRecord(CreateWriteDto());
            await updateController.UpdateAnthropometryRecord(5, CreateWriteDto());

            captured!.Bmi.Should().Be(updateEntity.Bmi);
            captured!.WaistHeightRatio.Should().Be(updateEntity.WaistHeightRatio);
            captured!.WaistHipRatio.Should().Be(updateEntity.WaistHipRatio);

            captured!.Bmi.Should().Be(22.9m);
            captured!.WaistHeightRatio.Should().Be(0.46m);
            captured!.WaistHipRatio.Should().Be(0.769m);
        }
    }
}