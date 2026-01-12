using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MedPal.API.Enums;
using MedPal.API.Interfaces;

namespace MedPal.API.Models
{
	public class Appointment : IAuditableEntity, ISoftDelete
	{
		[Key]
		public int Id { get; set; }

		[ForeignKey("Patient")]
		[Required]
		public int PatientId { get; set; }

		[ForeignKey("User")]
		[Required]
		public int UserId { get; set; }

		[ForeignKey("Clinic")]
		public int? ClinicId { get; set; }

		[Required]
		public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

		public string Notes { get; set; }

		[Required]
		public DateOnly Date { get; set; }

		[Required]
		public TimeOnly Time { get; set; }

		[Required]
		public DateTime CreatedAt { get; set; }

		public DateTime? UpdatedAt { get; set; }

		// IAuditableEntity implementation
		public int? CreatedByUserId { get; set; }
		public int? UpdatedByUserId { get; set; }
		public DateTime? LastModifiedAt { get; set; }
		public int? LastModifiedByUserId { get; set; }

		// ISoftDelete implementation
		public bool IsDeleted { get; set; } = false;
		public DateTime? DeletedAt { get; set; }
		public int? DeletedByUserId { get; set; }

		public virtual Patient Patient { get; set; }
		public virtual User User { get; set; }
		public virtual Clinic Clinic { get; set; }
		public virtual ICollection<Invoice> Invoices { get; set; }
	}
}