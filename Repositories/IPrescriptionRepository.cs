using System;
using System.Threading.Tasks;
using MedPal.API.Models;

namespace MedPal.API.Repositories
{
    public interface IPrescriptionRepository : IRepository<Prescription>
    {
        Task<Prescription> GetByUniqueCodeAsync(Guid uniqueCode);
        Task<IEnumerable<Prescription>> GetByPatientIdAsync(int patientId);
    }
}
