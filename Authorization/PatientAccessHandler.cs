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
    /// 2. Any staff of the patient's eligible account can access the patient record.
    ///    Eligible = active primary membership, or verified-and-consented secondary (A1).
    /// 3. Legacy-ghost clinic-link fallback: patients with NO eligible membership anywhere
    ///    remain reachable by staff of a clinic the patient is linked to (deprecated; T02b).
    /// 4. SuperAdmin has full access.
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

            // RULE 2: Staff of the patient's eligible account (A1)
            var accountId = _tenantContext.CurrentAccountId;
            if (accountId.HasValue)
            {
                // Eligible membership: active primary OR verified-and-consented secondary
                if (patient.PatientAccounts != null &&
                    patient.PatientAccounts.Any(pa =>
                        pa.AccountId == accountId.Value &&
                        !pa.IsDeleted &&
                        (pa.IsPrimaryAccount || (pa.IsVerifiedByPatient && (pa.ConsentToShareProfile ?? false)))))
                {
                    context.Succeed(requirement);
                    return;
                }

                // RULE 3 (deprecated legacy-ghost fallback): clinic link only grants access
                // when the patient has NO eligible membership anywhere.
                var hasEligibleMembershipAnywhere = patient.PatientAccounts != null &&
                    patient.PatientAccounts.Any(pa =>
                        !pa.IsDeleted &&
                        (pa.IsPrimaryAccount || (pa.IsVerifiedByPatient && (pa.ConsentToShareProfile ?? false))));
                if (!hasEligibleMembershipAnywhere)
                {
                    var clinicId = _tenantContext.CurrentClinicId;
                    if (clinicId.HasValue &&
                        patient.PatientClinics != null &&
                        patient.PatientClinics.Any(pc => pc.ClinicId == clinicId.Value && !pc.IsDeleted))
                    {
                        context.Succeed(requirement);
                        return;
                    }
                }
            }

            context.Fail();
        }
    }
}