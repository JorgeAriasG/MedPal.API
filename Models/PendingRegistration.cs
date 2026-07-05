using System;
using System.ComponentModel.DataAnnotations;

namespace MedPal.API.Models
{
    public class PendingRegistration
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; }

        [Required]
        public string RegistrationData { get; set; }

        [Required]
        [StringLength(100)]
        public string StripeSessionId { get; set; }

        [StringLength(100)]
        public string? StripeCustomerId { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "pending";

        public int? AccountId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddHours(1);
    }
}
