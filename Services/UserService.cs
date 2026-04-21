using System.Security.Claims;
using MedPal.API.Repositories;

namespace MedPal.API.Services
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            Username = GetUsername() ?? string.Empty;
            Role = GetRole() ?? string.Empty;
            AccountId = GetAccountId() ?? string.Empty;
        }

        public string UserId => GetUserId() ?? string.Empty;
        public string Username { get; set; }
        public string Role { get; set; }
        public string AccountId { get; set; }   
        public string Specialty => GetSpecialty() ?? string.Empty;

        private string? GetUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        }

        private string? GetUsername()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
        }

        private string? GetRole()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value;
        }

        private string? GetAccountId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.Claims.FirstOrDefault(x => x.Type == "account_id")?.Value;
        }

        private string? GetSpecialty()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.Claims.FirstOrDefault(x => x.Type == "specialty")?.Value;
        }
    }
}