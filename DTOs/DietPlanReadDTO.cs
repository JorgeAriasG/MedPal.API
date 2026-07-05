using System;
using System.Collections.Generic;

namespace MedPal.API.DTOs
{
    public class DietPlanReadDTO
    {
        public int Id { get; set; }
        public int PatientDetailsId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal? DailyCalories { get; set; }
        public decimal? ProteinG { get; set; }
        public decimal? CarbsG { get; set; }
        public decimal? FatG { get; set; }
        public decimal? FiberG { get; set; }
        public decimal? WaterMl { get; set; }
        public string? Objective { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string Status { get; set; } = "Draft";
        public string? Specialty { get; set; }
        public int? CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<DietPlanMealDTO> Meals { get; set; } = new List<DietPlanMealDTO>();
    }

    public class DietPlanMealDTO
    {
        public int Id { get; set; }
        public int DietPlanId { get; set; }
        public int MealOrder { get; set; }
        public string MealName { get; set; } = string.Empty;
        public string? TimeOfDay { get; set; }
        public string? Instructions { get; set; }
        public ICollection<DietPlanMealItemDTO> Items { get; set; } = new List<DietPlanMealItemDTO>();
    }

    public class DietPlanMealItemDTO
    {
        public int Id { get; set; }
        public int DietPlanMealId { get; set; }
        public int? FoodItemId { get; set; }
        public string? FoodItemName { get; set; }
        public string? CustomFoodName { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = "g";
        public string? Notes { get; set; }
        public decimal? Calories { get; set; }
        public decimal? Protein { get; set; }
        public decimal? Carbs { get; set; }
        public decimal? Fat { get; set; }
    }
}
