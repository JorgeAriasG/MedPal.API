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
    public class AvailabilityBlockServiceTests
    {
        private readonly Mock<IAvailabilityBlockRepository> _mockBlockRepo;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IValidator<AvailabilityBlockWriteDTO>> _mockValidator;
        private readonly AvailabilityBlockService _availabilityBlockService;

        public AvailabilityBlockServiceTests()
        {
            _mockBlockRepo = new Mock<IAvailabilityBlockRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockValidator = new Mock<IValidator<AvailabilityBlockWriteDTO>>();

            _availabilityBlockService = new AvailabilityBlockService(
                _mockBlockRepo.Object,
                _mockMapper.Object,
                _mockValidator.Object);
        }

        private static AvailabilityBlockWriteDTO ValidRequest() => new()
        {
            ClinicId = 1,
            DoctorId = 2,
            Date = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0)
        };

        [Fact]
        public async Task GetBlocksByClinicAsync_ShouldReturnMappedBlocks()
        {
            var from = DateOnly.FromDateTime(DateTime.Today);
            var to = DateOnly.FromDateTime(DateTime.Today.AddDays(7));
            var blocks = new List<AvailabilityBlock>
            {
                new AvailabilityBlock { Id = 1, ClinicId = 1 },
                new AvailabilityBlock { Id = 2, ClinicId = 1 }
            };
            var readDtos = new List<AvailabilityBlockReadDTO>
            {
                new AvailabilityBlockReadDTO { Id = 1, ClinicId = 1 },
                new AvailabilityBlockReadDTO { Id = 2, ClinicId = 1 }
            };

            _mockBlockRepo.Setup(r => r.GetBlocksByClinicAsync(1, from, to)).ReturnsAsync(blocks);
            _mockMapper.Setup(m => m.Map<IEnumerable<AvailabilityBlockReadDTO>>(blocks)).Returns(readDtos);

            var result = await _availabilityBlockService.GetBlocksByClinicAsync(1, from, to);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _mockBlockRepo.Verify(r => r.GetBlocksByClinicAsync(1, from, to), Times.Once);
        }

        [Fact]
        public async Task GetBlockByIdAsync_WhenExists_ShouldReturnMappedDTO()
        {
            var block = new AvailabilityBlock { Id = 1, ClinicId = 1 };
            var readDto = new AvailabilityBlockReadDTO { Id = 1, ClinicId = 1 };

            _mockBlockRepo.Setup(r => r.GetBlockByIdAsync(1)).ReturnsAsync(block);
            _mockMapper.Setup(m => m.Map<AvailabilityBlockReadDTO>(block)).Returns(readDto);

            var result = await _availabilityBlockService.GetBlockByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task GetBlockByIdAsync_WhenNotExists_ShouldReturnNull()
        {
            _mockBlockRepo.Setup(r => r.GetBlockByIdAsync(99)).ReturnsAsync((AvailabilityBlock)null);

            var result = await _availabilityBlockService.GetBlockByIdAsync(99);

            Assert.Null(result);
        }

        [Fact]
        public async Task CreateBlockAsync_ValidRequest_ShouldCreateAndReturnDTO()
        {
            var writeDto = ValidRequest();
            var block = new AvailabilityBlock { ClinicId = 1, DoctorId = 2 };
            var readDto = new AvailabilityBlockReadDTO { Id = 1, ClinicId = 1 };

            _mockValidator.Setup(v => v.ValidateAsync(writeDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _mockMapper.Setup(m => m.Map<AvailabilityBlock>(writeDto)).Returns(block);
            _mockBlockRepo.Setup(r => r.AddAsync(block)).ReturnsAsync(block);
            _mockMapper.Setup(m => m.Map<AvailabilityBlockReadDTO>(block)).Returns(readDto);

            var result = await _availabilityBlockService.CreateBlockAsync(writeDto);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            _mockBlockRepo.Verify(r => r.AddAsync(block), Times.Once);
            _mockBlockRepo.Verify(r => r.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateBlockAsync_InvalidRequest_ShouldThrowValidationException()
        {
            var writeDto = ValidRequest();
            var validationFailure = new ValidationFailure("EndTime", "La hora de fin debe ser posterior a la hora de inicio.");

            _mockValidator
                .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<AvailabilityBlockWriteDTO>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ValidationException(new[] { validationFailure }));

            await Assert.ThrowsAsync<ValidationException>(() => _availabilityBlockService.CreateBlockAsync(writeDto));
            _mockBlockRepo.Verify(r => r.AddAsync(It.IsAny<AvailabilityBlock>()), Times.Never);
        }

        [Fact]
        public async Task DeleteBlockAsync_ExistingId_ShouldReturnTrue_AndSoftDelete()
        {
            var block = new AvailabilityBlock { Id = 1, ClinicId = 1 };
            _mockBlockRepo.Setup(r => r.GetBlockByIdAsync(1)).ReturnsAsync(block);

            var result = await _availabilityBlockService.DeleteBlockAsync(1);

            Assert.True(result);
            Assert.True(block.IsDeleted);
            Assert.NotNull(block.DeletedAt);
            _mockBlockRepo.Verify(r => r.Update(block), Times.Once);
            _mockBlockRepo.Verify(r => r.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteBlockAsync_NonExistingId_ShouldReturnFalse()
        {
            _mockBlockRepo.Setup(r => r.GetBlockByIdAsync(99)).ReturnsAsync((AvailabilityBlock)null);

            var result = await _availabilityBlockService.DeleteBlockAsync(99);

            Assert.False(result);
            _mockBlockRepo.Verify(r => r.Update(It.IsAny<AvailabilityBlock>()), Times.Never);
        }
    }
}