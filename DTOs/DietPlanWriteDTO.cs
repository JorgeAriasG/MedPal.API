using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedPal.API.DTOs
{
    public class DietPlanWriteDTO
    {
        [Required(ErrorMessage = "El ID del paciente es requerido")]
        public int PatientDetailsId { get; set; }

        [Required(ErrorMessage = "El nombre del plan es requerido")]
        [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        public string Name { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Range(0, 10000)]
        public decimal? DailyCalories { get; set; }

        [Range(0, 1000)]
        public decimal? ProteinG { get; set; }

        [Range(0, 1000)]
        public decimal? CarbsG { get; set; }

        [Range(0, 1000)]
        public decimal? FatG { get; set; }

        [Range(0, 1000)]
        public decimal? FiberG { get; set; }

        [Range(0, 10000)]
        public decimal? WaterMl { get; set; }

        [StringLength(500)]
        public string? Objective { get; set; }

        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }

        [Required(ErrorMessage = "El estado es requerido")]
        [StringLength(20)]
        [RegularExpression("^(Draft|Active|Completed|Cancelled)$", ErrorMessage = "Estado inválido. Use: Draft, Active, Completed o Cancelled")]
        public string Status { get; set; } = "Draft";

        [StringLength(100)]
        public string? Specialty { get; set; }

        public List<DietPlanMealWriteDTO> Meals { get; set; } = new List<DietPlanMealWriteDTO>();
    }

    public class DietPlanMealWriteDTO
    {
        [Required(ErrorMessage = "El nombre de la comida es requerido")]
        [StringLength(100)]
        public string MealName { get; set; } = string.Empty;

        public int MealOrder { get; set; }

        [StringLength(50)]
        public string? TimeOfDay { get; set; }

        [StringLength(1000)]
        public string? Instructions { get; set; }

        public List<DietPlanMealItemWriteDTO> Items { get; set; } = new List<DietPlanMealItemWriteDTO>();
    }

    public class DietPlanMealItemWriteDTO
    {
        public int? FoodItemId { get; set; }

        [StringLength(200)]
        public string? CustomFoodName { get; set; }

        [Range(0, 99999)]
        public decimal Quantity { get; set; } = 1;

        [Required(ErrorMessage = "La unidad es requerida")]
        [StringLength(50)]
        public string Unit { get; set; } = "g";

        [StringLength(500)]
        public string? Notes { get; set; }

        [Range(0, 99999)]
        public decimal? Calories { get; set; }

        [Range(0, 99999)]
        public decimal? Protein { get; set; }

        [Range(0, 99999)]
        public decimal? Carbs { get; set; }

        [Range(0, 99999)]
        public decimal? Fat { get; set; }
    }

    public class DietPlanStatusUpdateDTO
    {
        [Required(ErrorMessage = "El estado es requerido")]
        [StringLength(20)]
        [RegularExpression("^(Draft|Active|Completed|Cancelled)$", ErrorMessage = "Estado inválido. Use: Draft, Active, Completed o Cancelled")]
        public string Status { get; set; } = string.Empty;
    }
}
