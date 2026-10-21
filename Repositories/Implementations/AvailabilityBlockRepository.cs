using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MedPal.API.Data;
using MedPal.API.Models;
using MedPal.API.Services;
using MedPal.API.Utils;
using Microsoft.EntityFrameworkCore;

namespace MedPal.API.Repositories.Implementations
{
    public class AvailabilityBlockRepository : TenantAwareRepository<AvailabilityBlock>, IAvailabilityBlockRepository
    {
        public AvailabilityBlockRepository(AppDbContext context, ITenantContextService tenantContext)
            : base(context, tenantContext)
        {
        }

        public async Task<IEnumerable<AvailabilityBlock>> GetBlocksByClinicAsync(int clinicId, DateOnly from, DateOnly to)
        {
            return await ApplyTenantFilter(_context.AvailabilityBlocks)
                .Where(b => b.ClinicId == clinicId && b.Date >= from && b.Date <= to && !b.IsDeleted)
                .OrderBy(b => b.Date)
                .ThenBy(b => b.StartTime)
                .ToListAsync();
        }

        public async Task<AvailabilityBlock> GetBlockByIdAsync(int id)
        {
            return await ApplyTenantFilter(_context.AvailabilityBlocks)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<IEnumerable<AvailabilityBlock>> GetBlocksForAvailabilityAsync(int clinicId, int? doctorId, DateOnly date)
        {
            return await _context.AvailabilityBlocks
                .AsNoTracking()
                .Where(b => b.ClinicId == clinicId && b.Date == date && !b.IsDeleted)
                .Where(b => b.DoctorId == null || (doctorId.HasValue && b.DoctorId == doctorId.Value))
                .ToListAsync();
        }

        public async Task<bool> HasOverlapWithAppointmentsAsync(int clinicId, int? doctorId, DateOnly date, TimeOnly startTime, TimeOnly endTime)
        {
            // El query filter global de Appointment ya aplica tenant + soft delete.
            var appointments = await _context.Appointments
                .Where(a => a.ClinicId == clinicId && a.Date == date && !a.IsDeleted)
                .Where(a => !doctorId.HasValue || a.UserId == doctorId.Value)
                .ToListAsync();

            foreach (var appointment in appointments)
            {
                if (SlotAvailability.IntervalsOverlap(startTime, endTime, appointment.Time, appointment.Time.AddMinutes(appointment.DurationMinutes)))
                    return true;
            }

            return false;
        }

        public async Task<bool> HasOverlapWithBlocksAsync(int clinicId, int? doctorId, DateOnly date, TimeOnly startTime, TimeOnly endTime, int? excludeBlockId = null)
        {
            var blocks = await ApplyTenantFilter(_context.AvailabilityBlocks)
                .Where(b => b.ClinicId == clinicId && b.Date == date && !b.IsDeleted)
                .Where(b => !doctorId.HasValue || b.DoctorId == null || b.DoctorId == doctorId.Value)
                .Where(b => excludeBlockId == null || b.Id != excludeBlockId.Value)
                .ToListAsync();

            foreach (var block in blocks)
            {
                if (SlotAvailability.IntervalsOverlap(startTime, endTime, block.StartTime, block.EndTime))
                    return true;
            }

            return false;
        }

        public async Task<AvailabilityBlock> AddAsync(AvailabilityBlock block)
        {
            await _context.AvailabilityBlocks.AddAsync(block);
            return block;
        }

        public void Update(AvailabilityBlock block)
        {
            _context.AvailabilityBlocks.Update(block);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}