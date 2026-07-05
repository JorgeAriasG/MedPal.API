using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MedPal.API.Interfaces;

namespace MedPal.API.Models
{
    public class DietPlanMealItem : IAuditableEntity, ISoftDelete
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("DietPlanMeal")]
        [Required]
        public int DietPlanMealId { get; set; }

        [ForeignKey("FoodItem")]
        public int? FoodItemId { get; set; }

        [StringLength(200)]
        public string? CustomFoodName { get; set; }

        public decimal Quantity { get; set; } = 1;

        [StringLength(50)]
        public string Unit { get; set; } = "g";

        [StringLength(500)]
        public string? Notes { get; set; }

        public decimal? Calories { get; set; }
        public decimal? Protein { get; set; }
        public decimal? Carbs { get; set; }
        public decimal? Fat { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? CreatedByUserId { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedByUserId { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public int? LastModifiedByUserId { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? DeletedByUserId { get; set; }

        public virtual DietPlanMeal DietPlanMeal { get; set; }
        public virtual FoodItem? FoodItem { get; set; }
    }
}
