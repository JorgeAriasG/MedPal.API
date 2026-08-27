using System;
using MedPal.API.Interfaces;

namespace MedPal.API.Models
{
    /// <summary>
    /// Membership of a patient in an account (M:N).
    /// Tenancy is resolved through this table; the primary membership gates staff access.
    /// Cross-account memberships require patient verification before the account can book appointments.
    /// </summary>
    public class PatientAccount : ISoftDelete, IAuditableEntity
    {
        public int PatientId { get; set; }
        public virtual Patient Patient { get; set; }

        public int AccountId { get; set; }
        public virtual Account Account { get; set; }

        /// <summary>
        /// True when this is the patient's primary account (gates staff access).
        /// </summary>
        public bool IsPrimaryAccount { get; set; } = false;

        /// <summary>
        /// True when the patient confirmed this membership (required before the account can book).
        /// </summary>
        public bool IsVerifiedByPatient { get; set; } = false;

        /// <summary>
        /// Per-account consent from the patient to share their profile with this account.
        /// </summary>
        public bool? ConsentToShareProfile { get; set; }

        // ISoftDelete implementation
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public int? DeletedByUserId { get; set; }

        // IAuditableEntity implementation
        public DateTime CreatedAt { get; set; }
        public int? CreatedByUserId { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedByUserId { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public int? LastModifiedByUserId { get; set; }
    }
}