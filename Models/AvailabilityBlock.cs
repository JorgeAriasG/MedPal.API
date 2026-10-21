using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MedPal.API.Interfaces;

namespace MedPal.API.Models
{
	public class AvailabilityBlock : IAuditableEntity, ISoftDelete
	{
		[Key]
		public int Id { get; set; }

		[ForeignKey("Clinic")]
		[Required]
		public int ClinicId { get; set; }

		[ForeignKey("Doctor")]
		public int? DoctorId { get; set; }

		[Required]
		public DateOnly Date { get; set; }

		[Required]
		public TimeOnly StartTime { get; set; }

		[Required]
		public TimeOnly EndTime { get; set; }

		public string? Reason { get; set; }

		/// <summary>
		/// Fecha en que se creó el bloqueo.
		/// </summary>
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

		/// <summary>
		/// Clínica a la que pertenece el bloqueo (requerido).
		/// </summary>
		public virtual Clinic Clinic { get; set; }

		/// <summary>
		/// Doctor al que aplica el bloqueo. Null = bloqueo de toda la clínica.
		/// </summary>
		public virtual User Doctor { get; set; }
	}
}