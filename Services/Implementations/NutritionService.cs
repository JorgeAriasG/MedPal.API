using MedPal.API.DTOs;
using MedPal.API.Repositories;
using Microsoft.EntityFrameworkCore;
using MedPal.API.Data;

namespace MedPal.API.Services.Implementations
{
    public class NutritionService : INutritionService
    {
        private readonly AppDbContext _context;
        private readonly IBodyCompositionRepository _bodyCompositionRepository;
        private readonly IAnthropometryRepository _anthropometryRepository;
        private readonly ILogger<NutritionService> _logger;

        public NutritionService(
            AppDbContext context,
            IBodyCompositionRepository bodyCompositionRepository,
            IAnthropometryRepository anthropometryRepository,
            ILogger<NutritionService> logger)
        {
            _context = context;
            _bodyCompositionRepository = bodyCompositionRepository;
            _anthropometryRepository = anthropometryRepository;
            _logger = logger;
        }

        public async Task<NutritionAssessmentDTO> CalculateAssessmentAsync(int patientDetailsId)
        {
            var patientDetails = await _context.PatientDetails
                .Include(pd => pd.Patient)
                .FirstOrDefaultAsync(pd => pd.Id == patientDetailsId && !pd.IsDeleted);

            if (patientDetails == null)
                return new NutritionAssessmentDTO { PatientDetailsId = patientDetailsId };

            var patient = patientDetails.Patient;
            var latestBodyComp = await _bodyCompositionRepository.GetLatestAsync(patientDetailsId);
            var latestAnthropometry = (await _anthropometryRepository.GetByPatientDetailsIdAsync(patientDetailsId)).FirstOrDefault();

            var result = new NutritionAssessmentDTO
            {
                PatientDetailsId = patientDetailsId,
                AssessmentDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                Bmi = latestBodyComp?.Bmi,
                BodyFatPercentage = latestBodyComp?.BodyFatPercentage,
                Bmr = latestBodyComp?.Bmr,
                MetabolicAge = latestBodyComp?.MetabolicAge,
                WaistHipRatio = latestAnthropometry?.WaistHipRatio ?? latestBodyComp?.WaistHipRatio
            };

            if (result.Bmi.HasValue)
                result.BmiClassification = ClassifyBmi(result.Bmi.Value);

            if (result.BodyFatPercentage.HasValue)
                result.BodyFatClassification = ClassifyBodyFat(result.BodyFatPercentage.Value, patient.Gender);

            if (result.WaistHipRatio.HasValue)
                result.WhrClassification = ClassifyWhr(result.WaistHipRatio.Value, patient.Gender);

            var weight = latestBodyComp?.Weight;
            if (weight.HasValue && weight.Value > 0)
            {
                result.Bmr ??= CalculateMifflinStJeor(weight.Value, latestBodyComp?.Height, patient.Gender, patient.Dob);
                result.EstimatedDailyCalories = CalculateEstimatedCalories(result.Bmr, "sedentary");
                result.RecommendedProteinG = CalculateRecommendedProtein(weight.Value);
                result.RecommendedCarbsG = result.EstimatedDailyCalories.HasValue
                    ? Math.Round(result.EstimatedDailyCalories.Value * 0.5m / 4, 0)
                    : null;
                result.RecommendedFatG = result.EstimatedDailyCalories.HasValue
                    ? Math.Round(result.EstimatedDailyCalories.Value * 0.25m / 9, 0)
                    : null;
            }

            return result;
        }

