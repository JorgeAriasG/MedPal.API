using System.ComponentModel.DataAnnotations;

namespace MedPal.API.Models
{
    public class Cie10Code
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string Code { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(100)]
        public string? Category { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
