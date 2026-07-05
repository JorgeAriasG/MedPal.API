using System;
using System.ComponentModel.DataAnnotations;
using MedPal.API.Interfaces;

namespace MedPal.API.Models
{
    public class FoodItem : IAuditableEntity, ISoftDelete
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Brand { get; set; }

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Subcategory { get; set; }

        public decimal ServingSize { get; set; } = 100;
        public string ServingUnit { get; set; } = "g";

        public decimal Calories { get; set; }
        public decimal Protein { get; set; }
        public decimal Carbs { get; set; }
        public decimal Fat { get; set; }
        public decimal? Fiber { get; set; }
        public decimal? Sodium { get; set; }
        public decimal? Sugar { get; set; }
        public decimal? SaturatedFat { get; set; }
        public decimal? TransFat { get; set; }
        public decimal? Cholesterol { get; set; }
        public decimal? Potassium { get; set; }
        public decimal? VitaminA { get; set; }
        public decimal? VitaminC { get; set; }
        public decimal? Calcium { get; set; }
        public decimal? Iron { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsSystemItem { get; set; }

        [StringLength(500)]
        public string? Allergens { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? CreatedByUserId { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedByUserId { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public int? LastModifiedByUserId { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? DeletedByUserId { get; set; }
    }
}
