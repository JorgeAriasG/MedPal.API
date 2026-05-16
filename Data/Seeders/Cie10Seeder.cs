using System.Text.Json;
using MedPal.API.Models;
using Microsoft.EntityFrameworkCore;

namespace MedPal.API.Data.Seeders
{
    public static class Cie10Seeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (await context.Cie10Codes.AnyAsync())
                return;

            var path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Data", "SeedData", "cie10_codes.json");
            var fullPath = Path.GetFullPath(path);

            if (!File.Exists(fullPath))
            {
                Console.WriteLine($"CIE-10 seed file not found at: {fullPath}");
                return;
            }

            var json = await File.ReadAllTextAsync(fullPath);
            var codes = JsonSerializer.Deserialize<List<Cie10Code>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (codes == null || codes.Count == 0)
            {
                Console.WriteLine("No CIE-10 codes found in seed file.");
                return;
            }

            foreach (var code in codes)
            {
                code.IsActive = true;
            }

            context.Cie10Codes.AddRange(codes);
            await context.SaveChangesAsync();
            Console.WriteLine($"Seeded {codes.Count} CIE-10 codes.");
        }
    }
}
