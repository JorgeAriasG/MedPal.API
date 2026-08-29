using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MedPal.API.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace MedPal.API.Services.Implementations
{
    public class PatientTokenService : IPatientTokenService
    {
        private readonly IConfiguration _configuration;

        public PatientTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GeneratePatientToken(Patient patient, string email)
        {
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
                throw new InvalidOperationException("JWT key is not configured.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, patient.Id.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim("patient_id", patient.Id.ToString()),
                new Claim("user_type", "patient"),
                new Claim(ClaimTypes.Role, "Patient"),
                new Claim("role", "Patient"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    double.Parse(_configuration["Jwt:ExpiryInMinutes"] ?? "60")),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}