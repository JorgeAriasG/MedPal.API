using MedPal.API.Models.Authorization;
using Microsoft.EntityFrameworkCore;

namespace MedPal.API.Data.Seeders
{
    /// <summary>
    /// Seeds default roles, permissions, and role-permission mappings
    /// </summary>
    public static class AuthorizationSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            // Seed roles
            await SeedRolesAsync(context);

            // Seed permissions
            await SeedPermissionsAsync(context);

            // Seed role-permission mappings
            await SeedRolePermissionsAsync(context);

            await context.SaveChangesAsync();
        }

        private static async Task SeedRolesAsync(AppDbContext context)
        {
            var roles = new List<Role>
            {
                new Role
                {
                    Name = "Admin",
                    Description = "System administrator with full access to all features",
                    IsSystemRole = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Role
                {
                    Name = "Doctor",
                    Description = "Medical doctor with access to patient records and appointments",
                    IsSystemRole = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Role
                {
                    Name = "Nurse",
                    Description = "Nurse with limited access to patient information",
                    IsSystemRole = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Role
                {
                    Name = "Receptionist",
                    Description = "Receptionist managing appointments and patient demographics",
                    IsSystemRole = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Role
                {
                    Name = "Patient",
                    Description = "Patient with access to own medical records",
                    IsSystemRole = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            foreach (var role in roles)
            {
                if (!await context.Roles.AnyAsync(r => r.Name == role.Name))
                {
                    await context.Roles.AddAsync(role);
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedPermissionsAsync(AppDbContext context)
        {
            var permissions = new List<Permission>
            {
                // Patient Permissions
                new Permission { Name = "Patients.ViewAll", Resource = "Patients", Action = "ViewAll", Description = "View all patients in the system", CreatedAt = DateTime.UtcNow },
                new Permission { Name = "Patients.ViewOwn", Resource = "Patients", Action = "ViewOwn", Description = "View own patient record", CreatedAt = DateTime.UtcNow },
                new Permission { Name = "Patients.Create", Resource = "Patients", Action = "Create", Description = "Create new patient records", CreatedAt = DateTime.UtcNow },
                new Permission { Name = "Patients.Update", Resource = "Patients", Action = "Update", Description = "Update patient information", CreatedAt = DateTime.UtcNow },
                new Permission { Name = "Patients.Delete", Resource = "Patients", Action = "Delete", Description = "Delete patient records", CreatedAt = DateTime.UtcNow },

                // Appointment Permissions
                new Permission { Name = "Appointments.ViewAll", Resource = "Appointments", Action = "ViewAll", Description = "View all appointments", CreatedAt = DateTime.UtcNow },
                new Permission { Name = "Appointments.ViewOwn", Resource = "Appointments", Action = "ViewOwn", Description = "View own appointments", CreatedAt = DateTime.UtcNow },
                new Permission { Name = "Appointments.Create", Resource = "Appointments", Action = "Create", Description = "Create new appointments", CreatedAt = DateTime.UtcNow },
                new Permission { Name = "Appointments.Update", Resource = "Appointments", Action = "Update", Description = "Update appointment details", CreatedAt = DateTime.UtcNow },
                new Permission { Name = "Appointments.Cancel", Resource = "Appointments", Action = "Cancel", Description = "Cancel appointments", CreatedAt = DateTime.UtcNow },

                // Medical Records Permissions
                new Permission { Name = "MedicalRecords.ViewAll", Resource = "MedicalRecords", Action = "ViewAll", Description = "View all medical records", CreatedAt = DateTime.UtcNow },
                new Permission { Name = "MedicalRecords.ViewOwn", Resource = "MedicalRecords", Action = "ViewOwn", Description = "View own medical records", CreatedAt = DateTime.UtcNow },
                new Permission { Name = "MedicalRecords.ViewAssigned", Resource = "MedicalRecords", Action = "ViewAssigned", Description = "View medical records of assigned patients", CreatedAt = DateTime.UtcNow },
                new Permission { Name = "MedicalRecords.Create", Resource = "MedicalRecords", Action = "Create", Description = "Create medical records", CreatedAt = DateTime.UtcNow },
                new Permission { Name = "MedicalRecords.Update", Resource = "MedicalRecords", Action = "Update", Description = "Update medical records", CreatedAt = DateTime.UtcNow },

                // Billing Permissions
                new Permission { Name = "Billing.View", Resource = "Billing", Action = "View", Description = "View billing information", CreatedAt = DateTime.UtcNow },
                new Permission { Name = "Billing.Manage", Resource = "Billing", Action = "Manage", Description = "Manage invoices and payments", CreatedAt = DateTime.UtcNow },

                // User Management Permissions
                new Permission { Name = "Users.ViewAll", Resource = "Users", Action = "ViewAll", Description = "View all users", CreatedAt = DateTime.UtcNow },
                new Permission { Name = "Users.Manage", Resource = "Users", Action = "Manage", Description = "Create, update, and delete users", CreatedAt = DateTime.UtcNow },
                new Permission { Name = "Users.ManageRoles", Resource = "Users", Action = "ManageRoles", Description = "Assign and remove user roles", CreatedAt = DateTime.UtcNow },

                // Reports Permissions
                new Permission { Name = "Reports.Generate", Resource = "Reports", Action = "Generate", Description = "Generate system reports", CreatedAt = DateTime.UtcNow },
                new Permission { Name = "Reports.View", Resource = "Reports", Action = "View", Description = "View generated reports", CreatedAt = DateTime.UtcNow },

                // Clinic Management Permissions
                new Permission { Name = "Clinics.View", Resource = "Clinics", Action = "View", Description = "View clinic information", CreatedAt = DateTime.UtcNow },
                new Permission { Name = "Clinics.Manage", Resource = "Clinics", Action = "Manage", Description = "Create and update clinic information", CreatedAt = DateTime.UtcNow },

                // Role Management Permissions
                new Permission { Name = "Roles.View", Resource = "Roles", Action = "View", Description = "View roles and their permissions", CreatedAt = DateTime.UtcNow },
                new Permission { Name = "Roles.Assign", Resource = "Roles", Action = "Assign", Description = "Assign roles to users", CreatedAt = DateTime.UtcNow },
                new Permission { Name = "Roles.Revoke", Resource = "Roles", Action = "Revoke", Description = "Remove roles from users", CreatedAt = DateTime.UtcNow },
                new Permission { Name = "Roles.ViewAudit", Resource = "Roles", Action = "ViewAudit", Description = "View role assignment and removal audit logs", CreatedAt = DateTime.UtcNow },
            };

            foreach (var permission in permissions)
            {
                if (!await context.Permissions.AnyAsync(p => p.Name == permission.Name))
                {
                    await context.Permissions.AddAsync(permission);
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedRolePermissionsAsync(AppDbContext context)
        {
            // Get all roles and permissions
            var admin = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            var doctor = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Doctor");
            var nurse = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Nurse");
            var receptionist = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Receptionist");
            var patient = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Patient");

            if (admin == null || doctor == null || nurse == null || receptionist == null || patient == null)
                return;

            // Admin: All permissions
            var allPermissions = await context.Permissions.ToListAsync();
            foreach (var permission in allPermissions)
            {
                if (!await context.RolePermissions.AnyAsync(rp => rp.RoleId == admin.Id && rp.PermissionId == permission.Id))
                {
                    await context.RolePermissions.AddAsync(new RolePermission
                    {
                        RoleId = admin.Id,
                        PermissionId = permission.Id,
                        GrantedAt = DateTime.UtcNow
                    });
                }
            }

            // Doctor: Patient and medical record management, appointments
            var doctorPermissions = new[]
            {
                "Patients.ViewAll", "Patients.Create", "Patients.Update",
                "Appointments.ViewAll", "Appointments.Create", "Appointments.Update", "Appointments.Cancel",
                "MedicalRecords.ViewAssigned", "MedicalRecords.Create", "MedicalRecords.Update",
                "Billing.View",
                "Reports.View",
                "Clinics.View"
            };
            await AssignPermissionsToRole(context, doctor.Id, doctorPermissions);

            // Nurse: View patients, manage appointments, limited medical records
            var nursePermissions = new[]
            {
                "Patients.ViewAll", "Patients.Update",
                "Appointments.ViewAll", "Appointments.Create", "Appointments.Update",
                "MedicalRecords.ViewAssigned",
                "Clinics.View"
            };
            await AssignPermissionsToRole(context, nurse.Id, nursePermissions);

            // Receptionist: Appointments and patient demographics
            var receptionistPermissions = new[]
            {
                "Patients.ViewAll", "Patients.Create", "Patients.Update",
                "Appointments.ViewAll", "Appointments.Create", "Appointments.Update", "Appointments.Cancel",
                "Billing.View",
                "Clinics.View"
            };
            await AssignPermissionsToRole(context, receptionist.Id, receptionistPermissions);

            // Patient: View own records only
            var patientPermissions = new[]
            {
                "Patients.ViewOwn",
                "Appointments.ViewOwn", "Appointments.Create",
                "MedicalRecords.ViewOwn",
                "Billing.View"
            };
            await AssignPermissionsToRole(context, patient.Id, patientPermissions);

            await context.SaveChangesAsync();
        }

        private static async Task AssignPermissionsToRole(AppDbContext context, int roleId, string[] permissionNames)
        {
            foreach (var permissionName in permissionNames)
            {
                var permission = await context.Permissions.FirstOrDefaultAsync(p => p.Name == permissionName);
                if (permission != null && !await context.RolePermissions.AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permission.Id))
                {
                    await context.RolePermissions.AddAsync(new RolePermission
                    {
                        RoleId = roleId,
                        PermissionId = permission.Id,
                        GrantedAt = DateTime.UtcNow
                    });
                }
            }
        }
    }
}
