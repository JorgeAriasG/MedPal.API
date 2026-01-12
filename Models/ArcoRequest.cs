using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MedPal.API.Interfaces;

namespace MedPal.API.Models
{
    public enum ArcoRequestType
    {
        Access,
        Rectification,
        Cancellation,
        Opposition
    }

    public enum ArcoRequestStatus
    {
        Pending,
        Approved,
        Rejected,
        Completed
    }

    public class ArcoRequest : ISoftDelete
    {
        [Key]
        public int Id { get; set; }

        public int? UserId { get; set; }

        public int? PatientId { get; set; }

        [Required]
        public ArcoRequestType RequestType { get; set; }

        [Required]
        public ArcoRequestStatus Status { get; set; } = ArcoRequestStatus.Pending;

        [Required]
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;

        public DateTime? CompletionDate { get; set; }

        public string Details { get; set; }

        public string ResponseNotes { get; set; }

        // ISoftDelete implementation
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public int? DeletedByUserId { get; set; }

        // Navigations
        public virtual User User { get; set; }
        public virtual Patient Patient { get; set; }
    }
}
