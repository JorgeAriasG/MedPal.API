using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MedPal.API.Interfaces;

namespace MedPal.API.Models
{
    public class DietPlanMeal : IAuditableEntity, ISoftDelete
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("DietPlan")]
        [Required]
        public int DietPlanId { get; set; }

        public int MealOrder { get; set; }

        [Required]
        [StringLength(100)]
        public string MealName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? TimeOfDay { get; set; }

        [StringLength(1000)]
        public string? Instructions { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? CreatedByUserId { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedByUserId { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public int? LastModifiedByUserId { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? DeletedByUserId { get; set; }

        public virtual DietPlan DietPlan { get; set; }
        public virtual ICollection<DietPlanMealItem> Items { get; set; } = new List<DietPlanMealItem>();
    }
}
