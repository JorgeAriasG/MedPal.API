using System;

namespace MedPal.API.DTOs
{
    public class FoodItemReadDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public string Category { get; set; } = string.Empty;
        public string? Subcategory { get; set; }
        public decimal ServingSize { get; set; }
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
        public bool IsActive { get; set; }
        public bool IsSystemItem { get; set; }
        public string? Allergens { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
