using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MedPal.API.Enums;
using MedPal.API.Interfaces;

namespace MedPal.API.Models
{
    public class Invoice : ISoftDelete, IAuditableEntity
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Patient")]
        [Required]
        public int PatientId { get; set; }

        [ForeignKey("Appointment")]
        [Required]
        public int AppointmentId { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        // NUEVO: Suma en caché de pagos para consultas rápidas
        public decimal PaidAmount { get; set; } = 0m;

        // NUEVO: Campo calculado para balance
        [NotMapped]
        public decimal RemainingAmount => TotalAmount - PaidAmount;

        [Required]
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;

        // NUEVO: Fecha de vencimiento
        public DateTime? DueDate { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // ISoftDelete implementation
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public int? DeletedByUserId { get; set; }

        // IAuditableEntity implementation
        public int? CreatedByUserId { get; set; }
        public int? UpdatedByUserId { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public int? LastModifiedByUserId { get; set; }

        public virtual Patient Patient { get; set; }
        public virtual Appointment Appointment { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
    }
}