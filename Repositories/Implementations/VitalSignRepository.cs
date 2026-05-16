using MedPal.API.Data;
using MedPal.API.Models;
using MedPal.API.Services;
using Microsoft.EntityFrameworkCore;

namespace MedPal.API.Repositories.Implementations
{
    public class VitalSignRepository : TenantAwareRepository<VitalSign>, IVitalSignRepository
    {
        public VitalSignRepository(AppDbContext context, ITenantContextService tenantContext)
            : base(context, tenantContext)
        {
        }

        public async Task<IEnumerable<VitalSign>> GetAllVitalSignsAsync()
        {
            return await ApplyTenantFilter(_context.VitalSigns
                .Include(vs => vs.PatientDetails)
                .Where(vs => !vs.IsDeleted))
                .OrderByDescending(vs => vs.RecordedAt)
                .ToListAsync();
        }

        public async Task<VitalSign?> GetVitalSignByIdAsync(int id)
        {
            return await ApplyTenantFilter(_context.VitalSigns
                .Include(vs => vs.PatientDetails))
                .FirstOrDefaultAsync(vs => vs.Id == id && !vs.IsDeleted);
        }

        public async Task<IEnumerable<VitalSign>> GetVitalSignsByPatientDetailsIdAsync(int patientDetailsId)
        {
            return await ApplyTenantFilter(_context.VitalSigns
                .Include(vs => vs.PatientDetails)
                .Where(vs => vs.PatientDetailsId == patientDetailsId && !vs.IsDeleted))
                .OrderByDescending(vs => vs.RecordedAt)
                .ToListAsync();
        }

        public async Task<VitalSign> AddVitalSignAsync(VitalSign vitalSign)
        {
            await _context.VitalSigns.AddAsync(vitalSign);
            return vitalSign;
        }

        public void UpdateVitalSign(VitalSign vitalSign)
        {
            _context.VitalSigns.Update(vitalSign);
        }

        public void RemoveVitalSign(VitalSign vitalSign)
        {
            _context.VitalSigns.Remove(vitalSign);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
