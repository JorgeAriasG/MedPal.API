using System;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using MedPal.API.Models;
using MedPal.API.Models.Authorization;
using MedPal.API.Services;
using MedPal.API.Services.Implementations;
using MedPal.API.Data.Converters;
using MedPal.API.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Proxies;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MedPal.API.Data
{
    public class AppDbContext : DbContext
    {
        // Multi-tenancy (Fase 1)
        public DbSet<Account> Accounts { get; set; }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<PatientDetails> PatientDetails { get; set; }
        public DbSet<MedicalHistory> MedicalHistories { get; set; }
        public DbSet<Allergy> Allergies { get; set; }
        public DbSet<InsuranceProvider> InsuranceProviders { get; set; }
        public DbSet<PatientInsurance> PatientInsurances { get; set; }
        public DbSet<EmergencyContact> EmergencyContacts { get; set; }
        public DbSet<Clinic> Clinics { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Settings> Settings { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PatientClinic> PatientClinics { get; set; }
        public DbSet<NotificationMessage> NotificationMessages { get; set; }

        // Authorization entities
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<RoleAuditLog> RoleAuditLogs { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<PrescriptionItem> PrescriptionItems { get; set; }
        public DbSet<ArcoRequest> ArcoRequests { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        // Phase 3: Consent and Audit (Consentimiento y Auditoría)
        public DbSet<PatientConsent> PatientConsents { get; set; }
        public DbSet<MedicalRecordAccessLog> MedicalRecordAccessLogs { get; set; }

        // Patient Portal Auth
        public DbSet<PatientAuth> PatientAuths { get; set; }

        // Vital Signs (Signos Vitales) for NOM-035 compliance
        public DbSet<VitalSign> VitalSigns { get; set; }

        // CIE-10 Diagnostic Codes Catalog
        public DbSet<Cie10Code> Cie10Codes { get; set; }

        // Waitlist for landing page early access
        public DbSet<WaitlistEntry> WaitlistEntries { get; set; }

        private readonly EncryptionProvider? _encryptionProvider;
        private readonly IServiceProvider? _serviceProvider;
        private ITenantContextService? _tenantContext;

        public AppDbContext(DbContextOptions<AppDbContext> options, EncryptionProvider? encryptionProvider = null, IServiceProvider? serviceProvider = null) : base(options)
        {
            _encryptionProvider = encryptionProvider;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Lazy-loads TenantContextService to avoid circular dependency issues.
        /// TenantContextService is only needed when query filters are applied.
        /// </summary>
        private ITenantContextService? GetTenantContext()
        {
            if (_tenantContext != null) return _tenantContext;
            
            // Try to get from service provider (runtime)
            _tenantContext = _serviceProvider?.GetService(typeof(ITenantContextService)) as ITenantContextService;
            
            return _tenantContext;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("DefaultConnection")
                              .UseLazyLoadingProxies()
                              .EnableSensitiveDataLogging()
                              .LogTo(Console.WriteLine)
                              .LogTo(message => System.Diagnostics.Debug.WriteLine(message));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Fase 1: Multi-tenancy Account relationships
            modelBuilder.Entity<Account>()
                .HasMany(a => a.Clinics)
                .WithOne(c => c.Account)
                .HasForeignKey(c => c.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Account>()
                .HasMany(a => a.Users)
                .WithOne(u => u.Account)
                .HasForeignKey(u => u.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Account>()
                .HasMany(a => a.Patients)
                .WithOne(p => p.Account)
                .HasForeignKey(p => p.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure your entity relationships and constraints here
            var timeOnlyConverter = new ValueConverter<TimeOnly, TimeSpan>(
                t => t.ToTimeSpan(),
                ts => TimeOnly.FromTimeSpan(ts));

            var dateOnlyConverter = new ValueConverter<DateOnly, DateTime>(
                d => d.ToDateTime(TimeOnly.MinValue),
                dt => DateOnly.FromDateTime(dt));

            // Configure foreign keys for PatientClinic
            modelBuilder.Entity<PatientClinic>()
                .HasKey(pc => new { pc.PatientId, pc.ClinicId });

            modelBuilder.Entity<PatientClinic>()
                .HasOne(pc => pc.Patient)
                .WithMany(p => p.PatientClinics)
                .HasForeignKey(pc => pc.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PatientClinic>()
                .HasOne(pc => pc.Clinic)
                .WithMany(c => c.PatientClinics)
                .HasForeignKey(pc => pc.ClinicId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure foreign keys for Invoice
            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Patient)
                .WithMany(p => p.Invoices)
                .HasForeignKey(i => i.PatientId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Appointment)
                .WithMany(a => a.Invoices)
                .HasForeignKey(i => i.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete

            // Configure other foreign keys similarly
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Invoice)
                .WithMany(i => i.Payments)
                .HasForeignKey(p => p.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete

            modelBuilder.Entity<Clinic>()
                .Property(c => c.Open)
                .HasConversion(timeOnlyConverter);

            modelBuilder.Entity<Clinic>()
                .Property(c => c.Close)
                .HasConversion(timeOnlyConverter);

            modelBuilder.Entity<Appointment>()
                .Property(a => a.Date)
                .HasConversion(dateOnlyConverter);

            modelBuilder.Entity<Appointment>()
                .Property(a => a.Time)
                .HasConversion(timeOnlyConverter);

            modelBuilder.Entity<Invoice>()
                .Property(i => i.TotalAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Invoice>()
                .Property(i => i.PaidAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Payment>()
                .Property(p => p.AmountPaid)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<MedicalHistory>()
                .Property(mh => mh.Diagnosis)
                .HasColumnName("Diagnosis");

            modelBuilder.Entity<MedicalHistory>()
                .Property(mh => mh.IsConfidential)
                .HasDefaultValue(true);

            // Configure foreign keys for MedicalHistory con soft delete
            modelBuilder.Entity<MedicalHistory>()
                .HasOne(mh => mh.HealthcareProfessional)
                .WithMany(u => u.CreatedMedicalHistories)
                .HasForeignKey(mh => mh.HealthcareProfessionalId)
                .OnDelete(DeleteBehavior.NoAction); // Cambiar a NoAction

            modelBuilder.Entity<MedicalHistory>()
                .HasOne(mh => mh.LastModifiedByUser)
                .WithMany(u => u.ModifiedMedicalHistories)
                .HasForeignKey(mh => mh.LastModifiedByUserId)
                .OnDelete(DeleteBehavior.NoAction); // Cambiar a NoAction

            // ========== AUTHORIZATION CONFIGURATION ==========

            // Role configuration
            modelBuilder.Entity<Role>()
                .HasIndex(r => r.Name)
                .IsUnique();

            // Permission configuration
            modelBuilder.Entity<Permission>()
                .HasIndex(p => p.Name)
                .IsUnique();

            // UserRole configuration (composite key)
            // Primary key is (UserId, RoleId) only - ClinicId is nullable
            // This allows: 1) Global roles (ClinicId = null), 2) Clinic-specific roles (ClinicId = not null)
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Clinic)
                .WithMany()
                .HasForeignKey(ur => ur.ClinicId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.AssignedByUser)
                .WithMany()
                .HasForeignKey(ur => ur.AssignedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // RolePermission configuration (composite key)
            modelBuilder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.GrantedByUser)
                .WithMany()
                .HasForeignKey(rp => rp.GrantedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // RoleAuditLog configuration
            modelBuilder.Entity<RoleAuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Role)
                    .WithMany()
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Clinic)
                    .WithMany()
                    .HasForeignKey(e => e.ClinicId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.AssignedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.AssignedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes for optimized queries
                entity.HasIndex(e => new { e.UserId, e.Timestamp });
                entity.HasIndex(e => new { e.ClinicId, e.Timestamp });
                entity.HasIndex(e => e.Timestamp);
            });

            // Prescription Configuration
            modelBuilder.Entity<Prescription>(entity =>
            {
                entity.HasIndex(e => e.UniqueCode).IsUnique();

                entity.HasOne(e => e.Doctor)
                    .WithMany()
                    .HasForeignKey(e => e.DoctorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Patient)
                    .WithMany()
                    .HasForeignKey(e => e.PatientId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<PrescriptionItem>(entity =>
            {
                entity.HasOne(e => e.Prescription)
                    .WithMany(p => p.Items)
                    .HasForeignKey(e => e.PrescriptionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Report Configuration
            modelBuilder.Entity<Report>(entity =>
            {
                // Indexes para consultas frecuentes
                entity.HasIndex(e => new { e.PatientId, e.CreatedAt });
                entity.HasIndex(e => e.ReportType);
                entity.HasIndex(e => e.IsArcoReport);
            });

            // AuditLog Configuration
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes para auditoría (NOM-004)
                entity.HasIndex(e => new { e.UserId, e.Timestamp });
                entity.HasIndex(e => new { e.EntityType, e.EntityId });
                entity.HasIndex(e => e.Action);
                entity.HasIndex(e => e.Timestamp);
            });

            // Apply encryption to sensitive medical data (only if provider is available)
            if (_encryptionProvider != null)
            {
                var encryptedConverter = new EncryptedConverter(_encryptionProvider);

                modelBuilder.Entity<MedicalHistory>()
                    .Property(e => e.Diagnosis)
                    .HasConversion(encryptedConverter);

                modelBuilder.Entity<MedicalHistory>()
                    .Property(e => e.ClinicalNotes)
                    .HasConversion(encryptedConverter);

                modelBuilder.Entity<MedicalHistory>()
                    .Property(e => e.SpecialtyData)
                    .HasConversion(encryptedConverter);

                modelBuilder.Entity<Prescription>()
                    .Property(e => e.Diagnosis)
                    .HasConversion(encryptedConverter);

                modelBuilder.Entity<Prescription>()
                    .Property(e => e.Notes)
                    .HasConversion(encryptedConverter);
            }

            // ========== PHASE 3: CONSENT AND AUDIT CONFIGURATION ==========

            // PatientConsent Configuration
            modelBuilder.Entity<PatientConsent>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Foreign key relationships
                entity.HasOne(e => e.PatientDetails)
                    .WithMany()
                    .HasForeignKey(e => e.PatientDetailsId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.RequestingClinic)
                    .WithMany()
                    .HasForeignKey(e => e.RequestingClinicId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.OwnerClinic)
                    .WithMany()
                    .HasForeignKey(e => e.OwnerClinicId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ApprovedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.ApprovedByUserId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.TargetDoctor)
                    .WithMany()
                    .HasForeignKey(e => e.TargetDoctorId)
                    .OnDelete(DeleteBehavior.NoAction);

                // Composite unique index: Only one active consent per clinic pair per patient
                entity.HasIndex(e => new { e.PatientDetailsId, e.RequestingClinicId, e.OwnerClinicId, e.IsDeleted })
                    .HasName("IX_PatientConsent_Unique");

                // Indexes for common queries
                entity.HasIndex(e => new { e.PatientDetailsId, e.IsApproved })
                    .HasName("IX_PatientConsent_PatientApproved");

                entity.HasIndex(e => new { e.RequestingClinicId, e.IsDeleted })
                    .HasName("IX_PatientConsent_RequestingClinic");

                entity.HasIndex(e => e.ExpiryDate)
                    .HasName("IX_PatientConsent_ExpiryDate");

                entity.HasIndex(e => e.CreatedAt)
                    .HasName("IX_PatientConsent_CreatedAt");
            });

            // MedicalRecordAccessLog Configuration
            modelBuilder.Entity<MedicalRecordAccessLog>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Foreign key relationships
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.MedicalHistory)
                    .WithMany()
                    .HasForeignKey(e => e.MedicalHistoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.PatientDetails)
                    .WithMany()
                    .HasForeignKey(e => e.PatientDetailsId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.AccessingClinic)
                    .WithMany()
                    .HasForeignKey(e => e.AccessingClinicId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.OwnerClinic)
                    .WithMany()
                    .HasForeignKey(e => e.MedicalRecordOwnerClinicId)
                    .OnDelete(DeleteBehavior.NoAction);

                // Indexes for audit queries (NOM-004 compliance)
                entity.HasIndex(e => new { e.UserId, e.AccessTime })
                    .HasName("IX_AccessLog_UserTime");

                entity.HasIndex(e => new { e.PatientDetailsId, e.AccessTime })
                    .HasName("IX_AccessLog_PatientTime");

                entity.HasIndex(e => new { e.MedicalHistoryId, e.AccessTime })
                    .HasName("IX_AccessLog_HistoryTime");

                entity.HasIndex(e => e.AccessTime)
                    .HasName("IX_AccessLog_AccessTime");

                entity.HasIndex(e => e.HadValidConsent)
                    .HasName("IX_AccessLog_Consent");

                entity.HasIndex(e => new { e.AccessingClinicId, e.AccessTime })
                    .HasName("IX_AccessLog_ClinicTime");
            });

            // Update MedicalHistory to include OwnerClinic configuration
            modelBuilder.Entity<MedicalHistory>()
                .HasOne(mh => mh.OwnerClinic)
                .WithMany()
                .HasForeignKey(mh => mh.OwnerClinicId)
                .OnDelete(DeleteBehavior.NoAction);

            // Add query filters for soft delete pattern
            // Automatically filter out deleted records from all queries
            modelBuilder.Entity<Patient>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<Clinic>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<Prescription>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<Invoice>().HasQueryFilter(i => !i.IsDeleted);
            modelBuilder.Entity<Payment>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<Report>().HasQueryFilter(r => !r.IsDeleted);
            modelBuilder.Entity<Allergy>().HasQueryFilter(a => !a.IsDeleted);
            modelBuilder.Entity<InsuranceProvider>().HasQueryFilter(ip => !ip.IsDeleted);
            modelBuilder.Entity<NotificationMessage>().HasQueryFilter(nm => !nm.IsDeleted);
            modelBuilder.Entity<Appointment>().HasQueryFilter(a => !a.IsDeleted);
            modelBuilder.Entity<MedicalHistory>().HasQueryFilter(mh => !mh.IsDeleted);
            modelBuilder.Entity<PatientDetails>().HasQueryFilter(pd => !pd.IsDeleted);
            modelBuilder.Entity<PatientInsurance>().HasQueryFilter(pi => !pi.IsDeleted);
            modelBuilder.Entity<ArcoRequest>().HasQueryFilter(ar => !ar.IsDeleted);
            modelBuilder.Entity<Settings>().HasQueryFilter(s => !s.IsDeleted);
            modelBuilder.Entity<PrescriptionItem>().HasQueryFilter(pri => !pri.IsDeleted);
            modelBuilder.Entity<UserRole>().HasQueryFilter(ur => !ur.IsDeleted);
            modelBuilder.Entity<PatientClinic>().HasQueryFilter(pc => !pc.IsDeleted);
            modelBuilder.Entity<PatientConsent>().HasQueryFilter(pc => !pc.IsDeleted);
            // PatientAuth Configuration
            modelBuilder.Entity<PatientAuth>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();

                entity.HasOne(e => e.Patient)
                    .WithMany()
                    .HasForeignKey(e => e.PatientId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<VitalSign>().HasQueryFilter(vs => !vs.IsDeleted);

            // NOTE: MedicalRecordAccessLog does NOT have soft delete filter (immutable audit trail per NOM-004)

            // // Fase 2: Query Filters para Multi-tenancy (Control de Acceso)
            // var tenantContext = GetTenantContext();
            // if (tenantContext != null)
            // {
            //     // Query Filter para User: 
            //     // SuperAdmin ve todos, AccountAdmin/ClinicAdmin ve solo de su scope
            //     modelBuilder.Entity<User>()
            //         .HasQueryFilter(u =>
            //             tenantContext.IsSuperAdmin ||
            //             (tenantContext.CurrentAccountId != null && u.AccountId == tenantContext.CurrentAccountId)
            //         );

            //     // Query Filter para Clinic:
            //     // SuperAdmin ve todos, AccountAdmin/ClinicAdmin ve solo de su scope
            //     modelBuilder.Entity<Clinic>()
            //         .HasQueryFilter(c =>
            //             tenantContext.IsSuperAdmin ||
            //             c.AccountId == null ||  // Permitir datos legacy sin AccountId
            //             (tenantContext.CurrentAccountId != null && c.AccountId == tenantContext.CurrentAccountId)
            //         );

            //     // Query Filter para Patient:
            //     // SuperAdmin ve todos, AccountAdmin ve pacientes de su cuenta, ClinicAdmin/Doctor ve de su clínica
            //     modelBuilder.Entity<Patient>()
            //         .HasQueryFilter(p =>
            //             tenantContext.IsSuperAdmin ||
            //             p.AccountId == null ||  // Permitir datos legacy sin AccountId
            //             (tenantContext.IsAccountAdmin && p.AccountId == tenantContext.CurrentAccountId) ||
            //             (tenantContext.IsClinicAdmin && p.ClinicId == tenantContext.CurrentClinicId)
            //         );

            //     // Query Filter para Appointment:
            //     // Similar a Patient pero también filtra por clínica
            //     modelBuilder.Entity<Appointment>()
            //         .HasQueryFilter(a =>
            //             tenantContext.IsSuperAdmin ||
            //             (tenantContext.IsAccountAdmin && a.Patient.AccountId == tenantContext.CurrentAccountId) ||
            //             (tenantContext.IsClinicAdmin && a.ClinicId == tenantContext.CurrentClinicId)
            //         );
            // }

            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// Override SaveChangesAsync para aplicar validaciones automáticas
        /// </summary>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Validaciones de Invoice
            var invoicesToValidate = ChangeTracker.Entries<Invoice>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                .Select(e => e.Entity)
                .ToList();

            foreach (var invoice in invoicesToValidate)
            {
                // Invoice.TotalAmount debe ser > 0
                if (invoice.TotalAmount <= 0)
                    throw new InvalidOperationException($"Invoice {invoice.Id}: TotalAmount debe ser mayor a 0");

                // Invoice.PaidAmount no puede exceder TotalAmount
                if (invoice.PaidAmount > invoice.TotalAmount)
                    throw new InvalidOperationException($"Invoice {invoice.Id}: PaidAmount ({invoice.PaidAmount}) no puede exceder TotalAmount ({invoice.TotalAmount})");

                // Invoice.PaidAmount no puede ser negativo
                if (invoice.PaidAmount < 0)
                    throw new InvalidOperationException($"Invoice {invoice.Id}: PaidAmount no puede ser negativo");
            }

            // Validaciones de Payment
            var paymentsToValidate = ChangeTracker.Entries<Payment>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                .Select(e => e.Entity)
                .ToList();

            foreach (var payment in paymentsToValidate)
            {
                // Payment.AmountPaid debe ser > 0 (salvo si está siendo eliminado)
                var entry = ChangeTracker.Entries<Payment>().FirstOrDefault(e => e.Entity == payment);
                if (entry?.State != EntityState.Deleted && payment.AmountPaid <= 0)
                    throw new InvalidOperationException($"Payment {payment.Id}: AmountPaid debe ser mayor a 0");

                // Validar que no exceda el balance del invoice
                if (entry?.State == EntityState.Added)
                {
                    var invoice = Invoices.FirstOrDefault(i => i.Id == payment.InvoiceId && !i.IsDeleted);
                    if (invoice != null && payment.AmountPaid > invoice.RemainingAmount)
                        throw new InvalidOperationException($"Payment: AmountPaid ({payment.AmountPaid}) excede el balance pendiente ({invoice.RemainingAmount})");
                }
            }

            // Validaciones de Patient
            var patientsToValidate = ChangeTracker.Entries<Patient>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                .Select(e => e.Entity)
                .ToList();

            foreach (var patient in patientsToValidate)
            {
                // Patient.UpdatedAt no puede ser null
                if (patient.UpdatedAt == null)
                    patient.UpdatedAt = DateTime.UtcNow;
            }

            // Auto-stamp UpdatedAt para Appointments
            var appointmentsToStamp = ChangeTracker.Entries<Appointment>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                .Select(e => e.Entity)
                .ToList();

            foreach (var appt in appointmentsToStamp)
            {
                if (appt.UpdatedAt == null)
                    appt.UpdatedAt = DateTime.UtcNow;
            }

            // Auto-stamp UpdatedAt/CreatedAt para PrescriptionItems
            var prescItemsToStamp = ChangeTracker.Entries<PrescriptionItem>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                .Select(e => e.Entity)
                .ToList();

            foreach (var pi in prescItemsToStamp)
            {
                if (pi.UpdatedAt == null || pi.UpdatedAt == default(DateTime))
                    pi.UpdatedAt = DateTime.UtcNow;
                if (pi.CreatedAt == null || pi.CreatedAt == default(DateTime))
                    pi.CreatedAt = DateTime.UtcNow;
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}