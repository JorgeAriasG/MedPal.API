using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MedPal.API.Authorization.Policies;

/// <summary>
/// Extensión para registrar todas las políticas de autorización en la inyección de dependencias.
/// Se usa en Program.cs para configurar las políticas.
/// </summary>
public static class AuthorizationPoliciesExtension
{
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            // Política para ver usuarios
            .AddPolicy("ViewUsersPolicy", policy =>
            {
                policy.RequireAssertion(context =>
                {
                    // Extraer el role del claim - usar ClaimTypes.Role que es el estándar de Microsoft
                    var roleClaim = context.User.FindFirst(ClaimTypes.Role);
                    if (roleClaim == null)
                        return false;

                    // SuperAdmin, AccountAdmin y ClinicAdmin pueden ver usuarios
                    return roleClaim.Value switch
                    {
                        "SuperAdmin" => true,
                        "AccountAdmin" => true,
                        "ClinicAdmin" => true,
                        _ => false
                    };
                });
            })

            // Política para ver pacientes
            .AddPolicy("ViewPatientsPolicy", policy =>
            {
                policy.RequireAssertion(context =>
                {
                    // Extraer el role del claim - usar ClaimTypes.Role que es el estándar de Microsoft
                    var roleClaim = context.User.FindFirst(ClaimTypes.Role);
                    if (roleClaim == null)
                        return false;

                    // SuperAdmin, AccountAdmin, ClinicAdmin y HealthProfessional pueden ver pacientes
                    return roleClaim.Value switch
                    {
                        "SuperAdmin" => true,
                        "AccountAdmin" => true,
                        "ClinicAdmin" => true,
                        "HealthProfessional" => true,
                        _ => false
                    };
                });
            })

            // Política para ver citas
            .AddPolicy("ViewAppointmentsPolicy", policy =>
            {
                policy.RequireAssertion(context =>
                {
                    // Extraer el role del claim - usar ClaimTypes.Role que es el estándar de Microsoft
                    var roleClaim = context.User.FindFirst(ClaimTypes.Role);
                    if (roleClaim == null)
                        return false;

                    // SuperAdmin, AccountAdmin, ClinicAdmin, HealthProfessional y Receptionist pueden ver citas
                    return roleClaim.Value switch
                    {
                        "SuperAdmin" => true,
                        "AccountAdmin" => true,
                        "ClinicAdmin" => true,
                        "HealthProfessional" => true,
                        "Receptionist" => true,
                        _ => false
                    };
                });
            })

            // Política para gestionar usuarios
            .AddPolicy("ManageUsersPolicy", policy =>
            {
                policy.RequireAssertion(context =>
                {
                    // Extraer el role del claim - usar ClaimTypes.Role que es el estándar de Microsoft
                    var roleClaim = context.User.FindFirst(ClaimTypes.Role);
                    if (roleClaim == null)
                        return false;

                    // Solo SuperAdmin, AccountAdmin y ClinicAdmin pueden gestionar usuarios
                    return roleClaim.Value switch
                    {
                        "SuperAdmin" => true,
                        "AccountAdmin" => true,
                        "ClinicAdmin" => true,
                        _ => false
                    };
                });
            })

            // Política para gestionar pacientes
            .AddPolicy("ManagePatientsPolicy", policy =>
            {
                policy.RequireAssertion(context =>
                {
                    // Extraer el role del claim - usar ClaimTypes.Role que es el estándar de Microsoft
                    var roleClaim = context.User.FindFirst(ClaimTypes.Role);
                    if (roleClaim == null)
                        return false;

                    // SuperAdmin, AccountAdmin, ClinicAdmin y HealthProfessional pueden gestionar pacientes
                    return roleClaim.Value switch
                    {
                        "SuperAdmin" => true,
                        "AccountAdmin" => true,
                        "ClinicAdmin" => true,
                        "HealthProfessional" => true,
                        _ => false
                    };
                });
            })

            // Política para ver auditoría
            .AddPolicy("ViewAuditLogPolicy", policy =>
            {
                policy.RequireAssertion(context =>
                {
                    // Extraer el role del claim - usar ClaimTypes.Role que es el estándar de Microsoft
                    var roleClaim = context.User.FindFirst(ClaimTypes.Role);
                    if (roleClaim == null)
                        return false;

                    // Solo SuperAdmin y AccountAdmin pueden ver auditoría
                    return roleClaim.Value switch
                    {
                        "SuperAdmin" => true,
                        "AccountAdmin" => true,
                        _ => false
                    };
                });
            })

            // Política para administración de cuentas
            .AddPolicy("AdministerAccountPolicy", policy =>
            {
                policy.RequireAssertion(context =>
                {
                    // Extraer el role del claim - usar ClaimTypes.Role que es el estándar de Microsoft
                    var roleClaim = context.User.FindFirst(ClaimTypes.Role);
                    if (roleClaim == null)
                        return false;

                    // Solo SuperAdmin y AccountAdmin pueden administrar cuentas
                    return roleClaim.Value switch
                    {
                        "SuperAdmin" => true,
                        "AccountAdmin" => true,
                        _ => false
                    };
                });
            })

            // Política para administración de clínicas
            .AddPolicy("AdministerClinicPolicy", policy =>
            {
                policy.RequireAssertion(context =>
                {
                    Console.WriteLine("[AdministerClinicPolicy] ENTRANDO AL POLICY");
                    
                    // Extraer el role del claim - usar ClaimTypes.Role que es el estándar de Microsoft
                    var roleClaim = context.User.FindFirst(ClaimTypes.Role);
                    Console.WriteLine($"[AdministerClinicPolicy] Role encontrado: {roleClaim?.Value ?? "NULL"}");
                    
                    if (roleClaim == null)
                    {
                        Console.WriteLine("[AdministerClinicPolicy] RECHAZADO: No hay claim role");
                        return false;
                    }

                    // SuperAdmin, AccountAdmin y ClinicAdmin pueden administrar clínicas
                    var result = roleClaim.Value switch
                    {
                        "SuperAdmin" => true,
                        "AccountAdmin" => true,
                        "ClinicAdmin" => true,
                        _ => false
                    };
                    
                    Console.WriteLine($"[AdministerClinicPolicy] RESULTADO: {result} para rol '{roleClaim.Value}'");
                    return result;
                });
            });

        return services;
    }
}
