using System.Collections.Generic;
using System.Threading.Tasks;
using MedPal.API.Models;

namespace MedPal.API.Repositories
{
    public interface IClinicalAttachmentRepository
    {
        Task<IEnumerable<ClinicalAttachment>> GetByMedicalHistoryIdAsync(int medicalHistoryId);
        Task<ClinicalAttachment> GetByIdAsync(int id);
        Task<ClinicalAttachment> AddAsync(ClinicalAttachment attachment);
        void Remove(ClinicalAttachment attachment);
        Task<int> CompleteAsync();
    }
}
