// RoleWriteDTO.cs
using System.ComponentModel.DataAnnotations;

namespace MedPal.API.DTOs
{
    public class RoleWriteDTO
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}