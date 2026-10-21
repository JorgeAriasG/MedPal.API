using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MedPal.API.Models;
using MedPal.API.Models.Authorization;
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

        /// <summary>
        /// Genera un token JWT para un usuario staff, emitiendo el contrato completo de claims.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Si el usuario no tiene roles asignados, lanza excepción para que el login
        /// retorne 403 sin emitir token.
        /// </exception>
        public string GenerateToken(User user)
        {
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
                throw new InvalidOperationException("JWT key is not configured.");

            // F3: Sin roles → no se emite JWT; el login debe retornar 403 "no roles assigned"
            if (user.UserRoles == null || user.UserRoles.Count == 0)
            {
                throw new InvalidOperationException("User has no roles assigned; cannot generate staff token. Login will return 403.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Claim "role" (primer rol, solo legacy UI). Se toma el primer rol de la lista.
            var firstRole = user.UserRoles.First().Role.Name;

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim("user_id", user.Id.ToString()),
                new Claim("email", user.Email),
                new Claim("user_type", "staff"),
                // account_id solo si tiene valor (SuperAdmin puede omitirlo)
                user.AccountId.HasValue
                    ? new Claim("account_id", user.AccountId.Value.ToString())
                    : null,
                // clinic_id solo si > 0
                user.ClinicId > 0
                    ? new Claim("clinic_id", user.ClinicId.ToString())
                    : null,
                // claim "roles" string[] canónico (se serializa como cadena separada por comas)
                new Claim("roles", string.Join(",", user.UserRoles.Select(u => u.Role.Name))),
                new Claim("role", firstRole), // legacy UI; NO usado para decisiones de autorización
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Filtrar nulos antes de crear el token
            claims = claims.Where(c => c != null).ToList();

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpiryInMinutes"] ?? "60")),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}