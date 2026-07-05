using MedPal.API.Models;
using Microsoft.EntityFrameworkCore;

namespace MedPal.API.Data.Seeders
{
    public static class FoodCatalogSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (await context.FoodItems.AnyAsync())
                return;

            var foods = new List<FoodItem>
            {
                // === Meat & Poultry ===
                new() { Name = "Pollo pechuga sin piel", Category = "meat-poultry", ServingSize = 100, ServingUnit = "g", Calories = 165, Protein = 31, Carbs = 0, Fat = 3.6m, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Pollo muslo", Category = "meat-poultry", ServingSize = 100, ServingUnit = "g", Calories = 209, Protein = 26, Carbs = 0, Fat = 11, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Res bistec", Category = "meat-poultry", ServingSize = 100, ServingUnit = "g", Calories = 250, Protein = 26, Carbs = 0, Fat = 16, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Res molida 80/20", Category = "meat-poultry", ServingSize = 100, ServingUnit = "g", Calories = 254, Protein = 26, Carbs = 0, Fat = 17, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Cerdo lomo", Category = "meat-poultry", ServingSize = 100, ServingUnit = "g", Calories = 242, Protein = 27, Carbs = 0, Fat = 14, IsActive = true, CreatedAt = DateTime.UtcNow },

                // === Fish & Seafood ===
                new() { Name = "Pescado tilapia", Category = "fish-seafood", ServingSize = 100, ServingUnit = "g", Calories = 96, Protein = 20, Carbs = 0, Fat = 1.7m, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Salmón", Category = "fish-seafood", ServingSize = 100, ServingUnit = "g", Calories = 208, Protein = 20, Carbs = 0, Fat = 13, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Atún en agua", Category = "fish-seafood", ServingSize = 100, ServingUnit = "g", Calories = 116, Protein = 26, Carbs = 0, Fat = 0.8m, IsActive = true, CreatedAt = DateTime.UtcNow },

                // === Eggs ===
                new() { Name = "Huevo entero", Category = "eggs", ServingSize = 1, ServingUnit = "pieza", Calories = 72, Protein = 6.3m, Carbs = 0.4m, Fat = 4.8m, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Clara de huevo", Category = "eggs", ServingSize = 1, ServingUnit = "clara", Calories = 17, Protein = 3.6m, Carbs = 0.2m, Fat = 0, IsActive = true, CreatedAt = DateTime.UtcNow },

                // === Dairy ===
                new() { Name = "Leche entera", Category = "dairy", ServingSize = 250, ServingUnit = "ml", Calories = 152, Protein = 8.1m, Carbs = 12, Fat = 8.2m, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Leche descremada", Category = "dairy", ServingSize = 250, ServingUnit = "ml", Calories = 86, Protein = 8.4m, Carbs = 12, Fat = 0.3m, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Yogurt griego natural", Category = "dairy", ServingSize = 200, ServingUnit = "g", Calories = 146, Protein = 20, Carbs = 7.9m, Fat = 3.8m, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Queso cottage", Category = "dairy", ServingSize = 100, ServingUnit = "g", Calories = 98, Protein = 11, Carbs = 3.4m, Fat = 4.3m, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Queso panela", Category = "dairy", ServingSize = 30, ServingUnit = "g", Calories = 81, Protein = 5.5m, Carbs = 1.5m, Fat = 5.8m, IsActive = true, CreatedAt = DateTime.UtcNow },

                // === Cereals & Grains ===
                new() { Name = "Arroz blanco cocido", Category = "cereals-grains", ServingSize = 100, ServingUnit = "g", Calories = 130, Protein = 2.7m, Carbs = 28, Fat = 0.3m, Fiber = 0.4m, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Arroz integral cocido", Category = "cereals-grains", ServingSize = 100, ServingUnit = "g", Calories = 111, Protein = 2.6m, Carbs = 23, Fat = 0.9m, Fiber = 1.8m, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Avena cocida", Category = "cereals-grains", ServingSize = 100, ServingUnit = "g", Calories = 71, Protein = 2.5m, Carbs = 12, Fat = 1.5m, Fiber = 1.7m, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Pan integral", Category = "cereals-grains", ServingSize = 1, ServingUnit = "rebanada", Calories = 85, Protein = 4, Carbs = 15, Fat = 0.8m, Fiber = 2, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Pasta cocida", Category = "cereals-grains", ServingSize = 100, ServingUnit = "g", Calories = 131, Protein = 5, Carbs = 25, Fat = 1.1m, Fiber = 1.8m, IsActive = true, CreatedAt = DateTime.UtcNow },

                // === Legumes ===
                new() { Name = "Frijoles negros cocidos", Category = "legumes", ServingSize = 100, ServingUnit = "g", Calories = 132, Protein = 8.7m, Carbs = 24, Fat = 0.5m, Fiber = 8.7m, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Lentejas cocidas", Category = "legumes", ServingSize = 100, ServingUnit = "g", Calories = 116, Protein = 9, Carbs = 20, Fat = 0.4m, Fiber = 7.9m, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Garbanzo cocido", Category = "legumes", ServingSize = 100, ServingUnit = "g", Calories = 139, Protein = 7.6m, Carbs = 22, Fat = 2.6m, Fiber = 7.6m, IsActive = true, CreatedAt = DateTime.UtcNow },

                // === Vegetables ===
                new() { Name = "Brócoli", Category = "vegetables", ServingSize = 100, ServingUnit = "g", Calories = 34, Protein = 2.8m, Carbs = 7, Fat = 0.4m, Fiber = 2.6m, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Espinaca", Category = "vegetables", ServingSize = 100, ServingUnit = "g", Calories = 23, Protein = 2.9m, Carbs = 3.6m, Fat = 0.4m, Fiber = 2.2m, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Zanahoria", Category = "vegetables", ServingSize = 100, ServingUnit = "g", Calories = 41, Protein = 0.9m, Carbs = 10, Fat = 0.2m, Fiber = 2.8m, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Jitomate", Category = "vegetables", ServingSize = 100, ServingUnit = "g", Calories = 18, Protein = 0.9m, Carbs = 3.9m, Fat = 0.2m, Fiber = 1.2m, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Cebolla", Category = "vegetables", ServingSize = 100, ServingUnit = "g", Calories = 40, Protein = 1.1m, Carbs = 9.3m, Fat = 0.1m, Fiber = 1.7m, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Aguacate", Category = "vegetables", ServingSize = 100, ServingUnit = "g", Calories = 160, Protein = 2, Carbs = 8.5m, Fat = 14.7m, Fiber = 6.7m, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Papa cocida", Category = "vegetables", ServingSize = 100, ServingUnit = "g", Calories = 87, Protein = 2, Carbs = 20, Fat = 0.1m, Fiber = 1.8m, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Camote cocido", Category = "vegetables", ServingSize = 100, ServingUnit = "g", Calories = 90, Protein = 2, Carbs = 21, Fat = 0.1m, Fiber = 3.3m, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Pepino", Category = "vegetables", ServingSize = 100, ServingUnit = "g", Calories = 15, Protein = 0.7m, Carbs = 3.6m, Fat = 0.1m, Fiber = 0.5m, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Calabacita", Category = "vegetables", ServingSize = 100, ServingUnit = "g", Calories = 17, Protein = 1.2m, Carbs = 3.1m, Fat = 0.3m, Fiber = 1m, IsActive = true, CreatedAt = DateTime.UtcNow },

                // === Fruits ===
                new() { Name = "Plátano", Category = "fruits", ServingSize = 1, ServingUnit = "pieza", Calories = 105, Protein = 1.3m, Carbs = 27, Fat = 0.4m, Fiber = 3.1m, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Manzana", Category = "fruits", ServingSize = 1, ServingUnit = "pieza", Calories = 95, Protein = 0.5m, Carbs = 25, Fat = 0.3m, Fiber = 4.4m, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Naranja", Category = "fruits", ServingSize = 1, ServingUnit = "pieza", Calories = 62, Protein = 1.2m, Carbs = 15, Fat = 0.2m, Fiber = 3.1m, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Fresas", Category = "fruits", ServingSize = 100, ServingUnit = "g", Calories = 32, Protein = 0.7m, Carbs = 7.7m, Fat = 0.3m, Fiber = 2, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Arándanos", Category = "fruits", ServingSize = 100, ServingUnit = "g", Calories = 57, Protein = 0.7m, Carbs = 14, Fat = 0.3m, Fiber = 2.4m, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Mango", Category = "fruits", ServingSize = 100, ServingUnit = "g", Calories = 60, Protein = 0.8m, Carbs = 15, Fat = 0.4m, Fiber = 1.6m, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Uvas", Category = "fruits", ServingSize = 100, ServingUnit = "g", Calories = 69, Protein = 0.7m, Carbs = 18, Fat = 0.2m, Fiber = 0.9m, IsActive = true, CreatedAt = DateTime.UtcNow },

                // === Nuts & Seeds ===
                new() { Name = "Almendras", Category = "nuts-seeds", ServingSize = 30, ServingUnit = "g", Calories = 164, Protein = 6, Carbs = 6.1m, Fat = 14, Fiber = 3.5m, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Nuez", Category = "nuts-seeds", ServingSize = 30, ServingUnit = "g", Calories = 185, Protein = 4.3m, Carbs = 3.9m, Fat = 18, Fiber = 1.9m, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Cacahuate", Category = "nuts-seeds", ServingSize = 30, ServingUnit = "g", Calories = 166, Protein = 7.6m, Carbs = 4.5m, Fat = 14, Fiber = 2.4m, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Chía semillas", Category = "nuts-seeds", ServingSize = 15, ServingUnit = "g", Calories = 73, Protein = 2.5m, Carbs = 6.3m, Fat = 4.5m, Fiber = 5.2m, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Mantequilla de cacahuate", Category = "nuts-seeds", ServingSize = 32, ServingUnit = "g", Calories = 188, Protein = 8, Carbs = 6.3m, Fat = 16, Fiber = 1.9m, IsActive = true, CreatedAt = DateTime.UtcNow },

                // === Oils & Fats ===
                new() { Name = "Aceite de oliva", Category = "oils-fats", ServingSize = 15, ServingUnit = "ml", Calories = 119, Protein = 0, Carbs = 0, Fat = 13.5m, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Aceite de coco", Category = "oils-fats", ServingSize = 15, ServingUnit = "ml", Calories = 117, Protein = 0, Carbs = 0, Fat = 13.6m, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Mantequilla", Category = "oils-fats", ServingSize = 10, ServingUnit = "g", Calories = 72, Protein = 0.1m, Carbs = 0.1m, Fat = 8.1m, IsActive = true, CreatedAt = DateTime.UtcNow },

                // === Sugars & Sweets ===
                new() { Name = "Miel", Category = "sugars-sweets", ServingSize = 21, ServingUnit = "g", Calories = 64, Protein = 0.1m, Carbs = 17, Fat = 0, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Chocolate amargo 70%", Category = "sugars-sweets", ServingSize = 30, ServingUnit = "g", Calories = 170, Protein = 2.2m, Carbs = 13, Fat = 12, Fiber = 3, IsActive = true, CreatedAt = DateTime.UtcNow },

                // === Beverages ===
                new() { Name = "Agua natural", Category = "beverages", ServingSize = 250, ServingUnit = "ml", Calories = 0, Protein = 0, Carbs = 0, Fat = 0, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Café negro", Category = "beverages", ServingSize = 240, ServingUnit = "ml", Calories = 2, Protein = 0.3m, Carbs = 0, Fat = 0, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Té verde", Category = "beverages", ServingSize = 240, ServingUnit = "ml", Calories = 2, Protein = 0, Carbs = 0.5m, Fat = 0, IsActive = true, CreatedAt = DateTime.UtcNow },
            };

            context.FoodItems.AddRange(foods);
            await context.SaveChangesAsync();
            Console.WriteLine($"Seeded {foods.Count} food items.");
        }
    }
}
