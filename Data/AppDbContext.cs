using System.Threading.Tasks;
using MedPal.API.Models;
using MedPal.API.Models.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Proxies;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MedPal.API.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Patient> Patients { get; set; }
        public DbSet<PatientDetails> PatientDetails { get; set; }
        public DbSet<MedicalHistory> MedicalHistories { get; set; }
        public DbSet<Allergy> Allergies { get; set; }
        public DbSet<InsuranceProvider> InsuranceProviders { get; set; }
        public DbSet<PatientInsurance> PatientInsurances { get; set; }
        public DbSet<Clinic> Clinics { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Settings> Settings { get; set; }
        public DbSet<UserTask> UserTasks { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<UserClinic> UserClinics { get; set; }

        // Authorization entities
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("DefaultConnection")
                              .UseLazyLoadingProxies()
                              .EnableSensitiveDataLogging();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure your entity relationships and constraints here
            var timeOnlyConverter = new ValueConverter<TimeOnly, TimeSpan>(
                t => t.ToTimeSpan(),
                ts => TimeOnly.FromTimeSpan(ts));

            var dateOnlyConverter = new ValueConverter<DateOnly, DateTime>(
                d => d.ToDateTime(TimeOnly.MinValue),
                dt => DateOnly.FromDateTime(dt));

            // Configure foreign keys for UserClinic
            modelBuilder.Entity<UserClinic>()
                .HasKey(uc => new { uc.UserId, uc.ClinicId });

            modelBuilder.Entity<UserClinic>()
                .HasOne(uc => uc.User)
                .WithMany(u => u.UserClinics)
                .HasForeignKey(uc => uc.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete

            modelBuilder.Entity<UserClinic>()
                .HasOne(uc => uc.Clinic)
                .WithMany(c => c.UserClinics)
                .HasForeignKey(uc => uc.ClinicId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete

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

            // Configure foreign keys for UserTask
            modelBuilder.Entity<UserTask>()
                .HasOne(ut => ut.Patient)
                .WithMany(p => p.UserTasks)
                .HasForeignKey(ut => ut.PatientId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete

            modelBuilder.Entity<UserTask>()
                .HasOne(ut => ut.User)
                .WithMany(u => u.UserTasks)
                .HasForeignKey(ut => ut.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete

            modelBuilder.Entity<Patient>()
                .HasOne(p => p.Clinic);

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
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId, ur.ClinicId });

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
                .OnDelete(DeleteBehavior.Restrict);

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

            base.OnModelCreating(modelBuilder);
        }
    }
}