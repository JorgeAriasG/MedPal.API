using System.ComponentModel.DataAnnotations;

namespace MedPal.API.DTOs
{
    public class InitiateRegistrationRequestDTO
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string Password { get; set; }

        [Required]
        public string ConfirmPassword { get; set; }

        [Required]
        [StringLength(50)]
        public string ProfessionalLicenseNumber { get; set; }

        [StringLength(50)]
        public string? Specialty { get; set; }

        [Required]
        public bool AcceptPrivacyTerms { get; set; }

        public string? PlanName { get; set; }
    }

    public class InitiateRegistrationResponseDTO
    {
        public string ClientSecret { get; set; }
        public string SessionId { get; set; }
        public string PublishableKey { get; set; }
    }
}
