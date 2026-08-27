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
            if (await context.Users.AnyAsync(u => u.Email == "doctor1@medpal.com"))
                return;

            var account = await context.Accounts.FirstOrDefaultAsync(a => a.Name == "MedPal System");
            if (account == null)
            {
                account = new Account { Name = "MedPal System", Description = "Cuenta base para Seeder", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
                context.Accounts.Add(account);
                await context.SaveChangesAsync();
            }

            var clinicA = new Clinic { Name = "Clínica Vida Sana", Location = "Av. Principal 123", ContactInfo = "555-1234", AccountId = account.Id, Open = new TimeOnly(8, 0), Close = new TimeOnly(20, 0), CreatedAt = DateTime.UtcNow };
            var clinicB = new Clinic { Name = "Centro Médico Los Alpes", Location = "Av. Secundaria 456", ContactInfo = "555-5678", AccountId = account.Id, Open = new TimeOnly(9, 0), Close = new TimeOnly(18, 0), CreatedAt = DateTime.UtcNow };
            context.Clinics.AddRange(clinicA, clinicB);
            await context.SaveChangesAsync();

            var healthProfessionalRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "HealthProfessional");
            var accountAdminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "AccountAdmin");
            var clinicAdminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "ClinicAdmin");
            var nurseRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Nurse");
            var receptionistRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Receptionist");

            var specialtyDoctors = new[]
            {
                new { Email = "doctor1@medpal.com",         Name = "Dr. John Smith",      Specialty = "Cardiolog\u00eda",   License = "MED-88990" },
                new { Email = "doctor.general@medpal.com",  Name = "Dra. Mar\u00eda Garc\u00eda", Specialty = "General",       License = "MED-88991" },
                new { Email = "doctor.pediatria@medpal.com",Name = "Dra. Ana L\u00f3pez",  Specialty = "Pediatr\u00eda",     License = "MED-88992" },
                new { Email = "doctor.dermatologia@medpal.com", Name = "Dr. Carlos Ruiz", Specialty = "Dermatolog\u00eda",  License = "MED-88993" },
                new { Email = "doctor.dental@medpal.com",   Name = "Dra. Laura Mart\u00ednez", Specialty = "Dental",        License = "MED-88994" },
                new { Email = "doctor.nutricion@medpal.com",Name = "Dr. Pedro S\u00e1nchez", Specialty = "Nutrici\u00f3n",     License = "MED-88995" },
            };

            var doctorUsers = new List<User>();
            foreach (var d in specialtyDoctors)
            {
                var user = new User
                {
                    Name = d.Name,
                    Email = d.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Doctor@123"),
                    Specialty = d.Specialty,
                    ProfessionalLicenseNumber = d.License,
                    IsActive = true,
                    HasAcceptedPrivacyTerms = true,
                    AccountId = account.Id,
                    ClinicId = clinicA.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                context.Users.Add(user);
                await context.SaveChangesAsync();

                if (healthProfessionalRole != null)
                {
                    context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = healthProfessionalRole.Id, ClinicId = clinicA.Id, AssignedAt = DateTime.UtcNow });
                    await context.SaveChangesAsync();
                }
                doctorUsers.Add(user);
            }

            var qaUsers = new[]
            {
                new { Email = "accountadmin@medpal.com",  Name = "Admin QA",        Password = "Admin@123",   Role = accountAdminRole,  IsAccountLevel = true  },
                new { Email = "clinicadmin2@medpal.com",  Name = "Clinic Admin QA",  Password = "Clinic@123",  Role = clinicAdminRole,   IsAccountLevel = false },
                new { Email = "nurse1@medpal.com",        Name = "Enfermera QA",     Password = "Nurse@123",   Role = nurseRole,         IsAccountLevel = false },
                new { Email = "receptionist1@medpal.com", Name = "Recepcionista QA", Password = "Recept@123",  Role = receptionistRole,  IsAccountLevel = false },
            };

            foreach (var qa in qaUsers)
            {
                if (qa.Role == null) continue;

                var newUser = new User
                {
                    Name = qa.Name,
                    Email = qa.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(qa.Password),
                    Specialty = "Administraci\u00f3n",
                    ProfessionalLicenseNumber = "",
                    IsActive = true,
                    HasAcceptedPrivacyTerms = true,
                    AccountId = account.Id,
                    ClinicId = clinicA.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                context.Users.Add(newUser);
                await context.SaveChangesAsync();

                context.UserRoles.Add(new UserRole
                {
                    UserId = newUser.Id,
                    RoleId = qa.Role.Id,
                    ClinicId = qa.IsAccountLevel ? null : clinicA.Id,
                    AssignedAt = DateTime.UtcNow
                });
                await context.SaveChangesAsync();
            }

            var random = new Random();
            var patients = new List<Patient>();
            Random _random = new Random();
            string[] FirstNames = { "Ana", "Maria", "Carlos", "Pedro", "Luis" };
            string[] LastNames = { "Samos", "Garcia", "Perez", "Rodriguez", "Lopez" };
            for (int i = 1; i <= 50; i++)
            {
                string rndName = $"{FirstNames[_random.Next(FirstNames.Length)]}";
                string rndLastName = $"{LastNames[_random.Next(LastNames.Length)]}";
                var patient = new Patient
                {
                    Name = rndName,
                    Email = $"{rndName.ToLower()}.{rndLastName.ToLower()}@clinicflow.com.mx",
                    Dob = new DateTime(random.Next(1950, 2010), random.Next(1, 12), random.Next(1, 28)),
                    Phone = $"555-000{i:D2}",
                    Address = $"Avenida Pruebas {100 + i}",
                    Gender = i % 2 == 0 ? "M" : "F",
                    Weight = i % 2 == 0 ? 80.0m : 70.0m,
                    Height = i % 2 == 0 ? 1.75m : 1.65m,
                    Middlename = "Test",
                    Lastname = rndLastName,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    PatientDetails = new PatientDetails
                    {
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        Allergies = i == 1 ? new List<Allergy> { new Allergy { AllergyName = "Penicilina", Severity = "High", Notes = "Alergia registrada por seeder", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow } } : new List<Allergy>()
                    }
                };
                patients.Add(patient);
            }
            context.Patients.AddRange(patients);
            await context.SaveChangesAsync();

            var anthropometryRecords = patients.Select(p => new AnthropometryRecord
            {
                PatientDetailsId = p.PatientDetails.Id,
                RecordedAt = DateTime.UtcNow,
                Weight = p.Gender == "M" ? 80.0m : 70.0m,
                Height = p.Gender == "M" ? 1.75m : 1.65m,
                Bmi = Math.Round((p.Gender == "M" ? 80.0m : 70.0m) / ((p.Gender == "M" ? 1.75m : 1.65m) * (p.Gender == "M" ? 1.75m : 1.65m)), 1),
                Waist = 92.0m,
                Hip = 106.0m,
                WaistHipRatio = 0.87m,
                Wrist = 16.0m,
                Thigh = 58.0m,
                Calf = 36.0m,
                TricepsSkinfold = 22.0m,
                BicepsSkinfold = 18.0m,
                SubscapularSkinfold = 25.0m,
                SuprailiacSkinfold = 28.0m,
                Notes = "Antropometr\u00eda inicial",
                CreatedAt = DateTime.UtcNow
            }).ToList();
            context.AnthropometryRecords.AddRange(anthropometryRecords);
            await context.SaveChangesAsync();

            var patientClinics = patients.Select(p => new PatientClinic { PatientId = p.Id, ClinicId = clinicA.Id, CreatedAt = DateTime.UtcNow }).ToList();
            context.PatientClinics.AddRange(patientClinics);

            var patientAccounts = patients.Select(p => new PatientAccount
            {
                PatientId = p.Id,
                AccountId = account.Id,
                IsPrimaryAccount = true,
                IsVerifiedByPatient = true,
                ConsentToShareProfile = true,
                CreatedAt = DateTime.UtcNow
            }).ToList();
            context.PatientAccounts.AddRange(patientAccounts);
            await context.SaveChangesAsync();

            var today = DateTime.UtcNow;
            var hours = new[] { 8, 9, 10, 11, 12, 14, 15, 16 };
            var minutes = new[] { 0, 30 };
            int apptCounter = 0;
            int rxCounter = 0;
            int doctorCount = doctorUsers.Count;

            foreach (var (patient, index) in patients.Select((p, i) => (p, i)))
            {
                var assignedDoctor = doctorUsers[index % doctorCount];

                context.MedicalHistories.Add(new MedicalHistory
                {
                    PatientDetailsId = patient.PatientDetails.Id,
                    SpecialtyType = assignedDoctor.Specialty,
                    SpecialtyData = "{}",
                    Diagnosis = "Chequeo general de rutina",
                    DiagnosisDate = today,
                    ClinicalNotes = "Paciente registrado mediante seeder de QA",
                    IsConfidential = false,
                    OwnerClinicId = clinicA.Id,
                    HealthcareProfessionalId = assignedDoctor.Id,
                    CreatedAt = today,
                    UpdatedAt = today
                });

                var pastDay = random.Next(0, 30);
                var futureDay = random.Next(1, 46);
                var hour1 = hours[random.Next(hours.Length)];
                var min1 = minutes[random.Next(minutes.Length)];
                var hour2 = hours[random.Next(hours.Length)];
                var min2 = minutes[random.Next(minutes.Length)];

                var statusRoll = random.Next(100);
                AppointmentStatus pastStatus;
                var pastDuration = 30;
                if (statusRoll < 60)
                {
                    pastStatus = AppointmentStatus.Completed;
                    pastDuration = random.Next(15, 61);
                }
                else if (statusRoll < 80)
                    pastStatus = AppointmentStatus.Cancelled;
                else if (statusRoll < 95)
                    pastStatus = AppointmentStatus.NoShow;
                else
                    pastStatus = AppointmentStatus.Rescheduled;

                context.Appointments.AddRange(
                    new Appointment
                    {
                        PatientId = patient.Id,
                        ClinicId = clinicA.Id,
                        UserId = assignedDoctor.Id,
                        Date = DateOnly.FromDateTime(today.AddDays(-pastDay)),
                        Time = new TimeOnly(hour1, min1),
                        Status = pastStatus,
                        DurationMinutes = pastDuration,
                        Notes = $"Consulta de rutina #{++apptCounter}",
                        CreatedAt = today,
                        UpdatedAt = today
                    },
                    new Appointment
                    {
                        PatientId = patient.Id,
                        ClinicId = clinicA.Id,
                        UserId = assignedDoctor.Id,
                        Date = DateOnly.FromDateTime(today.AddDays(futureDay)),
                        Time = new TimeOnly(hour2, min2),
                        Status = AppointmentStatus.Scheduled,
                        Notes = $"Consulta de seguimiento #{++apptCounter}",
                        CreatedAt = today,
                        UpdatedAt = today
                    }
                );

                var rx = new Prescription
                {
                    UniqueCode = Guid.NewGuid(),
                    DoctorId = assignedDoctor.Id,
                    PatientId = patient.Id,
                    Diagnosis = "Diagn\u00f3stico de rutina",
                    Notes = "Receta generada por seeder de QA",
                    IssuedAt = today,
                    ExpiresAt = today.AddDays(30),
                    Status = PrescriptionStatus.Active,
                    CreatedAt = today,
                    UpdatedAt = today
                };
                context.Prescriptions.Add(rx);
                await context.SaveChangesAsync();

                context.PrescriptionItems.Add(new PrescriptionItem
                {
                    PrescriptionId = rx.Id,
                    MedicationName = "Paracetamol 500mg",
                    Dosage = "1 tableta",
                    Frequency = "Cada 8 horas",
                    Duration = "7 d\u00edas",
                    Instructions = $"C\u00f3digo QR: {rx.UniqueCode}",
                    CreatedAt = today,
                    UpdatedAt = today
                });
                await context.SaveChangesAsync();
                rxCounter++;
            }

            // Seed cross-doctor consents: first 3 patients of doctor2 grant consent to doctor1
            var doctor2 = doctorUsers[1];
            var doctor2Patients = patients.Where((_, i) => i % doctorCount == 1).Take(3).ToList();
            foreach (var patient in doctor2Patients)
            {
                context.PatientConsents.Add(new PatientConsent
                {
                    PatientDetailsId = patient.PatientDetails.Id,
                    TargetDoctorId = doctorUsers[0].Id,
                    RequestingClinicId = clinicA.Id,
                    OwnerClinicId = clinicA.Id,
                    ConsentScope = "AllRecords",
                    IsApproved = true,
                    ConsentDate = today,
                    ExpiryDate = today.AddYears(1),
                    IsDeleted = false,
                    CreatedAt = today,
                    UpdatedAt = today
                });
            }

            await context.SaveChangesAsync();
            Console.WriteLine($"\u2705 Seeder: 6 doctores, 4 ops, 50 pacientes, {apptCounter} citas, {rxCounter} recetas, {doctor2Patients.Count} cross-doctor consents");
        }
    }
}
