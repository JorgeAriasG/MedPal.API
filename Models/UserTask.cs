using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedPal.API.Models
{
    public class UserTask
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Appointment")]
        [Required]
        public int AppointmentId { get; set; }

        [ForeignKey("Patient")]
        [Required]
        public int PatientId { get; set; }

        [ForeignKey("User")]
        [Required]
        public int UserId { get; set; }

        [Required]
        public string TaskDescription { get; set; }

        [Required]
        public string TaskStatus { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }

        public virtual Appointment Appointment { get; set; }
        public virtual Patient Patient { get; set; }
        public virtual User User { get; set; }
    }
}