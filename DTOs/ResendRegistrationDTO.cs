using System.ComponentModel.DataAnnotations;

namespace MedPal.API.DTOs
{
    public class ResendRegistrationDTO
    {
        [Required]
        [Phone]
        public string Phone { get; set; } = string.Empty;
    }
}