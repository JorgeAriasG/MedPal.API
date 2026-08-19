using Microsoft.AspNetCore.Authorization;

namespace MedPal.API.Authorization
{
    /// <summary>
    /// Requirement for medical record access authorization (NOM-004 compliance)
    /// </summary>
    public class MedicalRecordAccessRequirement : IAuthorizationRequirement
    {
        public int MedicalHistoryId { get; set; }

        /// <summary>
        /// Whether the patient themselves may access the record (read flows).
        /// Write flows must pass false so only the creator/admin/consent rules apply.
        /// </summary>
        public bool AllowSelfAccess { get; set; } = true;

        public MedicalRecordAccessRequirement()
        {
        }

        public MedicalRecordAccessRequirement(int medicalHistoryId)
        {
            MedicalHistoryId = medicalHistoryId;
        }

        public MedicalRecordAccessRequirement(int medicalHistoryId, bool allowSelfAccess)
        {
            MedicalHistoryId = medicalHistoryId;
            AllowSelfAccess = allowSelfAccess;
        }
    }
}