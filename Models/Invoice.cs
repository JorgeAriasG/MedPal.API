using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedPal.API.Models
{
    public class Invoice
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

        [Required]
        public string Status { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }

        public virtual Patient Patient { get; set; }
        public virtual Appointment Appointment { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
    }
}