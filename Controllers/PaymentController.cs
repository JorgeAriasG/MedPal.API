using AutoMapper;
using MedPal.API.DTOs;
using MedPal.API.Models;
using MedPal.API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace MedPal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IMapper _mapper;

        public PaymentController(IPaymentRepository paymentRepository, IInvoiceRepository invoiceRepository, IMapper mapper)
        {
            _paymentRepository = paymentRepository;
            _invoiceRepository = invoiceRepository;
            _mapper = mapper;
        }

        // GET: api/payment
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentReadDTO>>> GetAllPayments()
        {
            var payments = await _paymentRepository.GetAllPaymentsAsync();
            var dtos = _mapper.Map<IEnumerable<PaymentReadDTO>>(payments);
            return Ok(dtos);
        }

        // GET: api/payment/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentReadDTO>> GetPaymentById(int id)
        {
            var payment = await _paymentRepository.GetPaymentByIdAsync(id);
            if (payment == null)
            {
                return NotFound();
            }
            var dto = _mapper.Map<PaymentReadDTO>(payment);
            return Ok(dto);
        }

        // GET: api/payment/invoice/{invoiceId}
        [HttpGet("invoice/{invoiceId}")]
        public async Task<ActionResult<IEnumerable<PaymentReadDTO>>> GetPaymentsByInvoiceId(int invoiceId)
        {
            var payments = await _paymentRepository.GetPaymentsByInvoiceIdAsync(invoiceId);
            var dtos = _mapper.Map<IEnumerable<PaymentReadDTO>>(payments);
            return Ok(dtos);
        }

        // GET: api/payment/patient/{patientId}
        [HttpGet("patient/{patientId}")]
        public async Task<ActionResult<IEnumerable<PaymentReadDTO>>> GetPaymentsByPatientId(int patientId)
        {
            var payments = await _paymentRepository.GetPaymentsByPatientIdAsync(patientId);
            var dtos = _mapper.Map<IEnumerable<PaymentReadDTO>>(payments);
            return Ok(dtos);
        }

        // POST: api/payment
        [HttpPost]
        public async Task<ActionResult<PaymentReadDTO>> CreatePayment([FromBody] PaymentWriteDTO writeDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verify invoice exists
            var invoice = await _invoiceRepository.GetInvoiceByIdAsync(writeDTO.InvoiceId);
            if (invoice == null)
            {
                return NotFound($"Invoice with ID {writeDTO.InvoiceId} not found");
            }

            var payment = _mapper.Map<Payment>(writeDTO);
            payment.PaymentDate = DateTime.UtcNow;
            payment.CreatedAt = DateTime.UtcNow;

            var createdPayment = await _paymentRepository.AddPaymentAsync(payment);
            await _paymentRepository.CompleteAsync();

            var readDTO = _mapper.Map<PaymentReadDTO>(createdPayment);
            return CreatedAtAction(nameof(GetPaymentById), new { id = createdPayment.Id }, readDTO);
        }

        // PUT: api/payment/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePayment(int id, [FromBody] PaymentWriteDTO writeDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var payment = await _paymentRepository.GetPaymentByIdAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            // Verify invoice exists if changed
            if (payment.InvoiceId != writeDTO.InvoiceId)
            {
                var invoice = await _invoiceRepository.GetInvoiceByIdAsync(writeDTO.InvoiceId);
                if (invoice == null)
                {
                    return NotFound($"Invoice with ID {writeDTO.InvoiceId} not found");
                }
            }

            _mapper.Map(writeDTO, payment);
            payment.UpdatedAt = DateTime.UtcNow;

            _paymentRepository.UpdatePayment(payment);
            await _paymentRepository.CompleteAsync();

            return NoContent();
        }

        // DELETE: api/payment/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePayment(int id)
        {
            var payment = await _paymentRepository.GetPaymentByIdAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            _paymentRepository.RemovePayment(payment);
            await _paymentRepository.CompleteAsync();

            return NoContent();
        }
    }
}
