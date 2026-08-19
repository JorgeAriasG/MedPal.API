using MedPal.API.Data;
using MedPal.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedPal.API.Services.Implementations
{
    /// <summary>
    /// Implementation of IPatientConsentService for managing patient consent.
    /// Handles consent granting, revocation, and validation with full audit trail.
    /// </summary>
    public class ConsentService : IPatientConsentService
    {
        private readonly AppDbContext _context;
        private readonly ITenantContextService _tenantContext;
        private readonly ILogger<ConsentService> _logger;

        public ConsentService(AppDbContext context, ITenantContextService tenantContext, ILogger<ConsentService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Grants consent for a requesting clinic to access patient medical records.
        /// </summary>
        public async Task<PatientConsent> GrantConsentAsync(int patientDetailsId, int requestingClinicId, int ownerClinicId, string consentScope, int userId, DateTime? expiryDate = null)
        {
            try
            {
                // Validate that patient exists
                var patient = await _context.PatientDetails.FirstOrDefaultAsync(p => p.Id == patientDetailsId);
                if (patient == null)
                {
                    _logger.LogWarning($"Attempted to grant consent for non-existent patient {patientDetailsId}");
                    throw new InvalidOperationException($"Patient with ID {patientDetailsId} not found");
                }

                // Validate that clinics exist
                var requestingClinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == requestingClinicId);
                var ownerClinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == ownerClinicId);

                if (requestingClinic == null)
                    throw new InvalidOperationException($"Requesting clinic with ID {requestingClinicId} not found");
                if (ownerClinic == null)
                    throw new InvalidOperationException($"Owner clinic with ID {ownerClinicId} not found");

                // Check for existing consent
                var existingConsent = await _context.PatientConsents
                    .IgnoreQueryFilters() // Include soft-deleted records
                    .FirstOrDefaultAsync(pc => 
                        pc.PatientDetailsId == patientDetailsId &&
                        pc.RequestingClinicId == requestingClinicId &&
                        pc.OwnerClinicId == ownerClinicId);

                PatientConsent consent;

                if (existingConsent != null)
                {
                    // Update existing consent
                    existingConsent.ConsentScope = consentScope;
                    existingConsent.IsApproved = true;
                    existingConsent.ConsentDate = DateTime.UtcNow;
                    existingConsent.ExpiryDate = expiryDate;
                    existingConsent.ApprovedByUserId = userId;
                    existingConsent.UpdatedAt = DateTime.UtcNow;
                    existingConsent.UpdatedByUserId = userId;
                    existingConsent.IsDeleted = false; // Re-activate if previously revoked
                    existingConsent.DeletedAt = null;
                    existingConsent.DeletedByUserId = null;

                    _context.PatientConsents.Update(existingConsent);
                    consent = existingConsent;
                }
                else
                {
                    // Create new consent
                    consent = new PatientConsent
                    {
                        PatientDetailsId = patientDetailsId,
                        RequestingClinicId = requestingClinicId,
                        OwnerClinicId = ownerClinicId,
                        ConsentScope = consentScope,
                        IsApproved = true,
                        ConsentDate = DateTime.UtcNow,
                        ExpiryDate = expiryDate,
                        ApprovedByUserId = userId,
                        CreatedAt = DateTime.UtcNow,
                        CreatedByUserId = userId,
                        UpdatedAt = DateTime.UtcNow,
                        UpdatedByUserId = userId,
                        IsDeleted = false
                    };

                    await _context.PatientConsents.AddAsync(consent);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Consent granted: Patient {patientDetailsId}, Requesting Clinic {requestingClinicId}, Owner Clinic {ownerClinicId}, Scope: {consentScope}");

                return consent;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error granting consent: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Revokes consent for a requesting clinic to access patient medical records.
        /// </summary>
        public async Task<bool> RevokeConsentAsync(int patientDetailsId, int requestingClinicId, int ownerClinicId, int userId)
        {
            try
            {
                var consent = await _context.PatientConsents
                    .FirstOrDefaultAsync(pc =>
                        pc.PatientDetailsId == patientDetailsId &&
                        pc.RequestingClinicId == requestingClinicId &&
                        pc.OwnerClinicId == ownerClinicId &&
                        !pc.IsDeleted);

                if (consent == null)
                {
                    _logger.LogWarning($"Attempted to revoke non-existent consent: Patient {patientDetailsId}, Clinic {requestingClinicId}");
                    return false;
                }

                // Soft delete
                consent.IsDeleted = true;
                consent.DeletedAt = DateTime.UtcNow;
                consent.DeletedByUserId = userId;
                consent.UpdatedAt = DateTime.UtcNow;
                consent.UpdatedByUserId = userId;

                _context.PatientConsents.Update(consent);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Consent revoked: Patient {patientDetailsId}, Requesting Clinic {requestingClinicId}, Owner Clinic {ownerClinicId}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error revoking consent: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Checks if valid, non-expired consent exists for accessing medical records.
        /// </summary>
        public async Task<bool> IsConsentValidAsync(int patientDetailsId, int requestingClinicId, int ownerClinicId)
        {
            try
            {
                var consent = await _context.PatientConsents
                    .FirstOrDefaultAsync(pc =>
                        pc.PatientDetailsId == patientDetailsId &&
                        pc.RequestingClinicId == requestingClinicId &&
                        pc.OwnerClinicId == ownerClinicId &&
                        pc.IsApproved &&
                        !pc.IsDeleted);

                if (consent == null)
                {
                    _logger.LogWarning($"No valid consent found: Patient {patientDetailsId}, Requesting Clinic {requestingClinicId}");
                    return false;
                }

                // Check expiry
                if (consent.ExpiryDate.HasValue && consent.ExpiryDate.Value < DateTime.UtcNow)
                {
                    _logger.LogWarning($"Consent expired: Patient {patientDetailsId}, Requesting Clinic {requestingClinicId}, Expiry: {consent.ExpiryDate}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking consent validity: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Retrieves all non-deleted consent records for a specific patient.
        /// </summary>
        public async Task<IEnumerable<PatientConsent>> GetPatientConsentsAsync(int patientDetailsId)
        {
            try
            {
                return await _context.PatientConsents
                    .Where(pc => pc.PatientDetailsId == patientDetailsId && !pc.IsDeleted)
                    .Include(pc => pc.RequestingClinic)
                    .Include(pc => pc.OwnerClinic)
                    .Include(pc => pc.ApprovedByUser)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving patient consents: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Retrieves active (non-expired) consent records for a specific patient.
        /// </summary>
        public async Task<IEnumerable<PatientConsent>> GetActiveConsentsAsync(int patientDetailsId)
        {
            try
            {
                var now = DateTime.UtcNow;

                return await _context.PatientConsents
                    .Where(pc => 
                        pc.PatientDetailsId == patientDetailsId &&
                        pc.IsApproved &&
                        !pc.IsDeleted &&
                        (pc.ExpiryDate == null || pc.ExpiryDate > now))
                    .Include(pc => pc.RequestingClinic)
                    .Include(pc => pc.OwnerClinic)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving active consents: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Retrieves consent records for a specific clinic relationship.
        /// </summary>
        public async Task<IEnumerable<PatientConsent>> GetConsentsByClinicAsync(int patientDetailsId, int requestingClinicId)
        {
            try
            {
                return await _context.PatientConsents
                    .Where(pc => 
                        pc.PatientDetailsId == patientDetailsId &&
                        pc.RequestingClinicId == requestingClinicId &&
                        !pc.IsDeleted)
                    .Include(pc => pc.OwnerClinic)
                    .Include(pc => pc.ApprovedByUser)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving consents by clinic: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Checks if a patient has granted explicit consent to a specific doctor.
        /// Looks for PatientConsent with TargetDoctorId matching the requesting doctor.
        /// </summary>
        public async Task<bool> IsConsentForDoctorValidAsync(int patientDetailsId, int targetDoctorId)
        {
            try
            {
                // Doctor-level consent (TargetDoctorId matches)
                var consent = await _context.PatientConsents
                    .FirstOrDefaultAsync(pc =>
                        pc.PatientDetailsId == patientDetailsId &&
                        pc.TargetDoctorId == targetDoctorId &&
                        pc.IsApproved &&
                        !pc.IsDeleted);

                // Clinic-level consent (TargetDoctorId == null) also covers any doctor of the requesting clinic
                if (consent == null && _tenantContext.CurrentClinicId.HasValue)
                {
                    consent = await _context.PatientConsents
                        .FirstOrDefaultAsync(pc =>
                            pc.PatientDetailsId == patientDetailsId &&
                            pc.TargetDoctorId == null &&
                            pc.RequestingClinicId == _tenantContext.CurrentClinicId.Value &&
                            pc.IsApproved &&
                            !pc.IsDeleted);
                }

                if (consent == null)
                    return false;

                if (consent.ExpiryDate.HasValue && consent.ExpiryDate.Value < DateTime.UtcNow)
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking doctor consent: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Checks if a specific consent record is expired.
        /// </summary>
        public async Task<bool> IsConsentExpiredAsync(PatientConsent consent)
        {
            if (consent == null)
                return true;

            if (!consent.ExpiryDate.HasValue)
                return false; // No expiry date = never expires

            return consent.ExpiryDate.Value < DateTime.UtcNow;
        }

        /// <summary>
        /// Automatically revokes expired consents for a patient.
        /// </summary>
        public async Task<int> RevokeExpiredConsentsAsync(int patientDetailsId)
        {
            try
            {
                var now = DateTime.UtcNow;
                var expiredConsents = await _context.PatientConsents
                    .Where(pc =>
                        pc.PatientDetailsId == patientDetailsId &&
                        pc.ExpiryDate.HasValue &&
                        pc.ExpiryDate.Value < now &&
                        !pc.IsDeleted)
                    .ToListAsync();

                if (expiredConsents.Count == 0)
                    return 0;

                var systemUserId = _tenantContext.CurrentUserId ?? 1; // Fallback to system user

                foreach (var consent in expiredConsents)
                {
                    consent.IsDeleted = true;
                    consent.DeletedAt = now;
                    consent.DeletedByUserId = systemUserId;
                    consent.UpdatedAt = now;
                    consent.UpdatedByUserId = systemUserId;
                }

                _context.PatientConsents.UpdateRange(expiredConsents);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Revoked {expiredConsents.Count} expired consents for patient {patientDetailsId}");

                return expiredConsents.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error revoking expired consents: {ex.Message}");
                throw;
            }
        }
    }
}
