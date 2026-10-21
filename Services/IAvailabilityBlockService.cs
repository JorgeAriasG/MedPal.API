using MedPal.API.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MedPal.API.Services
{
    public interface IAvailabilityBlockService
    {
        Task<IEnumerable<AvailabilityBlockReadDTO>> GetBlocksByClinicAsync(int clinicId, DateOnly from, DateOnly to);
        Task<AvailabilityBlockReadDTO> GetBlockByIdAsync(int id);
        Task<AvailabilityBlockReadDTO> CreateBlockAsync(AvailabilityBlockWriteDTO request);
        Task<bool> DeleteBlockAsync(int id);
    }
}