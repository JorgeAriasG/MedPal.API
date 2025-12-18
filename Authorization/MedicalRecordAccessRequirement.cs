using Microsoft.AspNetCore.Authorization;

namespace MedPal.API.Authorization
{
    /// <summary>
    /// Requirement for medical record access authorization (NOM compliance)
    /// </summary>
    public class MedicalRecordAccessRequirement : IAuthorizationRequirement
    {
        public int MedicalHistoryId { get; set; }

        public MedicalRecordAccessRequirement()
        {
        }

        public MedicalRecordAccessRequirement(int medicalHistoryId)
        {
            MedicalHistoryId = medicalHistoryId;
        }
    }
}
