using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MedPal.API.Data;
using MedPal.API.Models;
using Microsoft.EntityFrameworkCore;

namespace MedPal.API.Services
{
    public class ArcoService : IArcoService
    {
        private readonly AppDbContext _context;

        public ArcoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ArcoRequest> CreateRequestAsync(ArcoRequest request)
        {
            _context.ArcoRequests.Add(request);
            await _context.SaveChangesAsync();
            return request;
        }

        public async Task<object> ExportUserDataAsync(int patientId)
        {
            var patient = await _context.Patients
                .Include(p => p.Appointments)
                .Include(p => p.PatientDetails)
                    .ThenInclude(pd => pd.MedicalHistories)
                .Include(p => p.Invoices)
                .FirstOrDefaultAsync(p => p.Id == patientId);

            if (patient == null) return null;

            // Projection ensuring no cycles and flat structure for portability
            return new
            {
                Profile = new
                {
                    patient.Name,
                    patient.Lastname,
                    patient.Email,
                    patient.Phone,
                    patient.Address,
                    patient.Dob,
                    patient.CreatedAt
                },
                MedicalHistory = patient.PatientDetails?.MedicalHistories.Select(m => new
                {
                    m.Diagnosis,
                    Date = m.DiagnosisDate
                }),
                Appointments = patient.Appointments.Select(a => new
                {
                    a.Date,
                    a.Time,
                    Reason = a.Notes,
                    a.Status
                }),
                Invoices = patient.Invoices.Select(i => new
                {
                    i.TotalAmount,
                    i.Status,
                    IssueDate = i.CreatedAt
                })
            };
        }

        public async Task AnonymizePatientAsync(int patientId)
        {
            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null) return;

            // Borrado Lógico Anonimizado
            patient.Name = $"ANONYMIZED-{patient.Id}";
            patient.Middlename = "REMOVED";
            patient.Lastname = "REMOVED";
            patient.Email = $"deleted-{patient.Id}@medpal.com"; // Unique dummy email
            patient.Phone = "0000000000";
            patient.Address = "REMOVED";
            // EmergencyContacts se manejan en cascada o se pueden marcar como IsDeleted individualmente

            patient.IsDeleted = true;
            patient.IsAnonymized = true;
            patient.IsMarketingBlocked = true; // Implicit opposition

            // También anonimizar el User asociado si existe
            if (patient.UserId.HasValue)
            {
                var user = await _context.Users.FindAsync(patient.UserId.Value);
                if (user != null)
                {
                    user.Name = $"ANONYMIZED-{user.Id}";
                    user.Email = $"deleted-{user.Id}@medpal.com";
                    user.IsDeleted = true;
                    user.IsActive = false;
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task ToggleMarketingBlockAsync(int patientId, bool blocked)
        {
            var patient = await _context.Patients.FindAsync(patientId);
            if (patient != null)
            {
                patient.IsMarketingBlocked = blocked;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ArcoRequest>> GetPendingRequestsAsync()
        {
            return await _context.ArcoRequests
                .Include(r => r.Patient)
                .Include(r => r.User)
                .Where(r => r.Status == ArcoRequestStatus.Pending)
                .ToListAsync();
        }
    }
}
