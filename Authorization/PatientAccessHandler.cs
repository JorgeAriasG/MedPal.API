using System.Linq;
using System.Security.Claims;
using MedPal.API.Repositories;
using MedPal.API.Services;
using Microsoft.AspNetCore.Authorization;

namespace MedPal.API.Authorization
{
    /// <summary>
    /// Authorization handler for patient record access.
    /// Rules:
    /// 1. The owner themselves (portal, patient_id claim) can only access their own record.
    /// 2. Any staff of the patient's account can access the patient record.
    ///    Patients without an account (legacy ghosts / portal sign-ups) are visible to
    ///    staff of a clinic the patient is linked to.
    /// 3. SuperAdmin has full access.
    /// </summary>
    public class PatientAccessHandler : AuthorizationHandler<PatientAccessRequirement>
    {
        private readonly IPatientRepository _patientRepository;
        private readonly ITenantContextService _tenantContext;

        public PatientAccessHandler(
            IPatientRepository patientRepository,
            ITenantContextService tenantContext)
        {
            _patientRepository = patientRepository;
            _tenantContext = tenantContext;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PatientAccessRequirement requirement)
        {
            var patient = await _patientRepository.GetPatientByIdAsync(requirement.PatientId);
            if (patient == null)
            {
                context.Fail();
                return;
            }

            // RULE 1: Portal patient — only their own record
            var patientIdClaim = context.User.FindFirst("patient_id");
            if (patientIdClaim != null && int.TryParse(patientIdClaim.Value, out int patientId))
            {
                if (patientId == patient.Id)
                {
                    context.Succeed(requirement);
                }
                else
                {
                    context.Fail();
                }
                return;
            }

            // RULE 2: SuperAdmin has full access
            if (_tenantContext.IsSuperAdmin)
            {
                context.Succeed(requirement);
                return;
            }

            // RULE 3: Staff of the patient's account
            var accountId = _tenantContext.CurrentAccountId;
            if (accountId.HasValue)
            {
                if (patient.AccountId.HasValue && patient.AccountId.Value == accountId.Value)
                {
                    context.Succeed(requirement);
                    return;
                }

                // Cross-account membership: patient verified the link and consented to share their profile
                if (patient.PatientAccounts != null &&
                    patient.PatientAccounts.Any(pa =>
                        pa.AccountId == accountId.Value &&
                        pa.IsVerifiedByPatient &&
                        (pa.ConsentToShareProfile ?? false)))
                {
                    context.Succeed(requirement);
                    return;
                }

                // Legacy patient without account: allow staff whose clinic is linked to the patient.
                var clinicId = _tenantContext.CurrentClinicId;
                if (patient.AccountId == null && clinicId.HasValue &&
                    patient.PatientClinics != null &&
                    patient.PatientClinics.Any(pc => pc.ClinicId == clinicId.Value && !pc.IsDeleted))
                {
                    context.Succeed(requirement);
                    return;
                }
            }

            context.Fail();
        }
    }
}