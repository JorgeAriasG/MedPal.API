using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MedPal.API.Models;
using MedPal.API.Models.Authorization;

namespace MedPal.API.Data.Seeders
{
    /// <summary>
    /// Seeder para crear un SuperAdmin inicial si no existe.
    /// Ejecutado automáticamente en Program.cs
    /// </summary>
    public class SuperAdminSeeder
    {
        public static async Task SeedSuperAdminAsync(AppDbContext context)
        {
            // Verificar si ya existe un SuperAdmin
            var existingSuperAdmin = await context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == "superadmin@medpal.com");

            if (existingSuperAdmin != null)
            {
                // SuperAdmin ya existe, no hacer nada
                return;
            }

            // Obtener el rol SuperAdmin
            var superAdminRole = await context.Roles
                .FirstOrDefaultAsync(r => r.Name == "SuperAdmin");

            if (superAdminRole == null)
            {
                // Si no existe el rol SuperAdmin, salir (debería existir de AuthorizationSeeder)
                Console.WriteLine("ERROR: SuperAdmin role not found in database. Run AuthorizationSeeder first.");
                return;
            }

            // Crear SuperAdmin User
            var superAdminUser = new User
            {
                Name = "SuperAdmin",
                Email = "superadmin@medpal.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("SuperAdmin@123"),  // ⚠️ CAMBIAR EN PRODUCCIÓN
                Specialty = "System Administrator",
                ProfessionalLicenseNumber = "SA-SYSTEM-001",
                IsActive = true,
                IsDeleted = false,
                HasAcceptedPrivacyTerms = true,
                AccountId = 0,  // SuperAdmin pertenece a la Account del sistema
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await context.Users.AddAsync(superAdminUser);
            await context.SaveChangesAsync();

            // Asignar role SuperAdmin
            var userRole = new UserRole
            {
                UserId = superAdminUser.Id,
                RoleId = superAdminRole.Id,
                ClinicId = null,  // Global role, no clinic specific
                AssignedByUserId = null,
                AssignedAt = DateTime.UtcNow,
                ExpiresAt = null  // Never expires
            };

            await context.UserRoles.AddAsync(userRole);
            await context.SaveChangesAsync();

            Console.WriteLine("✅ SuperAdmin usuario creado exitosamente");
            Console.WriteLine("   Email: superadmin@medpal.com");
            Console.WriteLine("   Password: SuperAdmin@123");
            Console.WriteLine("   ⚠️  CAMBIAR PASSWORD DESPUÉS DE PRIMERA AUTENTICACIÓN");
        }
    }
}
