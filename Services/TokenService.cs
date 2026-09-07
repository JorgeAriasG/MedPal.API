using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MedPal.API.Models;
using MedPal.API.Repositories;
using Microsoft.IdentityModel.Tokens;

namespace MedPal.API.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // public string GenerateToken(User user)
        // {
        //     var jwtKey = _configuration["Jwt:Key"];
        //     if (string.IsNullOrEmpty(jwtKey))
        //     {
        //         throw new InvalidOperationException("JWT key is not configured. Please set 'Jwt:Key' in configuration.");
        //     }

        //     var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        //     var key = System.Text.Encoding.UTF8.GetBytes(jwtKey);
        //     var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
        //     {
        //         Subject = new System.Security.Claims.ClaimsIdentity(new[]
        //         {
        //             new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()),
        //             new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, user.Email)
        //         }),
        //         Expires = DateTime.UtcNow.AddHours(1),
        //         SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
        //             new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
        //             Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
        //     };

        //     var token = tokenHandler.CreateToken(tokenDescriptor);
        //     return tokenHandler.WriteToken(token);
        // }

        public string GenerateToken(User user)
        {
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
                throw new InvalidOperationException("JWT key is not configured.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("user_id", user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Fase 2: Agregar claims para multi-tenancy
            if (user.AccountId.HasValue)
            {
                claims.Add(new Claim("account_id", user.AccountId.Value.ToString()));
            }

            if (user.ClinicId > 0)
            {
                claims.Add(new Claim("clinic_id", user.ClinicId.ToString()));
            }

            // Añadir roles del usuario (desde la relación UserRoles)
            // Fase 1: si el usuario NO tiene roles, NO se emiten claims de rol ni ClaimTypes.Role.
            // El JWT se emite igual (el login ya es el gate), pero sin roles el usuario tiene 0
            // permisos y la FallbackPolicy le barrará casi todo.
            if (user.UserRoles != null && user.UserRoles.Count > 0)
            {
                foreach (var userRole in user.UserRoles)
                {
                    if (userRole.Role != null)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));
                        // Fase 2: Agregar el primer rol como "role" para multi-tenancy
                        // (Si un usuario tiene múltiples roles, se usa el primero)
                        if (!claims.Any(c => c.Type == "role"))
                        {
                            claims.Add(new Claim("role", userRole.Role.Name));
                        }
                    }
                }
            }

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],  // ← Asegúrate de que esto está aquí
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpireMinutes"] ?? "60")),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}