using System.ComponentModel.DataAnnotations;

namespace MedPal.API.DTOs
{
    public class WaitlistRegisterDTO
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(300)]
        public string Email { get; set; }

        [StringLength(100)]
        public string? Specialty { get; set; }

        [StringLength(200)]
        public string? ClinicName { get; set; }
    }
}
