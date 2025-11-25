using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MedPal.API.Models;
using Microsoft.IdentityModel.Tokens;

namespace MedPal.API.Repositories
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
                new Claim(ClaimTypes.Role, user.Role ?? "User"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

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