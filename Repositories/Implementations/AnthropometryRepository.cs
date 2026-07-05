using MedPal.API.Data;
using MedPal.API.Models;
using MedPal.API.Services;
using Microsoft.EntityFrameworkCore;

namespace MedPal.API.Repositories.Implementations
{
    public class AnthropometryRepository : TenantAwareRepository<AnthropometryRecord>, IAnthropometryRepository
    {
        public AnthropometryRepository(AppDbContext context, ITenantContextService tenantContext)
            : base(context, tenantContext)
        {
        }

        public async Task<IEnumerable<AnthropometryRecord>> GetByPatientDetailsIdAsync(int patientDetailsId)
        {
            return await ApplyTenantFilter(_context.AnthropometryRecords
                .Include(ar => ar.PatientDetails)
                .Where(ar => ar.PatientDetailsId == patientDetailsId && !ar.IsDeleted))
                .OrderByDescending(ar => ar.RecordedAt)
                .ToListAsync();
        }

        public async Task<AnthropometryRecord?> GetByIdAsync(int id)
        {
            return await ApplyTenantFilter(_context.AnthropometryRecords
                .Include(ar => ar.PatientDetails))
                .FirstOrDefaultAsync(ar => ar.Id == id && !ar.IsDeleted);
        }

        public async Task<AnthropometryRecord> AddAsync(AnthropometryRecord record)
        {
            await _context.AnthropometryRecords.AddAsync(record);
            return record;
        }

        public void Update(AnthropometryRecord record)
        {
            _context.AnthropometryRecords.Update(record);
        }

        public void Remove(AnthropometryRecord record)
        {
            _context.AnthropometryRecords.Remove(record);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
