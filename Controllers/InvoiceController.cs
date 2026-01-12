using AutoMapper;
using MedPal.API.DTOs;
using MedPal.API.Models;
using MedPal.API.Enums;
using MedPal.API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace MedPal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IMapper _mapper;

        public InvoiceController(IInvoiceRepository invoiceRepository, IMapper mapper)
        {
            _invoiceRepository = invoiceRepository;
            _mapper = mapper;
        }

        // GET: api/invoice
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InvoiceReadDTO>>> GetAllInvoices()
        {
            var invoices = await _invoiceRepository.GetAllInvoicesAsync();
            var dtos = _mapper.Map<IEnumerable<InvoiceReadDTO>>(invoices);
            return Ok(dtos);
        }

        // GET: api/invoice/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<InvoiceReadDTO>> GetInvoiceById(int id)
        {
            var invoice = await _invoiceRepository.GetInvoiceByIdAsync(id);
            if (invoice == null)
            {
                return NotFound();
            }
            var dto = _mapper.Map<InvoiceReadDTO>(invoice);
            return Ok(dto);
        }

        // GET: api/invoice/patient/{patientId}
        [HttpGet("patient/{patientId}")]
        public async Task<ActionResult<IEnumerable<InvoiceReadDTO>>> GetInvoicesByPatientId(int patientId)
        {
            var invoices = await _invoiceRepository.GetInvoicesByPatientIdAsync(patientId);
            var dtos = _mapper.Map<IEnumerable<InvoiceReadDTO>>(invoices);
            return Ok(dtos);
        }

        // GET: api/invoice/appointment/{appointmentId}
        [HttpGet("appointment/{appointmentId}")]
        public async Task<ActionResult<IEnumerable<InvoiceReadDTO>>> GetInvoicesByAppointmentId(int appointmentId)
        {
            var invoices = await _invoiceRepository.GetInvoicesByAppointmentIdAsync(appointmentId);
            var dtos = _mapper.Map<IEnumerable<InvoiceReadDTO>>(invoices);
            return Ok(dtos);
        }

        // GET: api/invoice/status/{status}
        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<InvoiceReadDTO>>> GetInvoicesByStatus(InvoiceStatus status)
        {
            var invoices = await _invoiceRepository.GetInvoicesByStatusAsync(status);
            var dtos = _mapper.Map<IEnumerable<InvoiceReadDTO>>(invoices);
            return Ok(dtos);
        }

        // POST: api/invoice
        [HttpPost]
        public async Task<ActionResult<InvoiceReadDTO>> CreateInvoice([FromBody] InvoiceWriteDTO writeDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var invoice = _mapper.Map<Invoice>(writeDTO);
            invoice.CreatedAt = DateTime.UtcNow;
            invoice.Status = InvoiceStatus.Pending;

            var createdInvoice = await _invoiceRepository.AddInvoiceAsync(invoice);
            await _invoiceRepository.CompleteAsync();

            var readDTO = _mapper.Map<InvoiceReadDTO>(createdInvoice);
            return CreatedAtAction(nameof(GetInvoiceById), new { id = createdInvoice.Id }, readDTO);
        }

        // PUT: api/invoice/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInvoice(int id, [FromBody] InvoiceWriteDTO writeDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var invoice = await _invoiceRepository.GetInvoiceByIdAsync(id);
            if (invoice == null)
            {
                return NotFound();
            }

            _mapper.Map(writeDTO, invoice);
            invoice.UpdatedAt = DateTime.UtcNow;

            _invoiceRepository.UpdateInvoice(invoice);
            await _invoiceRepository.CompleteAsync();

            return NoContent();
        }

        // PATCH: api/invoice/{id}/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateInvoiceStatus(int id, [FromBody] UpdateInvoiceStatusRequest request)
        {
            if (!Enum.TryParse<InvoiceStatus>(request.Status, ignoreCase: true, out var status))
            {
                return BadRequest($"Invalid status. Valid values are: {string.Join(", ", Enum.GetNames(typeof(InvoiceStatus)))}");
            }

            var invoice = await _invoiceRepository.GetInvoiceByIdAsync(id);
            if (invoice == null)
            {
                return NotFound();
            }

            invoice.Status = status;
            invoice.UpdatedAt = DateTime.UtcNow;

            _invoiceRepository.UpdateInvoice(invoice);
            await _invoiceRepository.CompleteAsync();

            return NoContent();
        }

        // DELETE: api/invoice/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvoice(int id)
        {
            var invoice = await _invoiceRepository.GetInvoiceByIdAsync(id);
            if (invoice == null)
            {
                return NotFound();
            }

            _invoiceRepository.RemoveInvoice(invoice);
            await _invoiceRepository.CompleteAsync();

            return NoContent();
        }
    }

    public class UpdateInvoiceStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }
}
