using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MedPal.API.DTOs;
using MedPal.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedPal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AvailabilityBlocksController : BaseController
    {
        private readonly IAvailabilityBlockService _availabilityBlockService;

        public AvailabilityBlocksController(IAvailabilityBlockService availabilityBlockService)
        {
            _availabilityBlockService = availabilityBlockService;
        }

        // GET: api/AvailabilityBlocks?clinicId={id}&from=yyyy-MM-dd&to=yyyy-MM-dd
        [HttpGet]
        [Authorize(Policy = "ViewAppointmentsPolicy")]
        public async Task<ActionResult<IEnumerable<AvailabilityBlockReadDTO>>> GetBlocks(
            [FromQuery] int clinicId,
            [FromQuery] DateOnly from,
            [FromQuery] DateOnly to)
        {
            if (from > to)
                return BadRequest(new { message = "La fecha inicial no puede ser posterior a la fecha final." });

            var blocks = await _availabilityBlockService.GetBlocksByClinicAsync(clinicId, from, to);
            return Ok(blocks);
        }

        // POST: api/AvailabilityBlocks
        [HttpPost]
        [Authorize(Policy = "Appointments.Create")]
        public async Task<ActionResult<AvailabilityBlockReadDTO>> CreateBlock(AvailabilityBlockWriteDTO request)
        {
            var block = await _availabilityBlockService.CreateBlockAsync(request);
            return CreatedAtAction(nameof(GetBlocks), new { clinicId = block.ClinicId, from = block.Date, to = block.Date }, block);
        }

        // DELETE: api/AvailabilityBlocks/{id}
        [HttpDelete("{id}")]
        [Authorize(Policy = "Appointments.Cancel")]
        public async Task<IActionResult> DeleteBlock(int id)
        {
            var success = await _availabilityBlockService.DeleteBlockAsync(id);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}