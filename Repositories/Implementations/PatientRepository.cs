using AutoMapper;
using MedPal.API.Data;
using MedPal.API.DTOs;
using MedPal.API.Models;
using Microsoft.EntityFrameworkCore;

namespace MedPal.API.Repositories.Implementations
{
    public class PatientRepository : IPatientRepository
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public PatientRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Patient>> GetAllPatientsAsync(int clinicId)
        {
            return await _context.Patients
                .Where(p => p.ClinicId == clinicId) // Comparar por ID, no por objeto
                .ToListAsync();
        }

        public async Task<Patient> GetPatientByIdAsync(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
            {
                // Handle the case where the patient is not found
                throw new KeyNotFoundException($"Patient with Id {id} not found.");
            }
            return patient;
        }

        public async Task<Patient> AddPatientAsync(Patient patient)
        {
            // Inicializar detalles del paciente para evitar 404 en el frontend
            patient.PatientDetails = new PatientDetails
            {
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Patients.AddAsync(patient);
            await _context.SaveChangesAsync();
            return patient;
        }

        public async Task UpdatePatientAsync(int id, Patient patient)
        {
            patient.Id = id;
            var existingPatient = await _context.Patients.FindAsync(id);
            if (existingPatient != null)
            {
                _mapper.Map(patient, existingPatient);
                _context.Patients.Update(existingPatient);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeletePatientAsync(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient != null)
            {
                _context.Patients.Remove(patient);
                await _context.SaveChangesAsync();
            }
        }
    }
}