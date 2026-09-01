using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedPal.API.DTOs
{
    public class PatientRegisterDTO
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        public string Name { get; set; }

        public string? Middlename { get; set; }

        [Required(ErrorMessage = "El apellido es requerido")]
        public string Lastname { get; set; }

        [EmailAddress(ErrorMessage = "El email no es válido")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "El número de teléfono es requerido")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        public string Password { get; set; }

        [Required(ErrorMessage = "La confirmación de contraseña es requerida")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; }

        public string? Address { get; set; }

        public DateTime? Dob { get; set; }

        public string? Gender { get; set; }

        public List<int>? ClinicIds { get; set; }
    }

    public class PatientLoginDTO
    {
        [EmailAddress(ErrorMessage = "El email no es válido")]
        public string? Email { get; set; }

        public string? Phone { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        public string Password { get; set; }
    }

    public class PatientLoginResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public string Phone { get; set; }
    }
}
