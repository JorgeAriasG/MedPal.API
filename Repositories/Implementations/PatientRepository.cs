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
                .Where(p => p.Clinic.Id == clinicId)
                .Select(p => new Patient
                {
                    Id = p.Id,
                    Name = p.Name,
                    Middlename = p.Middlename,
                    Lastname = p.Lastname,
                    Email = p.Email,
                    Phone = p.Phone,
                    Address = p.Address,
                    Dob = p.Dob,
                    Gender = p.Gender,
                    EmergencyContact = p.EmergencyContact,
                    Clinic = new Clinic
                    {
                        Id = p.Clinic.Id,
                        Name = p.Clinic.Name,
                        Location = p.Clinic.Location,
                        ContactInfo = p.Clinic.ContactInfo
                    }
                }
                )
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