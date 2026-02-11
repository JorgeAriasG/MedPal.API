using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MedPal.API.Interfaces;

namespace MedPal.API.Models
{
    /// <summary>
    /// Modelo para gestionar consentimientos de acceso a registros médicos.
    /// Un paciente debe dar consentimiento explícito antes de que otra clínica 
    /// pueda acceder a sus registros médicos (NOM-004 Mexican standard).
    /// </summary>
    public class PatientConsent : ISoftDelete, IAuditableEntity
    {
        /// <summary>
        /// Identificador único del consentimiento.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// FK a PatientDetails - el paciente que da el consentimiento.
        /// </summary>
        [ForeignKey("PatientDetails")]
        [Required]
        public int PatientDetailsId { get; set; }

        /// <summary>
        /// FK a Clinic - la clínica que solicita acceso a los registros.
        /// </summary>
        [ForeignKey(nameof(RequestingClinic))]
        [Required]
        public int RequestingClinicId { get; set; }

        /// <summary>
        /// FK a Clinic - la clínica propietaria de los registros médicos.
        /// </summary>
        [ForeignKey(nameof(OwnerClinic))]
        [Required]
        public int OwnerClinicId { get; set; }

        /// <summary>
        /// Alcance del consentimiento. Ej: "AllRecords", "SpecificDateRange", "LabsOnly"
        /// </summary>
        [StringLength(100)]
        [Required]
        public string ConsentScope { get; set; } = "AllRecords";

        /// <summary>
        /// Indica si el consentimiento ha sido aprobado por el paciente.
        /// </summary>
        [Required]
        public bool IsApproved { get; set; } = false;

        /// <summary>
        /// Fecha en que se otorgó el consentimiento.
        /// </summary>
        [Required]
        public DateTime ConsentDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha en que expira el consentimiento (nullable - puede ser indefinido).
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// FK a User - el usuario que aprobó el consentimiento (típicamente el paciente o su representante).
        /// </summary>
        [ForeignKey(nameof(ApprovedByUser))]
        public int? ApprovedByUserId { get; set; }

        /// <summary>
        /// Notas adicionales sobre el consentimiento.
        /// </summary>
        public string? Notes { get; set; }

        // ISoftDelete implementation
        /// <summary>
        /// Indica si el registro ha sido eliminado lógicamente.
        /// </summary>
        [Required]
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Fecha en que se eliminó lógicamente el registro.
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// FK - Usuario que eliminó el registro.
        /// </summary>
        public int? DeletedByUserId { get; set; }

        // IAuditableEntity implementation
        /// <summary>
        /// Fecha de creación del registro.
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// FK - Usuario que creó el registro.
        /// </summary>
        public int? CreatedByUserId { get; set; }

        /// <summary>
        /// Fecha de última actualización.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// FK - Usuario que realizó la última actualización.
        /// </summary>
        public int? UpdatedByUserId { get; set; }

        /// <summary>
        /// Fecha de última modificación (similar a UpdatedAt pero más específico para auditoría).
        /// </summary>
        public DateTime? LastModifiedAt { get; set; }

        /// <summary>
        /// FK - Usuario que realizó la última modificación.
        /// </summary>
        public int? LastModifiedByUserId { get; set; }

        // Navigation properties
        /// <summary>
        /// Navegación a PatientDetails - el paciente que da el consentimiento.
        /// </summary>
        public virtual PatientDetails? PatientDetails { get; set; }

        /// <summary>
        /// Navegación a Clinic - la clínica que solicita acceso.
        /// </summary>
        public virtual Clinic? RequestingClinic { get; set; }

        /// <summary>
        /// Navegación a Clinic - la clínica propietaria de los registros.
        /// </summary>
        public virtual Clinic? OwnerClinic { get; set; }

        /// <summary>
        /// Navegación a User - el usuario que aprobó el consentimiento.
        /// </summary>
        public virtual User? ApprovedByUser { get; set; }
    }
}