        public async Task<InBodySyncResultDTO> SyncInBodyDataAsync(InBodySyncDTO syncDto)
        {
            try
            {
                var recordedAt = syncDto.RecordedAt ?? DateTime.UtcNow;

                var bodyComp = new Models.BodyComposition
                {
                    PatientDetailsId = syncDto.PatientDetailsId,
                    RecordedAt = recordedAt,
                    BwImported = true,
                    InBodyRawData = syncDto.RawData,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var created = await _bodyCompositionRepository.AddAsync(bodyComp);
                await _bodyCompositionRepository.CompleteAsync();

                _logger.LogInformation("InBody data synced for PatientDetailsId={PatientDetailsId}, BodyCompositionId={Id}",
                    syncDto.PatientDetailsId, created.Id);

                return new InBodySyncResultDTO
                {
                    Success = true,
                    BodyCompositionId = created.Id,
                    Message = "Datos de InBody sincronizados correctamente",
                    SyncedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "InBody sync failed for PatientDetailsId={PatientDetailsId}", syncDto.PatientDetailsId);
                return new InBodySyncResultDTO
                {
                    Success = false,
                    Message = $"Error al sincronizar datos de InBody: {ex.Message}",
                    SyncedAt = DateTime.UtcNow
                };
            }
        }

        private static string ClassifyBmi(decimal bmi)
        {
            if (bmi < 18.5m) return "Bajo peso";
            if (bmi < 25) return "Normal";
            if (bmi < 30) return "Sobrepeso";
            if (bmi < 35) return "Obesidad Grado I";
            if (bmi < 40) return "Obesidad Grado II";
            return "Obesidad Grado III";
        }

        private static string ClassifyBodyFat(decimal bodyFat, string gender)
        {
            var isMale = gender.Equals("Masculino", StringComparison.OrdinalIgnoreCase) ||
                         gender.Equals("M", StringComparison.OrdinalIgnoreCase) ||
                         gender.Equals("Male", StringComparison.OrdinalIgnoreCase);
            if (isMale)
            {
                if (bodyFat < 6) return "Esencial";
                if (bodyFat < 14) return "Atlético";
                if (bodyFat < 18) return "Saludable";
                if (bodyFat < 25) return "Promedio";
                return "Obesidad";
            }
            else
            {
                if (bodyFat < 14) return "Esencial";
                if (bodyFat < 21) return "Atlético";
                if (bodyFat < 25) return "Saludable";
                if (bodyFat < 32) return "Promedio";
                return "Obesidad";
            }
        }

        private static string ClassifyWhr(decimal whr, string gender)
        {
            var isMale = gender.Equals("Masculino", StringComparison.OrdinalIgnoreCase) ||
                         gender.Equals("M", StringComparison.OrdinalIgnoreCase) ||
                         gender.Equals("Male", StringComparison.OrdinalIgnoreCase);
            if (isMale)
                return whr <= 0.90m ? "Bajo riesgo" : "Riesgo alto";
            return whr <= 0.85m ? "Bajo riesgo" : "Riesgo alto";
        }

        private static decimal? CalculateMifflinStJeor(decimal weightKg, decimal? heightCm, string gender, DateTime dateOfBirth)
        {
            if (!heightCm.HasValue || heightCm.Value <= 0)
                return null;

            var age = DateTime.UtcNow.Year - dateOfBirth.Year;
            var isMale = gender.Equals("Masculino", StringComparison.OrdinalIgnoreCase) ||
                         gender.Equals("M", StringComparison.OrdinalIgnoreCase) ||
                         gender.Equals("Male", StringComparison.OrdinalIgnoreCase);

            decimal bmr;
            if (isMale)
                bmr = 10 * weightKg + 6.25m * heightCm.Value - 5 * age + 5;
            else
                bmr = 10 * weightKg + 6.25m * heightCm.Value - 5 * age - 161;

            return Math.Round(bmr, 0);
        }

        private static decimal? CalculateEstimatedCalories(decimal? bmr, string activityLevel)
        {
            if (!bmr.HasValue) return null;

            var factor = activityLevel switch
            {
                "sedentary" => 1.2m,
                "light" => 1.375m,
                "moderate" => 1.55m,
                "active" => 1.725m,
                "very_active" => 1.9m,
                _ => 1.2m
            };

            return Math.Round(bmr.Value * factor, 0);
        }

        private static decimal? CalculateRecommendedProtein(decimal weightKg)
        {
            return Math.Round(weightKg * 1.6m, 1);
        }
    }
}
