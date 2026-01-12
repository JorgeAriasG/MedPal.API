using System;
using MedPal.API.Interfaces;

namespace MedPal.API.Models
{
    public class UserClinic : ISoftDelete
    {
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public int ClinicId { get; set; }
        public virtual Clinic Clinic { get; set; }

        // ISoftDelete implementation
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public int? DeletedByUserId { get; set; }
    }
}