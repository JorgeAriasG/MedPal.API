using System.ComponentModel.DataAnnotations;

namespace MedPal.API.DTOs
{
    public class UserWriteDTO : UserRegisterDTO
    {
        [Required]
        public int ClinicId { get; set; }
        [Required]
        public int RoleId { get; set; }  // Fase 2: Nombre del rol a asignar (Doctor, Recepcionista, etc.)
        [Required]
        public int AccountId { get; set; }
    }
}