using System;
using System.Collections.Generic;

namespace MedPal.API.DTOs
{
    /// <summary>
    /// DTO para retornar el perfil del usuario autenticado con información completa
    /// incluyendo especialidad, tipo de usuario y permisos.
    /// </summary>
    public class UserProfileDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Specialty { get; set; }
        public string ProfessionalLicenseNumber { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastAccessAt { get; set; }
        public bool HasAcceptedPrivacyTerms { get; set; }
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Lista de roles asignados al usuario
        /// </summary>
        public IEnumerable<string> Roles { get; set; } = new List<string>();

        /// <summary>
        /// Lista de clínicas asociadas al usuario
        /// </summary>
        public IEnumerable<ClinicBasicDTO> Clinics { get; set; } = new List<ClinicBasicDTO>();
    }

    /// <summary>
    /// DTO básico para clínicas en el perfil del usuario
    /// </summary>
    public class ClinicBasicDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
