using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using MedPal.API.Models;
using MedPal.API.Models.Authorization;
using MedPal.API.Services;
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
        public DbSet<ClinicalAttachment> ClinicalAttachments { get; set; }
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
        public DbSet<PatientAccount> PatientAccounts { get; set; }
        public DbSet<NotificationMessage> NotificationMessages { get; set; }
        public DbSet<WhatsAppInteraction> WhatsAppInteractions { get; set; }

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

        // Subscription entities
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<PendingRegistration> PendingRegistrations { get; set; }

        // Nutrition Module (Specialty: Nutrición)
        public DbSet<FoodItem> FoodItems { get; set; }
        public DbSet<BodyComposition> BodyCompositions { get; set; }
        public DbSet<AnthropometryRecord> AnthropometryRecords { get; set; }
        public DbSet<DietPlan> DietPlans { get; set; }
        public DbSet<DietPlanMeal> DietPlanMeals { get; set; }
        public DbSet<DietPlanMealItem> DietPlanMealItems { get; set; }
        public DbSet<NutritionProgress> NutritionProgresses { get; set; }
        public DbSet<Supplement> Supplements { get; set; }

        private readonly EncryptionProvider? _encryptionProvider;

        /// <summary>
        /// Immutable tenant snapshot extracted from JWT claims per-request.
        /// EF Core re-evaluates this field per query because it's a DbContext instance member.
        /// When HasContext is false (seeder/migration), query filters pass all rows.
        /// </summary>
        private readonly TenantSnapshot _tenant;

        public AppDbContext(DbContextOptions<AppDbContext> options, EncryptionProvider? encryptionProvider = null, IServiceProvider? serviceProvider = null) : base(options)
        {
            _encryptionProvider = encryptionProvider;
            _tenant = ExtractTenantSnapshot(serviceProvider);
        }

        /// <summary>
        /// Extracts raw tenant values from the current HTTP request's JWT claims.
        /// Each Scoped DbContext instance gets a fresh snapshot from the active HttpContext.
        /// Returns an empty snapshot (all nulls) when no HTTP context is available.
        /// </summary>
        private static TenantSnapshot ExtractTenantSnapshot(IServiceProvider? serviceProvider)
        {
            if (serviceProvider == null) return new TenantSnapshot();

            var httpContextAccessor = serviceProvider.GetService(typeof(IHttpContextAccessor)) as IHttpContextAccessor;
            var user = httpContextAccessor?.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true) return new TenantSnapshot();

            // Patient portal tokens are NOT tenant principals: their NameIdentifier is the
            // patient Id (PatientAuthController), not a staff UserId. Without this guard the
            // fallback below would set UserId=patientId -> HasContext=true -> tenant query
            // filters hide everything (e.g. clinic/all and appointments/my return nothing).
            // Treat patient tokens as no tenant context; their endpoints filter by patientId.
            if (string.Equals(user.FindFirst("user_type")?.Value, "patient", StringComparison.OrdinalIgnoreCase))
                return new TenantSnapshot();

            int.TryParse(user.FindFirst("account_id")?.Value, out var accountId);
            int.TryParse(user.FindFirst("clinic_id")?.Value, out var clinicId);
            var userIdClaim = user.FindFirst("user_id") ?? user.FindFirst(ClaimTypes.NameIdentifier);
            int.TryParse(userIdClaim?.Value, out var userId);

            return new TenantSnapshot
            {
                AccountId = accountId > 0 ? accountId : null,
                ClinicId = clinicId > 0 ? clinicId : null,
                UserId = userId > 0 ? userId : null,
                Role = user.FindFirst("role")?.Value
            };
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

            // Configure foreign keys for PatientAccount (patient membership in an account)
            modelBuilder.Entity<PatientAccount>()
                .HasKey(pa => new { pa.PatientId, pa.AccountId });

            modelBuilder.Entity<PatientAccount>()
                .HasOne(pa => pa.Patient)
                .WithMany(p => p.PatientAccounts)
                .HasForeignKey(pa => pa.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PatientAccount>()
                .HasOne(pa => pa.Account)
                .WithMany(a => a.PatientAccounts)
                .HasForeignKey(pa => pa.AccountId)
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

            modelBuilder.Entity<Appointment>()
                .Property(a => a.Notes)
                .HasDefaultValue("");

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

            // ClinicalAttachment Configuration
            modelBuilder.Entity<ClinicalAttachment>(entity =>
            {
                entity.HasOne(a => a.MedicalHistory)
                    .WithMany()
                    .HasForeignKey(a => a.MedicalHistoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.OwnerClinic)
                    .WithMany()
                    .HasForeignKey(a => a.OwnerClinicId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(a => new { a.MedicalHistoryId, a.IsDeleted });
            });

            // Soft delete filters (entities without tenant filters)
            modelBuilder.Entity<InsuranceProvider>().HasQueryFilter(ip => !ip.IsDeleted);
            modelBuilder.Entity<PatientClinic>().HasQueryFilter(pc => !pc.IsDeleted);
            modelBuilder.Entity<PatientAccount>().HasQueryFilter(pa => !pa.IsDeleted);
            // PatientAuth Configuration
            modelBuilder.Entity<PatientAuth>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();

                entity.HasOne(e => e.Patient)
                    .WithMany()
                    .HasForeignKey(e => e.PatientId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ========== NUTRITION MODULE CONFIGURATION ==========

            // FoodItem Configuration
            modelBuilder.Entity<FoodItem>(entity =>
            {
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.Category);

                entity.Property(e => e.ServingSize).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Calories).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Protein).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Carbs).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Fat).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Fiber).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Sodium).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Sugar).HasColumnType("decimal(10,2)");
                entity.Property(e => e.SaturatedFat).HasColumnType("decimal(10,2)");
                entity.Property(e => e.TransFat).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Cholesterol).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Potassium).HasColumnType("decimal(10,2)");
                entity.Property(e => e.VitaminA).HasColumnType("decimal(10,2)");
                entity.Property(e => e.VitaminC).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Calcium).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Iron).HasColumnType("decimal(10,2)");
            });

            // BodyComposition Configuration
            modelBuilder.Entity<BodyComposition>(entity =>
            {
                entity.HasOne(e => e.PatientDetails)
                    .WithMany()
                    .HasForeignKey(e => e.PatientDetailsId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.Weight).HasColumnType("decimal(6,2)");
                entity.Property(e => e.Height).HasColumnType("decimal(6,2)");
                entity.Property(e => e.Bmi).HasColumnType("decimal(5,2)");
                entity.Property(e => e.BodyFatPercentage).HasColumnType("decimal(5,2)");
                entity.Property(e => e.MuscleMass).HasColumnType("decimal(6,2)");
                entity.Property(e => e.BoneMass).HasColumnType("decimal(6,2)");
                entity.Property(e => e.BodyWaterPercentage).HasColumnType("decimal(5,2)");
                entity.Property(e => e.Bmr).HasColumnType("decimal(8,2)");
                entity.Property(e => e.ProteinMass).HasColumnType("decimal(6,2)");
                entity.Property(e => e.WaistHipRatio).HasColumnType("decimal(5,3)");
                entity.Property(e => e.BodyFatMass).HasColumnType("decimal(6,2)");
                entity.Property(e => e.TotalBodyWater).HasColumnType("decimal(6,2)");
                entity.Property(e => e.IntracellularWater).HasColumnType("decimal(6,2)");
                entity.Property(e => e.ExtracellularWater).HasColumnType("decimal(6,2)");
                entity.Property(e => e.EcwTbwRatio).HasColumnType("decimal(5,3)");
                entity.Property(e => e.Minerals).HasColumnType("decimal(6,2)");
                entity.Property(e => e.PhaseAngle).HasColumnType("decimal(5,2)");
                entity.Property(e => e.SegmentalLeanRightArm).HasColumnType("decimal(6,2)");
                entity.Property(e => e.SegmentalLeanLeftArm).HasColumnType("decimal(6,2)");
                entity.Property(e => e.SegmentalLeanTrunk).HasColumnType("decimal(6,2)");
                entity.Property(e => e.SegmentalLeanRightLeg).HasColumnType("decimal(6,2)");
                entity.Property(e => e.SegmentalLeanLeftLeg).HasColumnType("decimal(6,2)");

                entity.HasIndex(e => new { e.PatientDetailsId, e.RecordedAt });
            });

            // AnthropometryRecord Configuration
            modelBuilder.Entity<AnthropometryRecord>(entity =>
            {
                entity.HasOne(e => e.PatientDetails)
                    .WithMany()
                    .HasForeignKey(e => e.PatientDetailsId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.Waist).HasColumnType("decimal(6,2)");
                entity.Property(e => e.Hip).HasColumnType("decimal(6,2)");
                entity.Property(e => e.WaistHipRatio).HasColumnType("decimal(5,3)");
                entity.Property(e => e.Weight).HasColumnType("decimal(6,2)");
                entity.Property(e => e.Height).HasColumnType("decimal(6,2)");
                entity.Property(e => e.Bmi).HasColumnType("decimal(5,2)");
                entity.Property(e => e.WaistHeightRatio).HasColumnType("decimal(5,3)");
                entity.Property(e => e.MidArmCircumference).HasColumnType("decimal(6,2)");
                entity.Property(e => e.BodyFatPercentageEstimated).HasColumnType("decimal(5,2)");
                entity.Property(e => e.Neck).HasColumnType("decimal(6,2)");
                entity.Property(e => e.ShoulderBreadth).HasColumnType("decimal(6,2)");
                entity.Property(e => e.Chest).HasColumnType("decimal(6,2)");
                entity.Property(e => e.Arm).HasColumnType("decimal(6,2)");
                entity.Property(e => e.Forearm).HasColumnType("decimal(6,2)");
                entity.Property(e => e.Wrist).HasColumnType("decimal(6,2)");
                entity.Property(e => e.Thigh).HasColumnType("decimal(6,2)");
                entity.Property(e => e.Calf).HasColumnType("decimal(6,2)");
                entity.Property(e => e.TricepsSkinfold).HasColumnType("decimal(6,2)");
                entity.Property(e => e.BicepsSkinfold).HasColumnType("decimal(6,2)");
                entity.Property(e => e.SubscapularSkinfold).HasColumnType("decimal(6,2)");
                entity.Property(e => e.SuprailiacSkinfold).HasColumnType("decimal(6,2)");
                entity.Property(e => e.CalfSkinfold).HasColumnType("decimal(6,2)");
                entity.Property(e => e.ThighSkinfold).HasColumnType("decimal(6,2)");
                entity.Property(e => e.AbdominalSkinfold).HasColumnType("decimal(6,2)");
                entity.Property(e => e.PectoralSkinfold).HasColumnType("decimal(6,2)");
                entity.Property(e => e.AxillarySkinfold).HasColumnType("decimal(6,2)");

                entity.HasIndex(e => new { e.PatientDetailsId, e.RecordedAt });
            });

            // DietPlan Configuration
            modelBuilder.Entity<DietPlan>(entity =>
            {
                entity.HasOne(e => e.PatientDetails)
                    .WithMany()
                    .HasForeignKey(e => e.PatientDetailsId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.DailyCalories).HasColumnType("decimal(8,2)");
                entity.Property(e => e.ProteinG).HasColumnType("decimal(8,2)");
                entity.Property(e => e.CarbsG).HasColumnType("decimal(8,2)");
                entity.Property(e => e.FatG).HasColumnType("decimal(8,2)");
                entity.Property(e => e.FiberG).HasColumnType("decimal(8,2)");
                entity.Property(e => e.WaterMl).HasColumnType("decimal(8,2)");
                entity.Property(e => e.StartDate).HasConversion(dateOnlyConverter);
                entity.Property(e => e.EndDate).HasConversion(dateOnlyConverter);

                entity.HasIndex(e => new { e.PatientDetailsId, e.Status });
            });

            // DietPlanMeal Configuration
            modelBuilder.Entity<DietPlanMeal>(entity =>
            {
                entity.HasOne(e => e.DietPlan)
                    .WithMany(p => p.Meals)
                    .HasForeignKey(e => e.DietPlanId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.DietPlanId, e.MealOrder });
            });

            // DietPlanMealItem Configuration
            modelBuilder.Entity<DietPlanMealItem>(entity =>
            {
                entity.HasOne(e => e.DietPlanMeal)
                    .WithMany(m => m.Items)
                    .HasForeignKey(e => e.DietPlanMealId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.FoodItem)
                    .WithMany()
                    .HasForeignKey(e => e.FoodItemId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.Property(e => e.Quantity).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Calories).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Protein).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Carbs).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Fat).HasColumnType("decimal(10,2)");
            });

            // NutritionProgress Configuration
            modelBuilder.Entity<NutritionProgress>(entity =>
            {
                entity.HasOne(e => e.PatientDetails)
                    .WithMany()
                    .HasForeignKey(e => e.PatientDetailsId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.Weight).HasColumnType("decimal(6,2)");
                entity.Property(e => e.BodyFatPercentage).HasColumnType("decimal(5,2)");
                entity.Property(e => e.MuscleMass).HasColumnType("decimal(6,2)");
                entity.Property(e => e.Waist).HasColumnType("decimal(6,2)");
                entity.Property(e => e.CaloriesConsumed).HasColumnType("decimal(8,2)");
                entity.Property(e => e.ProteinConsumed).HasColumnType("decimal(8,2)");
                entity.Property(e => e.CarbsConsumed).HasColumnType("decimal(8,2)");
                entity.Property(e => e.FatConsumed).HasColumnType("decimal(8,2)");
                entity.Property(e => e.SkeletalMuscleMass).HasColumnType("decimal(6,2)");
                entity.Property(e => e.WaistCircumference).HasColumnType("decimal(6,2)");

                entity.HasIndex(e => new { e.PatientDetailsId, e.RecordedAt });
            });

            // Supplement Configuration
            modelBuilder.Entity<Supplement>(entity =>
            {
                entity.HasOne(e => e.PatientDetails)
                    .WithMany()
                    .HasForeignKey(e => e.PatientDetailsId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.StartDate).HasConversion(dateOnlyConverter);
                entity.Property(e => e.EndDate).HasConversion(dateOnlyConverter);

                entity.HasIndex(e => new { e.PatientDetailsId, e.IsActive });
            });

            // Soft delete query filters for Nutrition entities (no tenant scoping needed)
            modelBuilder.Entity<FoodItem>().HasQueryFilter(fi => !fi.IsDeleted);
            modelBuilder.Entity<DietPlanMeal>().HasQueryFilter(dpm => !dpm.IsDeleted);
            modelBuilder.Entity<DietPlanMealItem>().HasQueryFilter(dpmi => !dpmi.IsDeleted);

            // WhatsAppInteractions Configuration
            modelBuilder.Entity<WhatsAppInteraction>(entity =>
            {
                entity.HasOne(e => e.Appointment)
                    .WithMany()
                    .HasForeignKey(e => e.AppointmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Patient)
                    .WithMany()
                    .HasForeignKey(e => e.PatientId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.NotificationMessage)
                    .WithMany()
                    .HasForeignKey(e => e.NotificationMessageId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => new { e.AppointmentId, e.ReceivedAt });
                entity.HasIndex(e => new { e.PatientPhone, e.ReceivedAt });
                entity.HasIndex(e => e.Wamid);
            });

            // Subscription configurations
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.Account)
                .WithMany(a => a.Subscriptions)
                .HasForeignKey(s => s.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.SubscriptionPlan)
                .WithMany()
                .HasForeignKey(s => s.SubscriptionPlanId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SubscriptionPlan>(entity =>
            {
                entity.Property(p => p.Price).HasPrecision(18, 2);
                entity.HasIndex(p => p.Name).IsUnique();
            });

            // NOTE: MedicalRecordAccessLog does NOT have soft delete filter (immutable audit trail per NOM-004)

            // =====================================================================
            // Multi-tenant query filters (soft delete + tenant scoping combined)
            //
            // Architecture: _tenant is a DbContext instance field (TenantSnapshot record).
            // EF Core re-evaluates instance member access per query, so each Scoped
            // DbContext instance uses the current request's JWT claims.
            //
            // When _tenant.HasContext is false (seeder/migration/background job),
            // all tenant guards pass — only soft-delete applies.
            // =====================================================================

            // User: SuperAdmin sees all, otherwise scoped by AccountId
            modelBuilder.Entity<User>()
                .HasQueryFilter(u =>
                    !u.IsDeleted && (
                        !_tenant.HasContext ||
                        _tenant.IsSuperAdmin ||
                        (_tenant.AccountId != null && u.AccountId == _tenant.AccountId)
                    ));

            // Clinic: SuperAdmin sees all, otherwise scoped by AccountId (legacy null passes through)
            modelBuilder.Entity<Clinic>()
                .HasQueryFilter(c =>
                    !c.IsDeleted && (
                        !_tenant.HasContext ||
                        _tenant.IsSuperAdmin ||
                        c.AccountId == null ||
                        (_tenant.AccountId != null && c.AccountId == _tenant.AccountId)
                    ));

            // Patient: SuperAdmin sees all, AccountAdmin by PatientAccounts M:N, ClinicAdmin by PatientClinics M:N
            modelBuilder.Entity<Patient>()
                .HasQueryFilter(p =>
                    !p.IsDeleted && (
                        !_tenant.HasContext ||
                        _tenant.IsSuperAdmin ||
                        (_tenant.AccountId != null &&
                            p.PatientAccounts.Any(pa =>
                                pa.AccountId == _tenant.AccountId && !pa.IsDeleted && (pa.IsPrimaryAccount || (pa.IsVerifiedByPatient && (pa.ConsentToShareProfile ?? false))))) ||
                        (_tenant.ClinicId != null &&
                            p.PatientClinics.Any(pc =>
                                pc.ClinicId == _tenant.ClinicId && !pc.IsDeleted))
                    ));

            modelBuilder.Entity<Patient>()
                .Property(p => p.Height)
                .HasColumnType("decimal(6,2)");

            modelBuilder.Entity<Patient>()
                .Property(p => p.Weight)
                .HasColumnType("decimal(6,2)");

            // Appointment: SuperAdmin sees all, AccountAdmin by PatientAccounts M:N, ClinicAdmin by direct ClinicId
            modelBuilder.Entity<Appointment>()
                .HasQueryFilter(a =>
                    !a.IsDeleted && (
                        !_tenant.HasContext ||
                        _tenant.IsSuperAdmin ||
                        (_tenant.AccountId != null && a.Patient.PatientAccounts.Any(pa => pa.AccountId == _tenant.AccountId && !pa.IsDeleted && (pa.IsPrimaryAccount || (pa.IsVerifiedByPatient && (pa.ConsentToShareProfile ?? false))))) ||
                        (_tenant.ClinicId != null && a.ClinicId == _tenant.ClinicId)
                    ));

            // Phase 2: Patient-Direct entities

            // Invoice: scoped by PatientAccounts M:N + Appointment.ClinicId
            modelBuilder.Entity<Invoice>()
                .HasQueryFilter(i =>
                    !i.IsDeleted && (
                        !_tenant.HasContext ||
                        _tenant.IsSuperAdmin ||
                        (_tenant.AccountId != null && i.Patient.PatientAccounts.Any(pa => pa.AccountId == _tenant.AccountId && !pa.IsDeleted && (pa.IsPrimaryAccount || (pa.IsVerifiedByPatient && (pa.ConsentToShareProfile ?? false))))) ||
                        (_tenant.ClinicId != null && i.Appointment.ClinicId == _tenant.ClinicId)
                    ));

            // Prescription: scoped by PatientAccounts M:N + PatientClinics for ClinicAdmin
            modelBuilder.Entity<Prescription>()
                .HasQueryFilter(p =>
                    !p.IsDeleted && (
                        !_tenant.HasContext ||
                        _tenant.IsSuperAdmin ||
                        (_tenant.AccountId != null && p.Patient.PatientAccounts.Any(pa => pa.AccountId == _tenant.AccountId && !pa.IsDeleted && (pa.IsPrimaryAccount || (pa.IsVerifiedByPatient && (pa.ConsentToShareProfile ?? false))))) ||
                        (_tenant.ClinicId != null &&
                            p.Patient.PatientClinics.Any(pc =>
                                pc.ClinicId == _tenant.ClinicId && !pc.IsDeleted))
                    ));

            // Report: scoped by PatientAccounts M:N + PatientClinics for ClinicAdmin
            modelBuilder.Entity<Report>()
                .HasQueryFilter(r =>
                    !r.IsDeleted && (
                        !_tenant.HasContext ||
                        _tenant.IsSuperAdmin ||
                        (_tenant.AccountId != null && r.Patient.PatientAccounts.Any(pa => pa.AccountId == _tenant.AccountId && !pa.IsDeleted && (pa.IsPrimaryAccount || (pa.IsVerifiedByPatient && (pa.ConsentToShareProfile ?? false))))) ||
                        (_tenant.ClinicId != null &&
                            r.Patient.PatientClinics.Any(pc =>
                                pc.ClinicId == _tenant.ClinicId && !pc.IsDeleted))
                    ));

            // EmergencyContact: scoped by PatientAccounts M:N + PatientClinics for ClinicAdmin
            modelBuilder.Entity<EmergencyContact>()
                .HasQueryFilter(ec =>
                    !ec.IsDeleted && (
                        !_tenant.HasContext ||
                        _tenant.IsSuperAdmin ||
                        (_tenant.AccountId != null && ec.Patient.PatientAccounts.Any(pa => pa.AccountId == _tenant.AccountId && !pa.IsDeleted && (pa.IsPrimaryAccount || (pa.IsVerifiedByPatient && (pa.ConsentToShareProfile ?? false))))) ||
                        (_tenant.ClinicId != null &&
                            ec.Patient.PatientClinics.Any(pc =>
                                pc.ClinicId == _tenant.ClinicId && !pc.IsDeleted))
                    ));

            // PatientInsurance: scoped by PatientAccounts M:N + PatientClinics for ClinicAdmin
            modelBuilder.Entity<PatientInsurance>()
                .HasQueryFilter(pi =>
                    !pi.IsDeleted && (
                        !_tenant.HasContext ||
                        _tenant.IsSuperAdmin ||
                        (_tenant.AccountId != null && pi.Patient.PatientAccounts.Any(pa => pa.AccountId == _tenant.AccountId && !pa.IsDeleted && (pa.IsPrimaryAccount || (pa.IsVerifiedByPatient && (pa.ConsentToShareProfile ?? false))))) ||
                        (_tenant.ClinicId != null &&
                            pi.Patient.PatientClinics.Any(pc =>
                                pc.ClinicId == _tenant.ClinicId && !pc.IsDeleted))
                    ));

            // WhatsAppInteraction: scoped by PatientAccounts M:N + Appointment.ClinicId
            modelBuilder.Entity<WhatsAppInteraction>()
                .HasQueryFilter(wi =>
                    !wi.IsDeleted && (
                        !_tenant.HasContext ||
                        _tenant.IsSuperAdmin ||
                        (_tenant.AccountId != null && wi.Patient.PatientAccounts.Any(pa => pa.AccountId == _tenant.AccountId && !pa.IsDeleted && (pa.IsPrimaryAccount || (pa.IsVerifiedByPatient && (pa.ConsentToShareProfile ?? false))))) ||
                        (_tenant.ClinicId != null && wi.Appointment.ClinicId == _tenant.ClinicId)
                    ));

            // ArcoRequest: scoped by PatientAccounts M:N + PatientClinics for ClinicAdmin (PatientId is nullable)
            modelBuilder.Entity<ArcoRequest>()
                .HasQueryFilter(ar =>
                    !ar.IsDeleted && (
                        !_tenant.HasContext ||
                        _tenant.IsSuperAdmin ||
                        (ar.PatientId != null && _tenant.AccountId != null && ar.Patient.PatientAccounts.Any(pa => pa.AccountId == _tenant.AccountId && !pa.IsDeleted && (pa.IsPrimaryAccount || (pa.IsVerifiedByPatient && (pa.ConsentToShareProfile ?? false))))) ||
                        (_tenant.ClinicId != null && ar.PatientId != null &&
                            ar.Patient.PatientClinics.Any(pc =>
                                pc.ClinicId == _tenant.ClinicId && !pc.IsDeleted))
                    ));

            // Phase 3: PatientDetails-Derived entities (chain: Entity → PatientDetails → Patient → Account/Clinics)

            // PatientDetails: scoped by PatientAccounts M:N + PatientClinics
            modelBuilder.Entity<PatientDetails>()
                .HasQueryFilter(pd =>
                    !pd.IsDeleted && (
                        !_tenant.HasContext ||
                        _tenant.IsSuperAdmin ||
                        (_tenant.AccountId != null && pd.Patient.PatientAccounts.Any(pa => pa.AccountId == _tenant.AccountId && !pa.IsDeleted && (pa.IsPrimaryAccount || (pa.IsVerifiedByPatient && (pa.ConsentToShareProfile ?? false))))) ||
                        (_tenant.ClinicId != null &&
                            pd.Patient.PatientClinics.Any(pc =>
                                pc.ClinicId == _tenant.ClinicId && !pc.IsDeleted))
                    ));

            // MedicalHistory: scoped by PatientDetails.Patient.PatientAccounts M:N + PatientClinics
            modelBuilder.Entity<MedicalHistory>()
                .HasQueryFilter(mh =>
                    !mh.IsDeleted && (
                        !_tenant.HasContext ||
                        _tenant.IsSuperAdmin ||
                        (_tenant.AccountId != null && mh.PatientDetails.Patient.PatientAccounts.Any(pa => pa.AccountId == _tenant.AccountId && !pa.IsDeleted && (pa.IsPrimaryAccount || (pa.IsVerifiedByPatient && (pa.ConsentToShareProfile ?? false))))) ||
                        (_tenant.ClinicId != null &&
                            mh.PatientDetails.Patient.PatientClinics.Any(pc =>
                                pc.ClinicId == _tenant.ClinicId && !pc.IsDeleted))
                    ));

            // Allergy: scoped by PatientDetails.Patient.PatientAccounts M:N + PatientClinics
            modelBuilder.Entity<Allergy>()
                .HasQueryFilter(a =>
                    !a.IsDeleted && (
                        !_tenant.HasContext ||
                        _tenant.IsSuperAdmin ||
                        (_tenant.AccountId != null && a.PatientDetails.Patient.PatientAccounts.Any(pa => pa.AccountId == _tenant.AccountId && !pa.IsDeleted && (pa.IsPrimaryAccount || (pa.IsVerifiedByPatient && (pa.ConsentToShareProfile ?? false))))) ||
                        (_tenant.ClinicId != null &&
                            a.PatientDetails.Patient.PatientClinics.Any(pc =>
                                pc.ClinicId == _tenant.ClinicId && !pc.IsDeleted))
                    ));

            // VitalSign: scoped by PatientDetails.Patient.PatientAccounts M:N + PatientClinics
            modelBuilder.Entity<VitalSign>()
                .Property(v => v.Temperature)
                .HasColumnType("decimal(5,2)");

            modelBuilder.Entity<VitalSign>()
                .Property(v => v.Weight)
                .HasColumnType("decimal(6,2)");

            modelBuilder.Entity<VitalSign>()
                .Property(v => v.Height)
                .HasColumnType("decimal(6,2)");

            modelBuilder.Entity<VitalSign>()
                .Property(v => v.Bmi)
                .HasColumnType("decimal(5,2)");

            modelBuilder.Entity<VitalSign>()
                .HasQueryFilter(vs =>
                    !vs.IsDeleted && (
                        !_tenant.HasContext ||
                        _tenant.IsSuperAdmin ||
                        (_tenant.AccountId != null && vs.PatientDetails.Patient.PatientAccounts.Any(pa => pa.AccountId == _tenant.AccountId && !pa.IsDeleted && (pa.IsPrimaryAccount || (pa.IsVerifiedByPatient && (pa.ConsentToShareProfile ?? false))))) ||
                        (_tenant.ClinicId != null &&
                            vs.PatientDetails.Patient.PatientClinics.Any(pc =>
                                pc.ClinicId == _tenant.ClinicId && !pc.IsDeleted))
                    ));

            // BodyComposition: scoped by PatientDetails.Patient.PatientAccounts M:N + PatientClinics
            modelBuilder.Entity<BodyComposition>()
                .HasQueryFilter(bc =>
                    !bc.IsDeleted && (
                        !_tenant.HasContext ||
                        _tenant.IsSuperAdmin ||
                        (_tenant.AccountId != null && bc.PatientDetails.Patient.PatientAccounts.Any(pa => pa.AccountId == _tenant.AccountId && !pa.IsDeleted && (pa.IsPrimaryAccount || (pa.IsVerifiedByPatient && (pa.ConsentToShareProfile ?? false))))) ||
                        (_tenant.ClinicId != null &&
                            bc.PatientDetails.Patient.PatientClinics.Any(pc =>
                                pc.ClinicId == _tenant.ClinicId && !pc.IsDeleted))
                    ));

            // AnthropometryRecord: scoped by PatientDetails.Patient.PatientAccounts M:N + PatientClinics
            modelBuilder.Entity<AnthropometryRecord>()
                .HasQueryFilter(ar =>
                    !ar.IsDeleted && (
                        !_tenant.HasContext ||
                        _tenant.IsSuperAdmin ||
                        (_tenant.AccountId != null && ar.PatientDetails.Patient.PatientAccounts.Any(pa => pa.AccountId == _tenant.AccountId && !pa.IsDeleted && (pa.IsPrimaryAccount || (pa.IsVerifiedByPatient && (pa.ConsentToShareProfile ?? false))))) ||
                        (_tenant.ClinicId != null &&
                            ar.PatientDetails.Patient.PatientClinics.Any(pc =>
                                pc.ClinicId == _tenant.ClinicId && !pc.IsDeleted))
                    ));

            // DietPlan: scoped by PatientDetails.Patient.PatientAccounts M:N + PatientClinics
            modelBuilder.Entity<DietPlan>()
                .HasQueryFilter(dp =>
                    !dp.IsDeleted && (
                        !_tenant.HasContext ||
                        _tenant.IsSuperAdmin ||
                        (_tenant.AccountId != null && dp.PatientDetails.Patient.PatientAccounts.Any(pa => pa.AccountId == _tenant.AccountId && !pa.IsDeleted && (pa.IsPrimaryAccount || (pa.IsVerifiedByPatient && (pa.ConsentToShareProfile ?? false))))) ||
                        (_tenant.ClinicId != null &&
                            dp.PatientDetails.Patient.PatientClinics.Any(pc =>
                                pc.ClinicId == _tenant.ClinicId && !pc.IsDeleted))
                    ));

            // NutritionProgress: scoped by PatientDetails.Patient.PatientAccounts M:N + PatientClinics
            modelBuilder.Entity<NutritionProgress>()
                .HasQueryFilter(np =>
                    !np.IsDeleted && (
                        !_tenant.HasContext ||
                        _tenant.IsSuperAdmin ||
                        (_tenant.AccountId != null && np.PatientDetails.Patient.PatientAccounts.Any(pa => pa.AccountId == _tenant.AccountId && !pa.IsDeleted && (pa.IsPrimaryAccount || (pa.IsVerifiedByPatient && (pa.ConsentToShareProfile ?? false))))) ||
                        (_tenant.ClinicId != null &&
                            np.PatientDetails.Patient.PatientClinics.Any(pc =>
                                pc.ClinicId == _tenant.ClinicId && !pc.IsDeleted))
                    ));

            // Supplement: scoped by PatientDetails.Patient.PatientAccounts M:N + PatientClinics
            modelBuilder.Entity<Supplement>()
                .HasQueryFilter(s =>
                    !s.IsDeleted && (
                        !_tenant.HasContext ||
                        _tenant.IsSuperAdmin ||
                        (_tenant.AccountId != null && s.PatientDetails.Patient.PatientAccounts.Any(pa => pa.AccountId == _tenant.AccountId && !pa.IsDeleted && (pa.IsPrimaryAccount || (pa.IsVerifiedByPatient && (pa.ConsentToShareProfile ?? false))))) ||
                        (_tenant.ClinicId != null &&
                            s.PatientDetails.Patient.PatientClinics.Any(pc =>
                                pc.ClinicId == _tenant.ClinicId && !pc.IsDeleted))
                    ));

            // Phase 4: Specialized + Authorization entities

            // PrescriptionItem: scoped via Prescription → Patient.Accounts M:N chain
            modelBuilder.Entity<PrescriptionItem>()
                .HasQueryFilter(pri =>
                    !pri.IsDeleted && (
                        !_tenant.HasContext ||
                        _tenant.IsSuperAdmin ||
                        (_tenant.AccountId != null && pri.Prescription.Patient.PatientAccounts.Any(pa => pa.AccountId == _tenant.AccountId && !pa.IsDeleted && (pa.IsPrimaryAccount || (pa.IsVerifiedByPatient && (pa.ConsentToShareProfile ?? false))))) ||
                        (_tenant.ClinicId != null &&
                            pri.Prescription.Patient.PatientClinics.Any(pc =>
                                pc.ClinicId == _tenant.ClinicId && !pc.IsDeleted))
                    ));

            // Payment: scoped via Invoice → Patient.Accounts M:N chain + Invoice.Appointment.ClinicId
            modelBuilder.Entity<Payment>()
                .HasQueryFilter(p =>
                    !p.IsDeleted && (
                        !_tenant.HasContext ||
                        _tenant.IsSuperAdmin ||
                        (_tenant.AccountId != null && p.Invoice.Patient.PatientAccounts.Any(pa => pa.AccountId == _tenant.AccountId && !pa.IsDeleted && (pa.IsPrimaryAccount || (pa.IsVerifiedByPatient && (pa.ConsentToShareProfile ?? false))))) ||
                        (_tenant.ClinicId != null && p.Invoice.Appointment.ClinicId == _tenant.ClinicId)
                    ));

            // NotificationMessage: scoped by User.AccountId + Appointment.ClinicId
            modelBuilder.Entity<NotificationMessage>()
                .HasQueryFilter(nm =>
                    !nm.IsDeleted && (
                        !_tenant.HasContext ||
                        _tenant.IsSuperAdmin ||
                        (_tenant.AccountId != null && nm.User.AccountId == _tenant.AccountId) ||
                        (_tenant.ClinicId != null && nm.Appointment != null && nm.Appointment.ClinicId == _tenant.ClinicId)
                    ));

            // UserRole: scoped by User.AccountId
            modelBuilder.Entity<UserRole>()
                .HasQueryFilter(ur =>
                    !ur.IsDeleted && (
                        !_tenant.HasContext ||
                        _tenant.IsSuperAdmin ||
                        (_tenant.AccountId != null && ur.User.AccountId == _tenant.AccountId)
                    ));

            // Settings: scoped by User.AccountId
            modelBuilder.Entity<Settings>()
                .HasQueryFilter(s =>
                    !s.IsDeleted && (
                        !_tenant.HasContext ||
                        _tenant.IsSuperAdmin ||
                        (_tenant.AccountId != null && s.User.AccountId == _tenant.AccountId)
                    ));

            // ClinicalAttachment: scoped via MedicalHistory → PatientDetails → Patient.Accounts M:N chain + OwnerClinicId
            modelBuilder.Entity<ClinicalAttachment>()
                .HasQueryFilter(ca =>
                    !ca.IsDeleted && (
                        !_tenant.HasContext ||
                        _tenant.IsSuperAdmin ||
                        (_tenant.AccountId != null && ca.MedicalHistory.PatientDetails.Patient.PatientAccounts.Any(pa => pa.AccountId == _tenant.AccountId && !pa.IsDeleted && (pa.IsPrimaryAccount || (pa.IsVerifiedByPatient && (pa.ConsentToShareProfile ?? false))))) ||
                        (_tenant.ClinicId != null && (
                            ca.OwnerClinicId == _tenant.ClinicId ||
                            ca.MedicalHistory.PatientDetails.Patient.PatientClinics.Any(pc =>
                                pc.ClinicId == _tenant.ClinicId && !pc.IsDeleted)))
                    ));

            // PatientConsent: scoped via PatientDetails → Patient.Accounts M:N chain + RequestingClinicId/OwnerClinicId
            modelBuilder.Entity<PatientConsent>()
                .HasQueryFilter(pc =>
                    !pc.IsDeleted && (
                        !_tenant.HasContext ||
                        _tenant.IsSuperAdmin ||
                        (_tenant.AccountId != null && pc.PatientDetails.Patient.PatientAccounts.Any(pa => pa.AccountId == _tenant.AccountId && !pa.IsDeleted && (pa.IsPrimaryAccount || (pa.IsVerifiedByPatient && (pa.ConsentToShareProfile ?? false))))) ||
                        (_tenant.ClinicId != null && (
                            pc.RequestingClinicId == _tenant.ClinicId ||
                            pc.OwnerClinicId == _tenant.ClinicId))
                    ));

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

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}