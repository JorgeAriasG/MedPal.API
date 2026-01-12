using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MedPal.API.Interfaces;

namespace MedPal.API.Models
{
    public class Settings : ISoftDelete
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        [Required]
        public int UserId { get; set; }

        [Required]
        public string Theme { get; set; }

        [Required]
        public bool NotificationsEnabled { get; set; }

        [Required]
        public string Language { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }

        // ISoftDelete implementation
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public int? DeletedByUserId { get; set; }

        public virtual User User { get; set; }
    }
}