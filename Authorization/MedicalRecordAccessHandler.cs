using System.Security.Claims;
using MedPal.API.Repositories;
using MedPal.API.Repositories.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace MedPal.API.Authorization
{
    /// <summary>
    /// Authorization handler for medical record access (NOM-004-SSA3-2012 compliance)
    /// Only allows access to:
    /// 1. The healthcare professional who created the record
    /// 2. System administrators (for auditing)
    /// 3. The patient themselves (if they have a User account)
    /// </summary>
    public class MedicalRecordAccessHandler : AuthorizationHandler<MedicalRecordAccessRequirement>
    {
        private readonly IMedicalHistoryRepository _medicalHistoryRepository;
        private readonly IPermissionRepository _permissionRepository;

        public MedicalRecordAccessHandler(
            IMedicalHistoryRepository medicalHistoryRepository,
            IPermissionRepository permissionRepository)
        {
            _medicalHistoryRepository = medicalHistoryRepository;
            _permissionRepository = permissionRepository;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            MedicalRecordAccessRequirement requirement)
        {
            // Get userId from JWT token
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                context.Fail();
                return;
            }

            // Get the medical record
            var medicalHistory = await _medicalHistoryRepository.GetMedicalHistoryByIdAsync(requirement.MedicalHistoryId);
            if (medicalHistory == null)
            {
                context.Fail();
                return;
            }

            // RULE 1: The healthcare professional who created the record ALWAYS has access (NOM-004-SSA3-2012)
            if (medicalHistory.HealthcareProfessionalId == userId)
            {
                context.Succeed(requirement);
                return;
            }

            // RULE 2: Admins can access for auditing and supervision
            bool isAdmin = await _permissionRepository.UserHasPermissionAsync(
                userId, "MedicalRecords.ViewAll", null);

            if (isAdmin)
            {
                context.Succeed(requirement);
                return;
            }

            // RULE 3: If it's the patient viewing their own medical record
            var patient = medicalHistory.PatientDetails?.Patient;
            if (patient?.UserId == userId)
            {
                context.Succeed(requirement);
                return;
            }

            // If none of the rules match, deny access
            context.Fail();
        }
    }
}
