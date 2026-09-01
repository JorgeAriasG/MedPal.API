using System.ComponentModel.DataAnnotations;

namespace MedPal.API.DTOs
{
    public class CompletePatientRegistrationDTO
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        public string? Name { get; set; }

        public string? Lastname { get; set; }
    }
}