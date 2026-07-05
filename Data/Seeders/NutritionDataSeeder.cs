using MedPal.API.Models;
using Microsoft.EntityFrameworkCore;

namespace MedPal.API.Data.Seeders
{
    public static class NutritionDataSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            var nutritionDoctor = await context.Users
                .FirstOrDefaultAsync(u => u.Email == "doctor.nutricion@medpal.com");
            if (nutritionDoctor == null)
            {
                Console.WriteLine("⚠️ NutritionDataSeeder: No se encontró doctor de Nutrición. Saltando.");
                return;
            }

            var nutritionPdIds = await context.MedicalHistories
                .Where(mh => mh.HealthcareProfessionalId == nutritionDoctor.Id)
                .Select(mh => mh.PatientDetailsId)
                .Distinct()
                .ToListAsync();

            if (nutritionPdIds.Count == 0)
            {
                Console.WriteLine("⚠️ NutritionDataSeeder: No hay pacientes de Nutrición. Saltando.");
                return;
            }

            var hasData = await context.BodyCompositions
                .AnyAsync(bc => nutritionPdIds.Contains(bc.PatientDetailsId));
            if (hasData)
            {
                Console.WriteLine("✅ NutritionDataSeeder: ya hay datos de nutrición. Saltando.");
                return;
            }

            var patients = await context.Patients
                .Include(p => p.PatientDetails)
                .Where(p => p.PatientDetails != null && nutritionPdIds.Contains(p.PatientDetails.Id))
                .ToListAsync();

            if (patients.Count == 0) return;

            var foodItems = await context.FoodItems.ToListAsync();
            var today = DateTime.UtcNow;

            var profiles = new[]
            {
                new { BaseWeight = 82.5m, HeightCm = 168m, Trend = "loss", Label = "éxito 1" },
                new { BaseWeight = 78.0m, HeightCm = 165m, Trend = "loss", Label = "éxito 2" },
                new { BaseWeight = 85.3m, HeightCm = 172m, Trend = "loss", Label = "éxito 3" },
                new { BaseWeight = 68.5m, HeightCm = 160m, Trend = "stable", Label = "estable 1" },
                new { BaseWeight = 71.2m, HeightCm = 163m, Trend = "stable", Label = "estable 2" },
                new { BaseWeight = 65.0m, HeightCm = 158m, Trend = "stable", Label = "estable 3" },
                new { BaseWeight = 92.0m, HeightCm = 175m, Trend = "gain", Label = "dificultad 1" },
                new { BaseWeight = 103.5m, HeightCm = 180m, Trend = "gain", Label = "dificultad 2" },
            };

            var rng = new Random(42);

