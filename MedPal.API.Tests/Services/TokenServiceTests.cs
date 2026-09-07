using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using MedPal.API.Models;
using MedPal.API.Models.Authorization;
using MedPal.API.Services;
using Microsoft.Extensions.Configuration;

namespace MedPal.API.Tests.Services
{
    /// <summary>
    /// Fase 1 (Endurecimiento crítico): fallback de token inseguro.
    /// Un usuario sin roles NO debe recibir claims de rol (ni "role" ni ClaimTypes.Role);
    /// el JWT se emite igual, pero con 0 permisos. Los claims de tenancy (account_id/clinic_id)
    /// se siguen emitiendo si existen.
    /// </summary>
    public class TokenServiceTests
    {
        private const string JwtKey = "test-secret-key-0123456789abcdef";

        private static TokenService CreateTokenService()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["Jwt:Key"] = JwtKey,
                    ["Jwt:Issuer"] = "https://api.clinicflow.com.mx",
                    ["Jwt:Audience"] = "clinicflow",
                    ["Jwt:ExpireMinutes"] = "60"
                })
                .Build();

            return new TokenService(configuration);
        }

        private static User CreateUser(params string[] roleNames)
        {
            var user = new User
            {
                Id = 1,
                Name = "Test User",
                Email = "test@clinicflow.test",
                PasswordHash = "hash",
                AccountId = 42,
                ClinicId = 7,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UserRoles = new List<UserRole>()
            };

            foreach (var roleName in roleNames)
            {
                user.UserRoles.Add(new UserRole
                {
                    UserId = user.Id,
                    RoleId = roleName.GetHashCode(),
                    Role = new Role { Name = roleName }
                });
            }

            return user;
        }

        private static IReadOnlyList<Claim> DecodeTokenClaims(string token)
        {
            return new JwtSecurityTokenHandler()
                .ReadJwtToken(token)
                .Claims
                .ToList();
        }

        [Fact]
        public void GenerateToken_UserWithoutRoles_DoesNotEmitRoleClaims()
        {
            var service = CreateTokenService();
            var claims = DecodeTokenClaims(service.GenerateToken(CreateUser()));

            Assert.DoesNotContain(claims, c => c.Type == "role");
            Assert.DoesNotContain(claims, c => c.Type == ClaimTypes.Role);
        }

        [Fact]
        public void GenerateToken_UserWithoutRoles_StillEmitsTenancyClaims()
        {
            var service = CreateTokenService();
            var claims = DecodeTokenClaims(service.GenerateToken(CreateUser()));

            Assert.Contains(claims, c => c.Type == "account_id" && c.Value == "42");
            Assert.Contains(claims, c => c.Type == "clinic_id" && c.Value == "7");
        }

        [Fact]
        public void GenerateToken_UserWithRoles_EmitsRoleClaims()
        {
            var service = CreateTokenService();
            var claims = DecodeTokenClaims(service.GenerateToken(CreateUser("HealthProfessional", "Nurse")));

            Assert.Contains(claims,
                c => (c.Type == "role" || c.Type == ClaimTypes.Role) && c.Value == "HealthProfessional");
            Assert.Contains(claims,
                c => (c.Type == "role" || c.Type == ClaimTypes.Role) && c.Value == "Nurse");
        }

        [Fact]
        public void GenerateToken_NurseRole_SerializesNurseCorrectly()
        {
            var service = CreateTokenService();
            var claims = DecodeTokenClaims(service.GenerateToken(CreateUser("Nurse")));

            Assert.Contains(claims,
                c => (c.Type == "role" || c.Type == ClaimTypes.Role) && c.Value == "Nurse");
        }
    }
}