using MedPal.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MedPal.API.Services
{
    /// <summary>
    /// Service for managing patient consent for medical record access across clinics.
    /// Implements NOM-004 compliance requirements for consent tracking and validation.
    /// </summary>
    public interface IPatientConsentService
    {
        /// <summary>
        /// Grants consent for a requesting clinic to access patient medical records.
        /// Creates or updates a PatientConsent record with audit trail.
        /// </summary>
        /// <param name="patientDetailsId">The patient's details ID</param>
        /// <param name="requestingClinicId">The clinic requesting access</param>
        /// <param name="ownerClinicId">The clinic that owns/manages the medical records</param>
        /// <param name="consentScope">Scope of consent (e.g., "FullAccess", "LabResults", "Prescriptions")</param>
        /// <param name="userId">ID of the user approving the consent</param>
        /// <param name="expiryDate">Optional expiry date for the consent</param>
        /// <returns>The created or updated PatientConsent record</returns>
        Task<PatientConsent> GrantConsentAsync(int patientDetailsId, int requestingClinicId, int ownerClinicId, string consentScope, int userId, DateTime? expiryDate = null);

        /// <summary>
        /// Revokes consent for a requesting clinic to access patient medical records.
        /// Soft-deletes the PatientConsent record with audit trail.
        /// </summary>
        /// <param name="patientDetailsId">The patient's details ID</param>
        /// <param name="requestingClinicId">The clinic whose access is being revoked</param>
        /// <param name="ownerClinicId">The clinic that owns the medical records</param>
        /// <param name="userId">ID of the user revoking the consent</param>
        /// <returns>True if revocation was successful, false if consent not found</returns>
        Task<bool> RevokeConsentAsync(int patientDetailsId, int requestingClinicId, int ownerClinicId, int userId);

        /// <summary>
        /// Checks if valid, non-expired consent exists for accessing medical records.
        /// Considers soft-deleted records and expiry dates.
        /// </summary>
        /// <param name="patientDetailsId">The patient's details ID</param>
        /// <param name="requestingClinicId">The clinic requesting access</param>
        /// <param name="ownerClinicId">The clinic that owns the medical records</param>
        /// <returns>True if valid consent exists and is not expired; false otherwise</returns>
        Task<bool> IsConsentValidAsync(int patientDetailsId, int requestingClinicId, int ownerClinicId);

        /// <summary>
        /// Retrieves all non-deleted consent records for a specific patient.
        /// Includes expired consents for audit purposes.
        /// </summary>
        /// <param name="patientDetailsId">The patient's details ID</param>
        /// <returns>List of PatientConsent records for the patient</returns>
        Task<IEnumerable<PatientConsent>> GetPatientConsentsAsync(int patientDetailsId);

        /// <summary>
        /// Retrieves active (non-expired) consent records for a specific patient.
        /// Excludes soft-deleted and expired consents.
        /// </summary>
        /// <param name="patientDetailsId">The patient's details ID</param>
        /// <returns>List of active PatientConsent records</returns>
        Task<IEnumerable<PatientConsent>> GetActiveConsentsAsync(int patientDetailsId);

        /// <summary>
        /// Retrieves consent records for a specific clinic relationship.
        /// Used to audit what access a clinic has to patient records.
        /// </summary>
        /// <param name="patientDetailsId">The patient's details ID</param>
        /// <param name="requestingClinicId">The clinic requesting records</param>
        /// <returns>List of PatientConsent records for this clinic pair</returns>
        Task<IEnumerable<PatientConsent>> GetConsentsByClinicAsync(int patientDetailsId, int requestingClinicId);

        /// <summary>
        /// Checks if a specific consent record is expired based on ExpiryDate.
        /// </summary>
        /// <param name="consent">The consent record to check</param>
        /// <returns>True if expired; false if still valid or no expiry date set</returns>
        Task<bool> IsConsentExpiredAsync(PatientConsent consent);

        /// <summary>
        /// Automatically revokes expired consents for a patient.
        /// Called periodically or on-demand for cleanup.
        /// </summary>
        /// <param name="patientDetailsId">The patient's details ID</param>
        /// <returns>Number of consents that were expired and revoked</returns>
        Task<int> RevokeExpiredConsentsAsync(int patientDetailsId);
    }
}
