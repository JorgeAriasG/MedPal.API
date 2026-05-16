using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedPal.API.Models
{
    [Table("WaitlistEntries")]
    public class WaitlistEntry
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [Required]
        [StringLength(300)]
        public string Email { get; set; }

        [StringLength(100)]
        public string? Specialty { get; set; }

        [StringLength(200)]
        public string? ClinicName { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