            foreach (var (patient, idx) in patients.Select((p, i) => (p, i)))
            {
                var pdId = patient.PatientDetails?.Id;
                if (pdId == null) continue;
                var pd = pdId.Value;

                var profile = profiles[idx % profiles.Length];
                var baseW = profile.BaseWeight;
                var heightCm = profile.HeightCm;
                var heightM = heightCm / 100m;

                var weights = GenerateWeightTrend(baseW, profile.Trend, 8, rng).ToArray();
                var bfps = GenerateFatTrend(profile.Trend, 8, rng).ToArray();
                var muscles = GenerateMuscleTrend(profile.Trend, 8, rng).ToArray();
                var waists = GenerateWaistTrend(baseW, profile.Trend, 8, rng).ToArray();

                // --- BodyComposition (8 records, ~mensual, últimos 7 meses) ---
                for (int i = 0; i < 8; i++)
                {
                    var daysAgo = 210 - i * 30;
                    var w = weights[i];
                    var bmi = Math.Round(w / ((heightCm / 100m) * (heightCm / 100m)), 1);

                    context.BodyCompositions.Add(new BodyComposition
                    {
                        PatientDetailsId = pd,
                        RecordedAt = today.AddDays(-daysAgo),
                        Weight = w,
                        Height = heightCm,
                        Bmi = bmi,
                        BodyFatPercentage = bfps[i],
                        MuscleMass = muscles[i],
                        BoneMass = 2.7m + (rng.Next(0, 4) * 0.1m),
                        BodyWaterPercentage = 50m - (bfps[i] * 0.3m),
                        VisceralFat = (int)Math.Round(bfps[i] / 4m),
                        Bmr = 1400m + (w * 2m) + rng.Next(-50, 51),
                        MetabolicAge = 38 + rng.Next(-5, 8),
                        Source = "InBody",
                        Notes = i == 0 ? "Medición inicial" :
                                i == 7 ? "Medición actual" :
                                $"Control mensual #{i}",
                        CreatedAt = today
                    });
                }

                // --- NutritionProgress (12 records, biweekly, últimos 6 meses) ---
                var progressWeights = GenerateWeightTrend(baseW, profile.Trend, 12, rng).ToArray();
                var progressBfps = GenerateFatTrend(profile.Trend, 12, rng).ToArray();
                var progressMuscles = GenerateMuscleTrend(profile.Trend, 12, rng).ToArray();
                var progressWaists = GenerateWaistTrend(baseW, profile.Trend, 12, rng).ToArray();
                var observanceFeedback = profile.Trend switch
                {
                    "loss" => new[]
                    {
                        "Inicia plan de reducción de peso con motivación",
                        "Buena adherencia, refiere mayor energía",
                        "Sigue plan al pie de la letra",
                        "Continúa progreso sin contratiempos",
                        "Reporta mejoría en digestión",
                        "Empieza a notar cambios en ropa",
                        "Refiere mayor facilidad para actividades diarias",
                        "Familia nota cambio positivo",
                        "Mantiene rutina de ejercicio",
                        "Muy satisfecho con resultados",
                        "Último control, resultados visibles",
                        "Plan completado exitosamente"
                    },
                    "stable" => new[]
                    {
                        "Inicia plan de mantenimiento",
                        "Adherencia moderada",
                        "Reporta algunos antojos",
                        "Mantiene peso estable",
                        "Tuvo evento social, se salió del plan",
                        "Retoma plan con normalidad",
                        "Refiere estrés laboral",
                        "Adherencia irregular esta semana",
                        "Se mantiene dentro del rango esperado",
                        "Reporta mejora en hábitos",
                        "Continúa con avances graduales",
                        "Estable, seguimiento rutinario"
                    },
                    _ => new[]
                    {
                        "Inicia plan con dudas",
                        "Dificultad para seguir horarios",
                        "Reporta ansiedad por comida",
                        "Inasistencia a consulta programada",
                        "Tuvo recaída el fin de semana",
                        "Dice estar comprometido pero sin cambios",
                        "No ha logrado reducir porciones",
                        "Refiere problemas personales",
                        "Continúa con hábitos previos",
                        "Peso estable, sin pérdida significativa",
                        "Requiere reforzar motivación",
                        "Evaluación: cambiar enfoque de tratamiento"
                    }
                };

                for (int i = 0; i < 12; i++)
                {
                    var w = progressWeights[i];
                    context.NutritionProgresses.Add(new NutritionProgress
                    {
                        PatientDetailsId = pd,
                        RecordedAt = today.AddDays(-168 + i * 14),
                        Weight = w,
                        BodyFatPercentage = progressBfps[i],
                        MuscleMass = progressMuscles[i],
                        Waist = progressWaists[i],
                        CaloriesConsumed = Math.Round(w * 28m, 0),
                        ProteinConsumed = Math.Round(w * 1.8m, 1),
                        CarbsConsumed = Math.Round(w * 3.2m, 0),
                        FatConsumed = Math.Round(w * 0.8m, 1),
                        Adherence = profile.Trend switch
                        {
                            "loss" => 8 + rng.Next(0, 3),
                            "stable" => 5 + rng.Next(0, 4),
                            _ => 3 + rng.Next(0, 3)
                        },
                        Observations = observanceFeedback[i],
                        CreatedAt = today
                    });
                }

                // --- AnthropometryRecord (5 records, ~cada 35 días, con weight/height/bmi) ---
                for (int i = 0; i < 5; i++)
                {
                    var daysAgo = 140 - i * 35;
                    var w = GenerateWeightAtDay(baseW, profile.Trend, 140, daysAgo, rng);
                    var bmi = Math.Round(w / (heightM * heightM), 1);
                    var waistVal = progressWaists[Math.Min(i * 2, progressWaists.Length - 1)];

                    context.AnthropometryRecords.Add(new AnthropometryRecord
                    {
                        PatientDetailsId = pd,
                        RecordedAt = today.AddDays(-daysAgo),
                        Weight = w,
                        Height = heightM,
                        Bmi = bmi,
                        Waist = waistVal,
                        Hip = waistVal + 18m + rng.Next(-2, 3),
                        WaistHipRatio = Math.Round(waistVal / (waistVal + 18m + rng.Next(-2, 3)), 2),
                        Wrist = 16m + (rng.Next(0, 3) * 0.5m),
                        Thigh = 56m + rng.Next(-4, 5) - (i * 0.5m),
                        Calf = 35m + rng.Next(-2, 3),
                        TricepsSkinfold = 22m + rng.Next(-3, 4) - (i * 1.2m),
                        BicepsSkinfold = 18m + rng.Next(-2, 3) - (i * 0.8m),
                        SubscapularSkinfold = 25m + rng.Next(-3, 4) - (i * 1.0m),
                        SuprailiacSkinfold = 28m + rng.Next(-3, 4) - (i * 1.5m),
                        BodyFatPercentageEstimated = bfps[Math.Min(i * 2, bfps.Length - 1)],
                        Notes = i == 0 ? "Antropometría inicial" :
                                i == 4 ? "Antropometría actual" :
                                $"Control antropométrico #{i}",
                        CreatedAt = today
                    });
                }

                // --- DietPlan (1 completado, 1 activo) ---
                var plan1 = new DietPlan
                {
                    PatientDetailsId = pd,
                    Name = profile.Trend switch
                    {
                        "loss" => "Plan de Reducción de Peso - Fase 1",
                        "stable" => "Plan de Mantenimiento",
                        _ => "Plan de Reeducación Alimentaria"
                    },
                    Description = profile.Trend switch
                    {
                        "loss" => "Plan hipocalórico balanceado de 12 semanas con enfoque en déficit calórico moderado y preservación de masa muscular.",
                        "stable" => "Plan isocalórico para mantenimiento de peso con énfasis en calidad nutricional y hábitos sostenibles.",
                        _ => "Plan de reestructuración de hábitos alimentarios con enfoque en reducción gradual de porciones y mejora de calidad nutricional."
                    },
                    DailyCalories = profile.Trend switch { "loss" => 1650, "stable" => 1950, _ => 1800 },
                    ProteinG = profile.Trend switch { "loss" => 130, "stable" => 110, _ => 100 },
                    CarbsG = profile.Trend switch { "loss" => 165, "stable" => 220, _ => 200 },
                    FatG = profile.Trend switch { "loss" => 44, "stable" => 55, _ => 60 },
                    FiberG = 25, WaterMl = 2000,
                    Objective = profile.Trend switch
                    {
                        "loss" => "Reducción de peso corporal preservando masa muscular",
                        "stable" => "Mantenimiento de peso y mejora de hábitos alimentarios",
                        _ => "Reeducación alimentaria y control de porciones"
                    },
                    StartDate = DateOnly.FromDateTime(today.AddDays(-180)),
                    EndDate = DateOnly.FromDateTime(today.AddDays(-90)),
                    Status = "Completed", Specialty = "Nutrición",
                    CreatedAt = today
                };
                context.DietPlans.Add(plan1);
                await context.SaveChangesAsync();

                var mealNames1 = new[] { "Desayuno", "Colación Matutina", "Almuerzo", "Colación Vespertina", "Cena" };
                for (int m = 0; m < mealNames1.Length; m++)
                {
                    var meal = new DietPlanMeal
                    {
                        DietPlanId = plan1.Id, MealOrder = m + 1,
                        MealName = mealNames1[m],
                        TimeOfDay = m switch { 0 => "08:00", 1 => "11:00", 2 => "14:00", 3 => "17:00", _ => "20:00" },
                        CreatedAt = today
                    };
                    context.DietPlanMeals.Add(meal);
                    await context.SaveChangesAsync();

                    AddMealItem(context, meal.Id, foodItems, "Huevo entero", 2, "pieza", today);
                    AddMealItem(context, meal.Id, foodItems, "Pan integral", 2, "rebanada", today);
                    AddMealItem(context, meal.Id, foodItems, "Pollo pechuga sin piel", 150, "g", today);
                    AddMealItem(context, meal.Id, foodItems, "Arroz integral cocido", 100, "g", today);
                }

                var plan2 = new DietPlan
                {
                    PatientDetailsId = pd,
                    Name = profile.Trend switch
                    {
                        "loss" => "Plan de Reducción de Peso - Fase 2",
                        "stable" => "Plan de Optimización Nutricional",
                        _ => "Plan de Intervención Intensiva"
                    },
                    Description = profile.Trend switch
                    {
                        "loss" => "Continuación del plan de reducción con ajuste de macros para evitar meseta metabólica.",
                        "stable" => "Plan enfocado en optimizar calidad nutricional y micro nutrientes.",
                        _ => "Plan estructurado con mayor seguimiento y estrategias de cambio conductual."
                    },
                    DailyCalories = profile.Trend switch { "loss" => 1550, "stable" => 1900, _ => 1750 },
                    ProteinG = profile.Trend switch { "loss" => 140, "stable" => 115, _ => 110 },
                    CarbsG = profile.Trend switch { "loss" => 150, "stable" => 210, _ => 190 },
                    FatG = profile.Trend switch { "loss" => 40, "stable" => 52, _ => 55 },
                    FiberG = 28, WaterMl = 2200,
                    Objective = profile.Trend switch
                    {
                        "loss" => "Continuar pérdida de peso superando meseta metabólica",
                        "stable" => "Optimizar composición corporal y calidad nutricional",
                        _ => "Establecer hábitos sostenibles y lograr adherencia"
                    },
                    StartDate = DateOnly.FromDateTime(today.AddDays(-60)),
                    EndDate = DateOnly.FromDateTime(today.AddDays(60)),
                    Status = "Active", Specialty = "Nutrición",
                    CreatedAt = today
                };
                context.DietPlans.Add(plan2);
                await context.SaveChangesAsync();

                var mealNames2 = new[] { "Desayuno", "Almuerzo", "Cena" };
                for (int m = 0; m < mealNames2.Length; m++)
                {
                    var meal = new DietPlanMeal
                    {
                        DietPlanId = plan2.Id, MealOrder = m + 1,
                        MealName = mealNames2[m],
                        TimeOfDay = m switch { 0 => "08:00", 1 => "14:00", _ => "20:00" },
                        CreatedAt = today
                    };
                    context.DietPlanMeals.Add(meal);
                    await context.SaveChangesAsync();

                    AddMealItem(context, meal.Id, foodItems, "Avena", 40, "g", today);
                    AddMealItem(context, meal.Id, foodItems, "Pescado tilapia", 120, "g", today);
                    AddMealItem(context, meal.Id, foodItems, "Brócoli", 100, "g", today);
                    AddMealItem(context, meal.Id, foodItems, "Aceite de oliva", 10, "ml", today);
                }

                // --- Supplements (2 activos, 1 inactivo) ---
                context.Supplements.AddRange(
                    new Supplement
                    {
                        PatientDetailsId = pd,
                        Name = "Vitamina D3 2000 UI", Brand = "Solaray",
                        Description = "Vitamina D3 para soporte inmunológico y óseo",
                        Form = "Cápsula", Dosage = "1 cápsula", Frequency = "Cada 24 horas",
                        Unit = "cápsula", Timing = "Después del desayuno", Duration = "6 meses",
                        Indication = "Déficit de vitamina D diagnosticado",
                        StartDate = DateOnly.FromDateTime(today.AddDays(-150)),
                        IsActive = true, PrescribedAt = today.AddDays(-150),
                        CreatedAt = today
                    },
                    new Supplement
                    {
                        PatientDetailsId = pd,
                        Name = "Proteína Whey Aislada", Brand = "Optimum Nutrition",
                        Description = "Proteína de suero aislada para soporte de masa muscular",
                        Form = "Polvo", Dosage = "1 scoop (30g)", Frequency = "Post-entreno",
                        Unit = "scoop", Timing = "Después del ejercicio", Duration = "3 meses",
                        Indication = "Asegurar aporte proteico en déficit calórico",
                        StartDate = DateOnly.FromDateTime(today.AddDays(-90)),
                        IsActive = true, PrescribedAt = today.AddDays(-90),
                        CreatedAt = today
                    },
                    new Supplement
                    {
                        PatientDetailsId = pd,
                        Name = "Magnesio Citrato 400mg", Brand = "Now Foods",
                        Description = "Magnesio citrato para relajación muscular y sueño",
                        Form = "Cápsula", Dosage = "1 cápsula", Frequency = "Cada 24 horas",
                        Unit = "cápsula", Timing = "Antes de dormir", Duration = "2 meses",
                        Indication = "Calambres musculares nocturnos",
                        StartDate = DateOnly.FromDateTime(today.AddDays(-120)),
                        EndDate = DateOnly.FromDateTime(today.AddDays(-60)),
                        IsActive = false, PrescribedAt = today.AddDays(-120),
                        CreatedAt = today
                    }
                );
            }

