using System.ComponentModel.DataAnnotations;

namespace MedPal.API.DTOs
{
    public class FoodItemWriteDTO
    {
        [Required(ErrorMessage = "El nombre del alimento es requerido")]
        [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        public string Name { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "La marca no puede exceder 100 caracteres")]
        public string? Brand { get; set; }

        [Required(ErrorMessage = "La categoría es requerida")]
        [StringLength(50, ErrorMessage = "La categoría no puede exceder 50 caracteres")]
        public string Category { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "La subcategoría no puede exceder 100 caracteres")]
        public string? Subcategory { get; set; }

        [Range(0.1, 9999, ErrorMessage = "La porción debe ser mayor a 0")]
        public decimal ServingSize { get; set; } = 100;

        [Required(ErrorMessage = "La unidad de porción es requerida")]
        [StringLength(50)]
        public string ServingUnit { get; set; } = "g";

        [Range(0, 9999, ErrorMessage = "Las calorías deben ser un valor positivo")]
        public decimal Calories { get; set; }

        [Range(0, 9999, ErrorMessage = "La proteína debe ser un valor positivo")]
        public decimal Protein { get; set; }

        [Range(0, 9999, ErrorMessage = "Los carbohidratos deben ser un valor positivo")]
        public decimal Carbs { get; set; }

        [Range(0, 9999, ErrorMessage = "La grasa debe ser un valor positivo")]
        public decimal Fat { get; set; }

        [Range(0, 9999)]
        public decimal? Fiber { get; set; }

        [Range(0, 9999)]
        public decimal? Sodium { get; set; }

        [Range(0, 9999)]
        public decimal? Sugar { get; set; }

        [Range(0, 9999)]
        public decimal? SaturatedFat { get; set; }

        [Range(0, 9999)]
        public decimal? TransFat { get; set; }

        [Range(0, 9999)]
        public decimal? Cholesterol { get; set; }

        [Range(0, 9999)]
        public decimal? Potassium { get; set; }

        [Range(0, 9999)]
        public decimal? VitaminA { get; set; }

        [Range(0, 9999)]
        public decimal? VitaminC { get; set; }

        [Range(0, 9999)]
        public decimal? Calcium { get; set; }

        [Range(0, 9999)]
        public decimal? Iron { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsSystemItem { get; set; }

        [StringLength(500)]
        public string? Allergens { get; set; }
    }
}
