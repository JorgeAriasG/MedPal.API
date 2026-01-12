using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MedPal.API.Models; // Ensure Models namespace is imported
using MedPal.API.DTOs;
using MedPal.API.Repositories;
using MedPal.API.Services;

namespace MedPal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrescriptionController : BaseController
    {
        private readonly IPrescriptionRepository _prescriptionRepository;
        private readonly IQrCodeService _qrCodeService;
        private readonly IUserRepository _userRepository; // To verify doctor
        private readonly IPatientRepository _patientRepository; // To verify patient

        public PrescriptionController(
            IPrescriptionRepository prescriptionRepository,
            IQrCodeService qrCodeService,
            IUserRepository userRepository,
            IPatientRepository patientRepository)
        {
            _prescriptionRepository = prescriptionRepository;
            _qrCodeService = qrCodeService;
            _userRepository = userRepository;
            _patientRepository = patientRepository;
        }

        // POST: api/Prescription
        [HttpPost]
        [Authorize(Policy = "MedicalRecords.Create")] // Assuming doctors have this permission
        public async Task<ActionResult<PrescriptionReadDTO>> CreatePrescription(PrescriptionWriteDTO prescriptionDto)
        {
            var doctorIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(doctorIdStr, out int doctorId))
            {
                return Unauthorized();
            }

            var patient = await _patientRepository.GetPatientByIdAsync(prescriptionDto.PatientId);
            if (patient == null)
            {
                return NotFound("Patient not found");
            }

            var prescription = new Prescription
            {
                DoctorId = doctorId,
                PatientId = prescriptionDto.PatientId,
                Diagnosis = prescriptionDto.Diagnosis,
                Notes = prescriptionDto.Notes,
                ExpiresAt = prescriptionDto.ExpiresAt,
                Items = prescriptionDto.Items.Select(i => new PrescriptionItem
                {
                    MedicationName = i.MedicationName,
                    Dosage = i.Dosage,
                    Frequency = i.Frequency,
                    Duration = i.Duration,
                    Instructions = i.Instructions
                }).ToList()
            };

            await _prescriptionRepository.AddAsync(prescription);

            // Fetch complete entity to return DTO
            var createdPrescription = await _prescriptionRepository.GetByIdAsync(prescription.Id);

            return CreatedAtAction(nameof(GetPrescriptionToken), new { id = createdPrescription.Id }, MapToReadDTO(createdPrescription));
        }

        // GET: api/Prescription/{id}
        [HttpGet("{id}")]
        [Authorize(Policy = "MedicalRecords.ViewOwn")]
        public async Task<ActionResult<PrescriptionReadDTO>> GetPrescriptionToken(int id)
        {
            var prescription = await _prescriptionRepository.GetByIdAsync(id);

            if (prescription == null)
            {
                return NotFound();
            }

            // Optional: Check if user has access (Doctor who created it or Patient who owns it)
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // Add logic here if strictly enforcing ownership view

            return MapToReadDTO(prescription);
        }

        // GET: api/Prescription/patient/{patientId}
        [HttpGet("patient/{patientId}")]
        [Authorize(Policy = "MedicalRecords.ViewOwn")]
        public async Task<ActionResult<IEnumerable<PrescriptionReadDTO>>> GetPrescriptionsByPatientId(int patientId)
        {
            // Optional: Validate if User has access to this Patient (Doctor assigned or Patient themselves)
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // TODO: Add stricter check: Is this user the Patient or their assigned Doctor?

            var prescriptions = await _prescriptionRepository.GetByPatientIdAsync(patientId);
            return Ok(prescriptions.Select(MapToReadDTO));
        }

        // GET: api/Prescription/validate/{uniqueCode}
        [HttpGet("validate/{uniqueCode}")]
        [AllowAnonymous] // Public endpoint for pharmacies
        public async Task<ActionResult<PrescriptionValidationDTO>> ValidatePrescription(Guid uniqueCode)
        {
            var prescription = await _prescriptionRepository.GetByUniqueCodeAsync(uniqueCode);

            if (prescription == null)
            {
                return NotFound("Prescription not found");
            }

            var isValid = prescription.Status == PrescriptionStatus.Active && prescription.ExpiresAt > DateTime.UtcNow;

            return new PrescriptionValidationDTO
            {
                IsValid = isValid,
                Status = prescription.Status.ToString(),
                DoctorName = prescription.Doctor?.Name ?? "Unknown",
                IssuedAt = prescription.IssuedAt,
                ExpiresAt = prescription.ExpiresAt,
                Items = prescription.Items.Select(i => new PrescriptionItemDTO
                {
                    MedicationName = i.MedicationName,
                    Dosage = i.Dosage,
                    Frequency = i.Frequency,
                    Duration = i.Duration,
                    Instructions = i.Instructions
                })
            };
        }

        // GET: api/Prescription/{id}/qr
        [HttpGet("{id}/qr")]
        [Authorize]
        public async Task<IActionResult> GetPrescriptionQr(int id)
        {
            var prescription = await _prescriptionRepository.GetByIdAsync(id);
            if (prescription == null)
            {
                return NotFound();
            }

            // URL that the QR will point to (The validation endpoint in frontend or backend)
            // Ideally points to a frontend URL like: https://medpal.app/verify/GUID
            // For now we will encode the GUID directly or the validate API URL
            var qrContent = prescription.UniqueCode.ToString();
            var qrCodeBytes = _qrCodeService.GenerateQrCode(qrContent);

            return File(qrCodeBytes, "image/png");
        }

        private PrescriptionReadDTO MapToReadDTO(Prescription p)
        {
            return new PrescriptionReadDTO
            {
                Id = p.Id,
                UniqueCode = p.UniqueCode,
                DoctorName = p.Doctor?.Name ?? "Unknown",
                DoctorSpecialty = p.Doctor?.Specialty,
                DoctorLicense = p.Doctor?.ProfessionalLicenseNumber,
                PatientName = $"{p.Patient?.Name} {p.Patient?.Lastname}",
                Diagnosis = p.Diagnosis,
                Notes = p.Notes,
                IssuedAt = p.IssuedAt,
                ExpiresAt = p.ExpiresAt,
                Status = p.Status.ToString(),
                Items = p.Items.Select(i => new PrescriptionItemDTO
                {
                    MedicationName = i.MedicationName,
                    Dosage = i.Dosage,
                    Frequency = i.Frequency,
                    Duration = i.Duration,
                    Instructions = i.Instructions
                })
            };
        }
    }
}