            await context.SaveChangesAsync();
            Console.WriteLine($"✅ NutritionDataSeeder: {patients.Count} pacientes de Nutrición con datos completos (BodyComposition, Progress, Anthropometry, DietPlans, Supplements)");
        }

        private static IEnumerable<decimal> GenerateWeightTrend(decimal baseWeight, string trend, int count, Random rng)
        {
            var results = new List<decimal>();
            for (int i = 0; i < count; i++)
            {
                var p = (double)i / (count - 1);
                var change = trend switch
                {
                    "loss" => -(decimal)(p * 8.0),
                    "gain" => (decimal)(p * 6.0),
                    _ => (decimal)(Math.Sin(p * Math.PI * 2.0) * 1.5)
                };
                var noise = (decimal)(rng.NextDouble() - 0.5) * 0.6m;
                results.Add(Math.Round(baseWeight + change + noise, 1));
            }
            return results;
        }

        private static IEnumerable<decimal> GenerateFatTrend(string trend, int count, Random rng)
        {
            var results = new List<decimal>();
            var baseFat = trend switch { "loss" => 33m, "stable" => 28m, _ => 36m };
            for (int i = 0; i < count; i++)
            {
                var p = (double)i / (count - 1);
                var change = trend switch
                {
                    "loss" => -(decimal)(p * 6.0),
                    "gain" => (decimal)(p * 4.0),
                    _ => (decimal)(Math.Sin(p * Math.PI * 2.0) * 1.5)
                };
                var noise = (decimal)(rng.NextDouble() - 0.5) * 1.0m;
                results.Add(Math.Round(Math.Max(baseFat + change + noise, 18m), 1));
            }
            return results;
        }

        private static IEnumerable<decimal> GenerateMuscleTrend(string trend, int count, Random rng)
        {
            var results = new List<decimal>();
            var baseMuscle = trend switch { "loss" => 41.8m, "stable" => 40.0m, _ => 43.0m };
            for (int i = 0; i < count; i++)
            {
                var p = (double)i / (count - 1);
                var change = trend switch
                {
                    "loss" => (decimal)(p * 2.5),
                    "gain" => -(decimal)(p * 0.5),
                    _ => (decimal)(Math.Sin(p * Math.PI * 2.0) * 0.5)
                };
                var noise = (decimal)(rng.NextDouble() - 0.5) * 0.4m;
                results.Add(Math.Round(baseMuscle + change + noise, 1));
            }
            return results;
        }

        private static IEnumerable<decimal> GenerateWaistTrend(decimal baseWeight, string trend, int count, Random rng)
        {
            var results = new List<decimal>();
            var baseWaist = 60m + baseWeight * 0.35m;
            for (int i = 0; i < count; i++)
            {
                var p = (double)i / (count - 1);
                var change = trend switch
                {
                    "loss" => -(decimal)(p * 14.0),
                    "gain" => (decimal)(p * 8.0),
                    _ => (decimal)(Math.Sin(p * Math.PI * 2.0) * 2.0)
                };
                var noise = (decimal)(rng.NextDouble() - 0.5) * 2.0m;
                results.Add(Math.Round(baseWaist + change + noise, 0));
            }
            return results;
        }

        private static decimal GenerateWeightAtDay(decimal baseWeight, string trend, int totalDays, int daysAgo, Random rng)
        {
            var p = (double)(totalDays - daysAgo) / totalDays;
            var change = trend switch
            {
                "loss" => -(decimal)(p * 8.0),
                "gain" => (decimal)(p * 6.0),
                _ => (decimal)(Math.Sin(Math.Max(p, 0.01) * Math.PI * 2.0) * 1.5)
            };
            var noise = (decimal)(rng.NextDouble() - 0.5) * 0.8m;
            return Math.Round(baseWeight + change + noise, 1);
        }

        private static void AddMealItem(AppDbContext context, int mealId, List<FoodItem> foodItems, string foodName, decimal quantity, string unit, DateTime now)
        {
            var food = foodItems.FirstOrDefault(f => f.Name == foodName);
            context.DietPlanMealItems.Add(new DietPlanMealItem
            {
                DietPlanMealId = mealId,
                FoodItemId = food?.Id,
                CustomFoodName = food == null ? foodName : null,
                Quantity = quantity,
                Unit = unit,
                CreatedAt = now
            });
        }
    }
}
