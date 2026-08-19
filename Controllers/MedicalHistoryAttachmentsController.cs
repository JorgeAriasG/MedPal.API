using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MedPal.API.DTOs;
using MedPal.API.Models;
using MedPal.API.Repositories;
using MedPal.API.Services;

namespace MedPal.API.Controllers
{
    [ApiController]
    [Authorize]
    public class MedicalHistoryAttachmentsController : ControllerBase
    {
        private static readonly string[] AllowedTypes = { "radio", "photo", "doc" };

        private readonly IMedicalHistoryRepository _medicalHistoryRepository;
        private readonly IClinicalAttachmentRepository _attachmentRepository;
        private readonly IAttachmentStorageService _storageService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IUserService _userService;

        public MedicalHistoryAttachmentsController(
            IMedicalHistoryRepository medicalHistoryRepository,
            IClinicalAttachmentRepository attachmentRepository,
            IAttachmentStorageService storageService,
            IAuthorizationService authorizationService,
            IUserService userService)
        {
            _medicalHistoryRepository = medicalHistoryRepository;
            _attachmentRepository = attachmentRepository;
            _storageService = storageService;
            _authorizationService = authorizationService;
            _userService = userService;
        }

        // GET: api/medicalhistory/{medicalHistoryId}/attachments
        [HttpGet("api/medicalhistory/{medicalHistoryId}/attachments")]
        [Authorize(Policy = "MedicalRecords.ViewOwn")]
        public async Task<ActionResult<IEnumerable<ClinicalAttachmentReadDTO>>> GetAttachments(int medicalHistoryId)
        {
            var medicalHistory = await _medicalHistoryRepository.GetMedicalHistoryByIdAsync(medicalHistoryId);
            if (medicalHistory == null)
            {
                return NotFound();
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, medicalHistory, "MedicalRecords.Read");
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            var attachments = await _attachmentRepository.GetByMedicalHistoryIdAsync(medicalHistoryId);
            return Ok(attachments.Select(MapToReadDTO));
        }

        // POST: api/medicalhistory/{medicalHistoryId}/attachments
        [HttpPost("api/medicalhistory/{medicalHistoryId}/attachments")]
        [Authorize(Policy = "MedicalRecords.Update")]
        [RequestSizeLimit(100 * 1024 * 1024)] // 100 MB max
        public async Task<ActionResult<ClinicalAttachmentReadDTO>> UploadAttachment(
            int medicalHistoryId,
            [FromForm] IFormFile file,
            [FromForm] string type = "doc")
        {
            var medicalHistory = await _medicalHistoryRepository.GetMedicalHistoryByIdAsync(medicalHistoryId);
            if (medicalHistory == null)
            {
                return NotFound();
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, medicalHistory, "MedicalRecords.Update");
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Se requiere un archivo." });
            }

            if (!AllowedTypes.Contains(type))
            {
                type = "doc";
            }

            var storagePath = await _storageService.SaveAsync(medicalHistoryId, file.FileName, file.OpenReadStream(), HttpContext.RequestAborted);

            int.TryParse(_userService.UserId, out int userId);

            var attachment = new ClinicalAttachment
            {
                MedicalHistoryId = medicalHistoryId,
                Type = type,
                FileName = file.FileName,
                StoragePath = storagePath,
                MimeType = file.ContentType ?? "application/octet-stream",
                Size = file.Length,
                UploadedByUserId = userId == 0 ? null : userId,
                CreatedAt = DateTime.UtcNow,
                OwnerClinicId = medicalHistory.OwnerClinicId,
            };

            await _attachmentRepository.AddAsync(attachment);
            await _attachmentRepository.CompleteAsync();

            return CreatedAtAction(nameof(GetAttachments), new { medicalHistoryId }, MapToReadDTO(attachment));
        }

        // GET: api/attachments/{id}/content
        [HttpGet("api/attachments/{id}/content")]
        public async Task<IActionResult> DownloadAttachment(int id)
        {
            var attachment = await _attachmentRepository.GetByIdAsync(id);
            if (attachment == null)
            {
                return NotFound();
            }

            var medicalHistory = await _medicalHistoryRepository.GetMedicalHistoryByIdAsync(attachment.MedicalHistoryId);
            if (medicalHistory == null)
            {
                return NotFound();
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, medicalHistory, "MedicalRecords.Read");
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            var fullPath = _storageService.GetFullPath(attachment.StoragePath);
            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound();
            }

            var stream = System.IO.File.OpenRead(fullPath);
            return File(stream, attachment.MimeType ?? "application/octet-stream", attachment.FileName);
        }

        // DELETE: api/attachments/{id}
        [HttpDelete("api/attachments/{id}")]
        [Authorize(Policy = "MedicalRecords.Update")]
        public async Task<IActionResult> DeleteAttachment(int id)
        {
            var attachment = await _attachmentRepository.GetByIdAsync(id);
            if (attachment == null)
            {
                return NotFound();
            }

            var medicalHistory = await _medicalHistoryRepository.GetMedicalHistoryByIdAsync(attachment.MedicalHistoryId);
            if (medicalHistory == null)
            {
                return NotFound();
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, medicalHistory, "MedicalRecords.Update");
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            int.TryParse(_userService.UserId, out int userId);

            attachment.IsDeleted = true;
            attachment.DeletedAt = DateTime.UtcNow;
            attachment.DeletedByUserId = userId == 0 ? null : userId;

            _storageService.Delete(attachment.StoragePath);
            await _attachmentRepository.CompleteAsync();

            return NoContent();
        }

        private static ClinicalAttachmentReadDTO MapToReadDTO(ClinicalAttachment attachment)
        {
            return new ClinicalAttachmentReadDTO
            {
                Id = attachment.Id,
                MedicalHistoryId = attachment.MedicalHistoryId,
                Type = attachment.Type,
                FileName = attachment.FileName,
                MimeType = attachment.MimeType,
                Size = attachment.Size,
                UploadedByUserId = attachment.UploadedByUserId,
                CreatedAt = attachment.CreatedAt,
            };
        }
    }
}
