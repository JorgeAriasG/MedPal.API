using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MedPal.API.Models;
using MedPal.API.Data;
using MedPal.API.Services;
using MedPal.API.Repositories.Implementations;

namespace MedPal.API.Repositories
{
    public class ClinicalAttachmentRepository : TenantAwareRepository<ClinicalAttachment>, IClinicalAttachmentRepository
    {
        public ClinicalAttachmentRepository(AppDbContext context, ITenantContextService tenantContext)
            : base(context, tenantContext)
        {
        }

        public async Task<IEnumerable<ClinicalAttachment>> GetByMedicalHistoryIdAsync(int medicalHistoryId)
        {
            return await ApplyTenantFilter(_context.ClinicalAttachments)
                .Where(a => a.MedicalHistoryId == medicalHistoryId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public override async Task<ClinicalAttachment> GetByIdAsync(int id)
        {
            return await ApplyTenantFilter(_context.ClinicalAttachments)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<ClinicalAttachment> AddAsync(ClinicalAttachment attachment)
        {
            await _context.ClinicalAttachments.AddAsync(attachment);
            return attachment;
        }

        public void Remove(ClinicalAttachment attachment)
        {
            _context.ClinicalAttachments.Remove(attachment);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
