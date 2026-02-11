using System;
using System.Threading.Tasks;
using MedPal.API.Data;
using MedPal.API.Models;
using MedPal.API.Services;
using Microsoft.EntityFrameworkCore;

namespace MedPal.API.Repositories.Implementations
{
    public class PrescriptionRepository : TenantAwareRepository<Prescription>, IPrescriptionRepository
    {
        public PrescriptionRepository(AppDbContext context, ITenantContextService tenantContext)
            : base(context, tenantContext)
        {
        }

        public async Task<Prescription> GetByUniqueCodeAsync(Guid uniqueCode)
        {
            return await ApplyTenantFilter(_context.Prescriptions
                .Include(p => p.Doctor)
                .Include(p => p.Patient)
                .Include(p => p.Items))
                .FirstOrDefaultAsync(p => p.UniqueCode == uniqueCode);
        }

        public async Task<IEnumerable<Prescription>> GetByPatientIdAsync(int patientId)
        {
            return await ApplyTenantFilter(_context.Prescriptions
                .Include(p => p.Doctor)
                .Include(p => p.Patient)
                .Include(p => p.Items)
                .Where(p => p.PatientId == patientId))
                .OrderByDescending(p => p.IssuedAt)
                .ToListAsync();
        }
    }
}
