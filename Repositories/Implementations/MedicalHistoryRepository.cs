using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MedPal.API.Models;
using MedPal.API.Data;

namespace MedPal.API.Repositories
{
    public class MedicalHistoryRepository : IMedicalHistoryRepository
    {
        private readonly AppDbContext _context;

        public MedicalHistoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MedicalHistory>> GetAllMedicalHistoriesAsync()
        {
            return await _context.MedicalHistories.ToListAsync();
        }

        public async Task<MedicalHistory> GetMedicalHistoryByIdAsync(int id)
        {
            return await _context.MedicalHistories.FindAsync(id);
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