using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedPal.API.Models
{
	public class Appointment
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
		public string Status { get; set; }

		public string Notes { get; set; }

		[Required]
		public DateOnly Date { get; set; }

		[Required]
		public TimeOnly Time { get; set; }

		[Required]
		public DateTime CreatedAt { get; set; }

		[Required]
		public DateTime UpdatedAt { get; set; }

		public virtual Patient Patient { get; set; }
		public virtual User User { get; set; }
		public virtual Clinic Clinic { get; set; }
		public virtual ICollection<Invoice> Invoices { get; set; }
	}
}