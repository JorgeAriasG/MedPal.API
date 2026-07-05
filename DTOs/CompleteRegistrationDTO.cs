using System.Collections.Generic;

namespace MedPal.API.DTOs
{
    public class CompleteRegistrationRequestDTO
    {
        public string SessionId { get; set; }
    }

    public class CompleteRegistrationResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public string Role { get; set; }
        public int AccountId { get; set; }
        public int ClinicId { get; set; }
        public List<string>? Permissions { get; set; }
    }
}
