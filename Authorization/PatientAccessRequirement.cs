using Microsoft.AspNetCore.Authorization;

namespace MedPal.API.Authorization
{
    /// <summary>
    /// Requirement for patient record access.
    /// Access is granted to staff of the patient's account or to the patient themselves (portal).
    /// </summary>
    public class PatientAccessRequirement : IAuthorizationRequirement
    {
        public int PatientId { get; set; }

        public PatientAccessRequirement()
        {
        }

        public PatientAccessRequirement(int patientId)
        {
            PatientId = patientId;
        }
    }
}
