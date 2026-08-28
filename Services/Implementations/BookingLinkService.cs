using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace MedPal.API.Services.Implementations
{
    public class BookingLinkService : IBookingLinkService
    {
        private readonly IConfiguration _config;

        public BookingLinkService(IConfiguration config)
        {
            _config = config;
        }

        public string Issue(int clinicId, int doctorId)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: new[]
                {
                    new Claim("type", "booking_share"),
                    new Claim("clinic_id", clinicId.ToString()),
                    new Claim("doctor_id", doctorId.ToString())
                },
                expires: DateTime.UtcNow.AddDays(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public (int ClinicId, int DoctorId)? Validate(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var validationParams = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _config["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _config["Jwt:Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"])),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(2)
                };

                var principal = handler.ValidateToken(token, validationParams, out _);
                var clinicClaim = principal.FindFirst("clinic_id")?.Value;
                var doctorClaim = principal.FindFirst("doctor_id")?.Value;
                var typeClaim = principal.FindFirst("type")?.Value;

                if (typeClaim != "booking_share" || clinicClaim == null || doctorClaim == null)
                    return null;

                if (!int.TryParse(clinicClaim, out int clinicId) || !int.TryParse(doctorClaim, out int doctorId))
                    return null;

                return (clinicId, doctorId);
            }
            catch
            {
                return null;
            }
        }
    }
}