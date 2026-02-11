using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MedPal.API.Models;
using MedPal.API.Data;
using MedPal.API.Services;
using MedPal.API.Repositories.Implementations;

namespace MedPal.API.Repositories
{
    public class MedicalHistoryRepository : TenantAwareRepository<MedicalHistory>, IMedicalHistoryRepository
    {
        public MedicalHistoryRepository(AppDbContext context, ITenantContextService tenantContext)
            : base(context, tenantContext)
        {
        }

        public async Task<IEnumerable<MedicalHistory>> GetAllMedicalHistoriesAsync()
        {
            return await ApplyTenantFilter(_context.MedicalHistories).ToListAsync();
        }

        public async Task<MedicalHistory> GetMedicalHistoryByIdAsync(int id)
        {
            return await ApplyTenantFilter(_context.MedicalHistories)
                .FirstOrDefaultAsync(mh => mh.Id == id);
        }

        public async Task<MedicalHistory> AddMedicalHistoryAsync(MedicalHistory medicalHistory)
        {
            await _context.MedicalHistories.AddAsync(medicalHistory);
            return medicalHistory;
        }

        public void UpdateMedicalHistory(MedicalHistory medicalHistory)
        {
            _context.MedicalHistories.Update(medicalHistory);
        }

        public void RemoveMedicalHistory(MedicalHistory medicalHistory)
        {
            _context.MedicalHistories.Remove(medicalHistory);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}