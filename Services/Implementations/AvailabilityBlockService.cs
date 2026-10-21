using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MedPal.API.DTOs;
using MedPal.API.Models;
using MedPal.API.Repositories;

namespace MedPal.API.Services.Implementations
{
    public class AvailabilityBlockService : IAvailabilityBlockService
    {
        private readonly IAvailabilityBlockRepository _availabilityBlockRepository;
        private readonly IMapper _mapper;
        private readonly IValidator<AvailabilityBlockWriteDTO> _validator;

        public AvailabilityBlockService(
            IAvailabilityBlockRepository availabilityBlockRepository,
            IMapper mapper,
            IValidator<AvailabilityBlockWriteDTO> validator)
        {
            _availabilityBlockRepository = availabilityBlockRepository;
            _mapper = mapper;
            _validator = validator;
        }

        public async Task<IEnumerable<AvailabilityBlockReadDTO>> GetBlocksByClinicAsync(int clinicId, DateOnly from, DateOnly to)
        {
            var blocks = await _availabilityBlockRepository.GetBlocksByClinicAsync(clinicId, from, to);
            return _mapper.Map<IEnumerable<AvailabilityBlockReadDTO>>(blocks);
        }

        public async Task<AvailabilityBlockReadDTO> GetBlockByIdAsync(int id)
        {
            var block = await _availabilityBlockRepository.GetBlockByIdAsync(id);
            if (block == null) return null;

            return _mapper.Map<AvailabilityBlockReadDTO>(block);
        }

        public async Task<AvailabilityBlockReadDTO> CreateBlockAsync(AvailabilityBlockWriteDTO request)
        {
            await _validator.ValidateAndThrowAsync(request);

            var block = _mapper.Map<AvailabilityBlock>(request);
            block.CreatedAt = DateTime.UtcNow;
            block.UpdatedAt = DateTime.UtcNow;

            await _availabilityBlockRepository.AddAsync(block);
            await _availabilityBlockRepository.CompleteAsync();

            return _mapper.Map<AvailabilityBlockReadDTO>(block);
        }

        public async Task<bool> DeleteBlockAsync(int id)
        {
            var block = await _availabilityBlockRepository.GetBlockByIdAsync(id);
            if (block == null) return false;

            block.IsDeleted = true;
            block.DeletedAt = DateTime.UtcNow;
            block.UpdatedAt = DateTime.UtcNow;
            _availabilityBlockRepository.Update(block);
            await _availabilityBlockRepository.CompleteAsync();

            return true;
        }
    }
}