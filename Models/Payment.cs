using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MedPal.API.Enums;
using MedPal.API.Interfaces;

namespace MedPal.API.Models
{
    public class Payment : ISoftDelete
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Invoice")]
        [Required]
        public int InvoiceId { get; set; }

        [Required]
        public decimal AmountPaid { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }

        // ISoftDelete implementation
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public int? DeletedByUserId { get; set; }

        public virtual Invoice Invoice { get; set; }
    }
}