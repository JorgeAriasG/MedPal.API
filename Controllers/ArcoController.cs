using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MedPal.API.Models;
using MedPal.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedPal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArcoController : BaseController
    {
        private readonly IArcoService _arcoService;

        public ArcoController(IArcoService arcoService)
        {
            _arcoService = arcoService;
        }

        // POST: api/arco/request
        [HttpPost("request")]
        [Authorize] // Any user can request
        public async Task<ActionResult> CreateRequest([FromBody] ArcoRequestDto dto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = int.TryParse(userIdStr, out var id) ? id : null;

            var request = new ArcoRequest
            {
                UserId = userId,
                PatientId = dto.PatientId, // Optional: if request is on behalf of a patient
                RequestType = dto.RequestType,
                Details = dto.Details,
                Status = ArcoRequestStatus.Pending
            };

            var created = await _arcoService.CreateRequestAsync(request);
            return Ok(new { Message = "ARCO request submitted.", RequestId = created.Id });
        }

        // GET: api/arco/export/{patientId} (Right to Access)
        [HttpGet("export/{patientId}")]
        [Authorize(Policy = "Users.Manage")] // Only admins or authorized staff for now
        public async Task<ActionResult> ExportData(int patientId)
        {
            var data = await _arcoService.ExportUserDataAsync(patientId);
            if (data == null) return NotFound("Patient not found.");

            return Ok(data);
        }

        // POST: api/arco/execute/{requestId} (Admin executes the action)
        [HttpPost("execute/{requestId}")]
        [Authorize(Policy = "Users.Manage")]
        public async Task<ActionResult> ExecuteArcoAction(int requestId)
        {
            // Ideally retrieve request and check type. 
            // For MVP simplicity, we assume the admin knows what they are triggering based on the pending queue.
            // In a real app, this would be more complex state machine.
            return BadRequest("Use specific action endpoints for now.");
        }

        // POST: api/arco/anonymize/{patientId} (Right to Cancellation)
        [HttpPost("anonymize/{patientId}")]
        [Authorize(Policy = "Users.Manage")] // Critical action
        public async Task<ActionResult> AnonymizePatient(int patientId)
        {
            await _arcoService.AnonymizePatientAsync(patientId);
            return Ok("Patient data anonymized (Right to Cancellation executed).");
        }

        // POST: api/arco/marketing-block/{patientId} (Right to Opposition)
        [HttpPost("marketing-block/{patientId}")]
        [Authorize(Policy = "Users.Manage")]
        public async Task<ActionResult> ToggleMarketingBlock(int patientId, [FromQuery] bool block)
        {
            await _arcoService.ToggleMarketingBlockAsync(patientId, block);
            return Ok($"Marketing block set to {block} (Right to Opposition executed).");
        }
    }

    public class ArcoRequestDto
    {
        public int? PatientId { get; set; }
        public ArcoRequestType RequestType { get; set; }
        public string Details { get; set; }
    }
}
