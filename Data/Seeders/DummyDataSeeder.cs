using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MedPal.API.Models;
using MedPal.API.Models.Authorization;
using MedPal.API.Enums;

namespace MedPal.API.Data.Seeders
{
    public class DummyDataSeeder
    {
        public static async Task SeedDummyDataAsync(AppDbContext context)
        {
            Console.WriteLine("🚀 Iniciando rutinade Seeders y Limpieza...");

            // --- 0. Limpieza: Eliminar SuperAdmins Duplicados ---
            var superAdmins = await context.Users.Where(u => u.Email == "superadmin@medpal.com").OrderBy(u => u.Id).ToListAsync();
            if (superAdmins.Count > 1)
            {
                var duplicatesToDrop = superAdmins.Skip(1).ToList();
                // Primero quitamos sus roles si los tienen para no violar referential integrity
                var duplicateIds = duplicatesToDrop.Select(d => d.Id).ToList();
                var associatedRoles = await context.UserRoles.Where(ur => duplicateIds.Contains(ur.UserId)).ToListAsync();
                context.UserRoles.RemoveRange(associatedRoles);
                
                context.Users.RemoveRange(duplicatesToDrop);
                await context.SaveChangesAsync();
                Console.WriteLine($"🧹 Se eliminaron {duplicatesToDrop.Count} SuperAdmins duplicados.");
            }

            // --- 0.A Deprecar roles obsoletos (soft-delete) ---
            var obsoleteRoleNames = new[] { "HealthProfessional", "Admin" };
            var obsoleteRoles = await context.Roles
                .IgnoreQueryFilters()
                .Where(r => obsoleteRoleNames.Contains(r.Name) && !r.IsDeleted)
                .ToListAsync();
            foreach (var role in obsoleteRoles)
            {
                role.IsDeleted = true;
                role.DeletedAt = DateTime.UtcNow;
            }
            if (obsoleteRoles.Any())
            {
                await context.SaveChangesAsync();
                Console.WriteLine($"🚓 Roles obsoletos marcados como deprecados: {string.Join(", ", obsoleteRoles.Select(r => r.Name))}");
            }

            // 1. Obtener o crear Account "MedPal System"
            var account = await context.Accounts.FirstOrDefaultAsync(a => a.Name == "MedPal System");
            if (account == null)
            {
                account = new Account
                {
                    Name = "MedPal System",
                    Description = "Cuenta base para Seeder",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await context.Accounts.AddAsync(account);
                await context.SaveChangesAsync();
            }

            // 2. Crear Clínicas
            var clinicA = await context.Clinics.FirstOrDefaultAsync(c => c.Name == "Clínica Vida Sana");
            if (clinicA == null)
            {
                clinicA = new Clinic { Name = "Clínica Vida Sana", Location = "Av. Principal 123", ContactInfo = "555-1234", AccountId = account.Id, Open = new TimeOnly(8,0), Close = new TimeOnly(20,0), CreatedAt = DateTime.UtcNow };
                var clinicB = new Clinic { Name = "Centro Médico Los Alpes", Location = "Av. Secundaria 456", ContactInfo = "555-5678", AccountId = account.Id, Open = new TimeOnly(9,0), Close = new TimeOnly(18,0), CreatedAt = DateTime.UtcNow };
                await context.Clinics.AddRangeAsync(clinicA, clinicB);
                await context.SaveChangesAsync();
            }

            // 3. Crear Doctores
            var doctorRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Doctor");
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "ClinicAdmin");

            var doctor = await context.Users.FirstOrDefaultAsync(u => u.Email == "doctor1@medpal.com");
            if (doctor == null)
            {
                doctor = new User
                {
                    Name = "Dr. John Smith",
                    Email = "doctor1@medpal.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Doctor@123"),
                    Specialty = "Cardiología",
                    ProfessionalLicenseNumber = "MED-88990",
                    IsActive = true,
                    HasAcceptedPrivacyTerms = true,
                    AccountId = account.Id,
                    PrincipalClinicId = clinicA.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await context.Users.AddAsync(doctor);
                await context.SaveChangesAsync();

                if (doctorRole != null)
                {
                    await context.UserRoles.AddAsync(new UserRole { UserId = doctor.Id, RoleId = doctorRole.Id, ClinicId = clinicA.Id, AssignedAt = DateTime.UtcNow });
                }
                
                // Darle privilegios de administrador de su clínica para hacer tests
                if (adminRole != null)
                {
                    await context.UserRoles.AddAsync(new UserRole { UserId = doctor.Id, RoleId = adminRole.Id, ClinicId = clinicA.Id, AssignedAt = DateTime.UtcNow });
                }
                
                await context.SaveChangesAsync();
            }
            else if (doctor.PrincipalClinicId == null || doctor.PrincipalClinicId != clinicA.Id)
            {
                // Parche de actualización en caso de que el doctor ya existiera pero sin clínica principal
                doctor.PrincipalClinicId = clinicA.Id;
                context.Users.Update(doctor);
                await context.SaveChangesAsync();
            }

            // --- 3.B Usuarios QA Adicionales ---
            var accountAdminRole  = await context.Roles.FirstOrDefaultAsync(r => r.Name == "AccountAdmin");
            var clinicAdminRole   = await context.Roles.FirstOrDefaultAsync(r => r.Name == "ClinicAdmin");
            var nurseRole         = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Nurse");
            var receptionistRole  = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Receptionist");

            var qaUsers = new[]
            {
                new { Email = "accountadmin@medpal.com",   Name = "Admin QA",         Password = "Admin@123",       Specialty = "Administración",    License = "",          Role = accountAdminRole,  IsAccountLevel = true  },
                new { Email = "clinicadmin2@medpal.com",   Name = "Clinic Admin QA",   Password = "Clinic@123",      Specialty = "Administración",    License = "",          Role = clinicAdminRole,   IsAccountLevel = false },
                new { Email = "nurse1@medpal.com",         Name = "Enfermera QA",      Password = "Nurse@123",       Specialty = "Enfermer\u00eda",       License = "ENF-00123", Role = nurseRole,         IsAccountLevel = false },
                new { Email = "receptionist1@medpal.com",  Name = "Recepcionista QA",  Password = "Recept@123",      Specialty = "Administraci\u00f3n",    License = "",          Role = receptionistRole,  IsAccountLevel = false },
            };

            foreach (var qa in qaUsers)
            {
                var existing = await context.Users.FirstOrDefaultAsync(u => u.Email == qa.Email);
                if (existing == null && qa.Role != null)
                {
                    var newUser = new User
                    {
                        Name = qa.Name,
                        Email = qa.Email,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(qa.Password),
                        Specialty = qa.Specialty,
                        ProfessionalLicenseNumber = qa.License,
                        IsActive = true,
                        HasAcceptedPrivacyTerms = true,
                        AccountId = account.Id,
                        PrincipalClinicId = clinicA.Id,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await context.Users.AddAsync(newUser);
                    await context.SaveChangesAsync();

                    var clinicIdForRole = qa.IsAccountLevel ? (int?)null : clinicA.Id;
                    await context.UserRoles.AddAsync(new UserRole
                    {
                        UserId = newUser.Id,
                        RoleId = qa.Role.Id,
                        ClinicId = clinicIdForRole,
                        AssignedAt = DateTime.UtcNow
                    });
                    await context.SaveChangesAsync();
                    Console.WriteLine($"✅ Usuario QA creado: {qa.Email} [{qa.Role.Name}]");
                }
            }

            // 0.B Limpieza de viejos pacientes y sus citas (Para evitar el crash por FK/UpdatedAt)
            var oldPatients = await context.Patients
                .Where(p => p.Email.StartsWith("paciente") && p.Email.EndsWith("@medpal.com"))
                .ToListAsync();

            if (oldPatients.Any())
            {
                var oldPatientIds = oldPatients.Select(p => p.Id).ToList();

                // Primero eliminar prescriptions items de prescipciones de esos pacientes
                var oldPrescriptions = await context.Prescriptions
                    .IgnoreQueryFilters()
                    .Where(p => oldPatientIds.Contains(p.PatientId))
                    .ToListAsync();
                var oldPrescriptionIds = oldPrescriptions.Select(p => p.Id).ToList();
                var oldPrescriptionItems = await context.PrescriptionItems
                    .IgnoreQueryFilters()
                    .Where(pi => oldPrescriptionIds.Contains(pi.PrescriptionId))
                    .ToListAsync();
                context.PrescriptionItems.RemoveRange(oldPrescriptionItems);
                context.Prescriptions.RemoveRange(oldPrescriptions);

                // Luego eliminar appointments
                var oldAppointments = await context.Appointments
                    .IgnoreQueryFilters()
                    .Where(a => oldPatientIds.Contains(a.PatientId))
                    .ToListAsync();
                context.Appointments.RemoveRange(oldAppointments);

                // Luego eliminar medical histories
                var oldDetails = await context.PatientDetails
                    .IgnoreQueryFilters()
                    .Where(pd => oldPatientIds.Contains(pd.PatientId))
                    .ToListAsync();
                context.PatientDetails.RemoveRange(oldDetails);

                context.Patients.RemoveRange(oldPatients);
                await context.SaveChangesAsync();
                Console.WriteLine($"🧹 Se eliminaron {oldPatients.Count} pacientes y sus datos relacionados.");
            }

            // 4. Crear 50 Pacientes (A gran escala)
            var patients = new List<Patient>();
            var random = new Random();

            for (int i = 1; i <= 50; i++)
            {
                var patient = new Patient
                {
                    Name = $"Paciente QA {i}",
                    Email = $"paciente{i}@medpal.com",
                    Dob = new DateTime(random.Next(1950, 2010), random.Next(1, 12), random.Next(1, 28)),
                    Phone = $"555-000{i:D2}",
                    Address = $"Avenida Pruebas {100 + i}",
                    Gender = i % 2 == 0 ? "M" : "F",
                    Middlename = "Test",
                    Lastname = "QA",
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    AccountId = account.Id,
                    ClinicId = clinicA.Id,
                    PatientDetails = new PatientDetails
                    {
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        Allergies = (i == 1) ? new List<Allergy> { 
                            new Allergy { 
                                AllergyName = "Penicilina", 
                                Severity = "High", 
                                Notes = "QA Test",
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow 
                            } 
                        } : new List<Allergy>()
                    }
                };
                patients.Add(patient);
            }
            await context.Patients.AddRangeAsync(patients);
            await context.SaveChangesAsync();

            // 5. Historial Médico, Citas (100) y Recetas (50)
            int apptCounter = 0;
            foreach (var patient in patients)
            {
                var history = new MedicalHistory
                {
                    PatientDetailsId = patient.PatientDetails.Id,
                    SpecialtyType = "General",
                    SpecialtyData = "{}",
                    Diagnosis = "Chequeo General de QA",
                    DiagnosisDate = DateTime.UtcNow.AddDays(-random.Next(1, 30)),
                    ClinicalNotes = "Evaluación masiva generada por el seeder",
                    IsConfidential = true,
                    OwnerClinicId = clinicA.Id,
                    HealthcareProfessionalId = doctor.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await context.MedicalHistories.AddAsync(history);

                // Cada paciente tendrá 2 citas (Llegando al hito de 100 appointments)
                var date = DateTime.UtcNow.AddDays(-random.Next(2, 30));
                var timeHour = random.Next(8, 17);

                var pastAppt = new Appointment
                {
                    PatientId = patient.Id,
                    ClinicId = clinicA.Id,
                    UserId = doctor.Id,
                    Date = DateOnly.FromDateTime(date),
                    Time = new TimeOnly(timeHour, 0),
                    Status = AppointmentStatus.Completed,
                    Notes = $"Revisión mensual QA #{apptCounter++}",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var futureAppt = new Appointment
                {
                    PatientId = patient.Id,
                    ClinicId = clinicA.Id,
                    UserId = doctor.Id,
                    Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(random.Next(1, 45))),
                    Time = new TimeOnly(timeHour == 17 ? 8 : timeHour + 1, 30),
                    Status = AppointmentStatus.Scheduled,
                    Notes = $"Seguimiento programado QA #{apptCounter++}",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await context.Appointments.AddRangeAsync(pastAppt, futureAppt);
                
                
                // Receta para la Cita pasada (50 recetas con QR GUID)
                var prescriptionCode = Guid.NewGuid();
                var prescription = new Prescription
                {
                    UniqueCode = prescriptionCode,  // Representa el QR
                    DoctorId = doctor.Id,
                    PatientId = patient.Id,
                    Diagnosis = "Síndrome de QA recurrente",
                    Notes = "Generado automáticamente por el Seeder de QA",
                    IssuedAt = pastAppt.Date.ToDateTime(pastAppt.Time),
                    ExpiresAt = pastAppt.Date.ToDateTime(pastAppt.Time).AddDays(15),
                    Status = PrescriptionStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await context.Prescriptions.AddAsync(prescription);
                await context.SaveChangesAsync();

                var item = new PrescriptionItem
                {
                    PrescriptionId = prescription.Id,
                    MedicationName = "Automatización 500mg",
                    Dosage = "1 Script",
                    Frequency = "Cada ejecución",
                    Duration = "1 Sprint",
                    Instructions = $"Escaneable vía QR: {prescriptionCode}",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await context.PrescriptionItems.AddAsync(item);
                await context.SaveChangesAsync();
            }

            Console.WriteLine($"✅ Dummy Data inyectada: 50 Pacientes, {apptCounter} Citas, 50 Recetas con QRs.");
        }
    }
}
