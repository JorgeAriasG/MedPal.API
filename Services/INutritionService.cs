using MedPal.API.DTOs;

namespace MedPal.API.Services
{
    public interface INutritionService
    {
        Task<NutritionAssessmentDTO> CalculateAssessmentAsync(int patientDetailsId);
        Task<InBodySyncResultDTO> SyncInBodyDataAsync(InBodySyncDTO syncDto);
    }
}
