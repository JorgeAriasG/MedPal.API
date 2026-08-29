using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentValidation;
using MedPal.API.DTOs;
using MedPal.API.Exceptions;
using MedPal.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MedPal.API.Controllers
{
    /// <summary>
    /// Reserva pública de citas (T02c). Capa HTTP delgada: NO se inyectan repositorios ni
    /// AppDbContext; toda la lógica vive en IBookingService / IPatientRegistrationService.
    /// Las excepciones de dominio se traducen a la forma <c>{ message }</c> que consume la UI.
    /// </summary>
    [ApiController]
    [Route("api/booking")]
    public class BookingController : BaseController
    {
        private readonly IBookingService _bookingService;
        private readonly IPatientRegistrationService _registrationService;
        private readonly ILogger<BookingController> _logger;

        public BookingController(
            IBookingService bookingService,
            IPatientRegistrationService registrationService,
            ILogger<BookingController> logger)
        {
            _bookingService = bookingService;
            _registrationService = registrationService;
            _logger = logger;
        }

        [HttpPost("complete")]
        [AllowAnonymous]
        public async Task<ActionResult<BookingResultDTO>> CompleteBooking([FromBody] BookingCompleteDTO dto)
        {
            try
            {
                var result = await _bookingService.CompleteBookingAsync(GetAuthPatientId(), dto.Sr, dto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return BadRequest(new { message = "Se requiere un link de reserva válido o una sesión de paciente." });
            }
            catch (Exception ex)
            {
                return ToErrorResult(ex);
            }
        }

        [HttpPost("registration/complete")]
        [AllowAnonymous]
        public async Task<ActionResult<PatientLoginResponseDTO>> CompletePatientRegistration([FromBody] CompletePatientRegistrationDTO dto)
        {
            try
            {
                return Ok(await _registrationService.CompletePatientRegistrationAsync(dto));
            }
            catch (Exception ex)
            {
                return ToErrorResult(ex);
            }
        }

        [HttpPost("registration/resend")]
        [AllowAnonymous]
        public async Task<IActionResult> ResendRegistration([FromBody] ResendRegistrationDTO dto)
        {
            try
            {
                return Ok(new { message = await _registrationService.ResendRegistrationAsync(dto) });
            }
            catch (Exception ex)
            {
                return ToErrorResult(ex);
            }
        }

        [HttpGet("availability")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<TimeSlotDTO>>> GetPublicAvailability(
            [FromQuery] string? sr = null,
            [FromQuery] int? clinicId = null,
            [FromQuery] int? doctorId = null,
            [FromQuery] DateOnly? date = null)
        {
            if (!date.HasValue)
                return BadRequest(new { message = "La fecha es requerida." });

            try
            {
                return Ok(await _bookingService.GetPublicAvailabilityAsync(sr, clinicId, doctorId, date.Value, GetAuthPatientId()));
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Se requiere un link de reserva válido o una sesión de paciente." });
            }
            catch (Exception ex)
            {
                return ToErrorResult(ex);
            }
        }

        [HttpPost("staff/link")]
        [Authorize]
        public async Task<ActionResult<BookingLinkDTO>> GenerateStaffLink([FromBody] BookingLinkStaffDTO dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized();

            try
            {
                return Ok(await _bookingService.GenerateStaffLinkAsync(userId, dto));
            }
            catch (Exception ex)
            {
                return ToErrorResult(ex);
            }
        }

        private int? GetAuthPatientId()
        {
            var patientIdClaim = User.FindFirst("patient_id");
            if (patientIdClaim != null && int.TryParse(patientIdClaim.Value, out int pid))
                return pid;

            return null;
        }

        private ActionResult ToErrorResult(Exception ex)
        {
            return ex switch
            {
                KeyNotFoundException => NotFound(new { message = ex.Message }),
                ForbiddenAccessException => StatusCode((int)HttpStatusCode.Forbidden, new { message = ex.Message }),
                UnauthorizedAccessException => Unauthorized(new { message = ex.Message }),
                FluentValidation.ValidationException validationException => BadRequest(new { message = validationException.Message }),
                InvalidOperationException => BadRequest(new { message = ex.Message }),
                _ => HandleUnexpected(ex)
            };
        }

        private ObjectResult HandleUnexpected(Exception ex)
        {
            _logger.LogError(ex, "Error no esperado en booking flow");
            return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "Error interno del servidor." });
        }
    }
}