using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedPal.API.Models
{
    /// <summary>
    /// Modelo para auditoría de accesos a registros médicos.
    /// Registra TODOS los accesos a registros médicos para cumplir con 
    /// requisitos de auditoría (NOM-004 Mexican standard).
    /// 
    /// IMPORTANTE: Este modelo NO implementa ISoftDelete - los registros 
    /// de acceso NUNCA deben ser borrados (requerimiento de cumplimiento).
    /// </summary>
    public class MedicalRecordAccessLog
    {
        /// <summary>
        /// Identificador único del registro de acceso.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// FK a User - el usuario que está accediendo al registro.
        /// </summary>
        [ForeignKey(nameof(User))]
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// FK a MedicalHistory - el registro médico siendo accedido.
        /// </summary>
        [ForeignKey(nameof(MedicalHistory))]
        public int? MedicalHistoryId { get; set; }

        /// <summary>
        /// FK a PatientDetails - el paciente cuyos registros se están accediendo.
        /// </summary>
        [ForeignKey(nameof(PatientDetails))]
        [Required]
        public int PatientDetailsId { get; set; }

        /// <summary>
        /// Fecha y hora en que ocurrió el acceso.
        /// </summary>
        [Required]
        public DateTime AccessTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Propósito del acceso: "Treatment", "Audit", "Administration", "Research", "Other"
        /// </summary>
        [StringLength(50)]
        [Required]
        public string Purpose { get; set; } = "Treatment";

        /// <summary>
        /// FK a Clinic - la clínica desde donde se accede al registro.
        /// </summary>
        [ForeignKey(nameof(AccessingClinic))]
        [Required]
        public int AccessingClinicId { get; set; }

        /// <summary>
        /// FK a Clinic - la clínica propietaria del registro médico.
        /// </summary>
        [ForeignKey(nameof(OwnerClinic))]
        [Required]
        public int MedicalRecordOwnerClinicId { get; set; }

        /// <summary>
        /// Indica si el usuario tenía consentimiento válido al momento del acceso.
        /// Crítico para cumplimiento regulatorio.
        /// </summary>
        [Required]
        public bool HadValidConsent { get; set; } = false;

        /// <summary>
        /// Razón/Justificación adicional para el acceso (ej: "Emergency", "Scheduled appointment").
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// Dirección IP desde donde se realizó el acceso (para auditoría de seguridad).
        /// </summary>
        [StringLength(45)]
        public string? IpAddress { get; set; }

        /// <summary>
        /// Identificador de sesión o token (para trazabilidad de la sesión).
        /// </summary>
        [StringLength(500)]
        public string? SessionId { get; set; }

        // Navigation properties

        /// <summary>
        /// Navegación a User - el usuario que accedió.
        /// </summary>
        public virtual User? User { get; set; }

        /// <summary>
        /// Navegación a MedicalHistory - el registro médico accedido.
        /// </summary>
        public virtual MedicalHistory? MedicalHistory { get; set; }

        /// <summary>
        /// Navegación a PatientDetails - el paciente cuyos datos fueron accedidos.
        /// </summary>
        public virtual PatientDetails? PatientDetails { get; set; }

        /// <summary>
        /// Navegación a Clinic - la clínica desde donde se realizó el acceso.
        /// </summary>
        public virtual Clinic? AccessingClinic { get; set; }

        /// <summary>
        /// Navegación a Clinic - la clínica propietaria del registro.
        /// </summary>
        public virtual Clinic? OwnerClinic { get; set; }
    }
}
