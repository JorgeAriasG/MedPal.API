# 📋 PLAN DETALLADO: FASE 4 - API LAYER & VALIDACIONES

**Fecha de Creación**: 09 Enero 2026  
**Estado del Proyecto**: Phases 1-3 ✅ Completadas (36 migraciones aplicadas)  
**Próximo Hito**: Phase 4 - API Layer

---

## 📊 RESUMEN DE ESTADO ACTUAL

### ✅ Completado (Phases 1-3)

| Fase | Descripción | Estado | Migraciones |
|------|-------------|--------|------------|
| **Phase 1** | Modelos críticos (Invoice, AuditLog, Enums) | ✅ 100% | 6 |
| **Phase 2** | Soft Delete + Auditable Entity interfaces | ✅ 100% | 1 |
| **Phase 3** | EmergencyContact + NotificationMessage | ✅ 100% | 2 |
| **TOTAL** | **Base de datos sincronizada** | ✅ 100% | **36** |

### 📈 Servicios Implementados

```
✅ InvoiceService
   - ValidateInvoiceAsync()
   - UpdatePaidAmountAsync()
   - CalculateStatus()
   - CanAcceptPaymentAsync()

✅ PaymentService
   - ValidatePaymentAsync()
   - CreatePaymentAsync()
   - CancelPaymentAsync()
   - GetPaymentsByInvoiceAsync()
   - ValidatePaymentAmountAsync()

✅ AppDbContext.SaveChangesAsync()
   - Validaciones automáticas de Invoice
   - Validaciones automáticas de Payment
   - Validaciones automáticas de Patient
```

---

## 🎯 PHASE 4: API LAYER & VALIDACIONES AVANZADAS

### 📋 Estructura Propuesta

```
MedPal.API/
├── Controllers/
│   ├── EmergencyContactController.cs        [NUEVO]
│   ├── PaymentController.cs                 [MEJORAR]
│   ├── InvoiceController.cs                 [MEJORAR]
│   └── NotificationMessageController.cs     [NUEVO]
│
├── DTOs/
│   ├── EmergencyContactReadDTO.cs           [NUEVO]
│   ├── EmergencyContactWriteDTO.cs          [NUEVO]
│   ├── PaymentReadDTO.cs                    [NUEVO]
│   ├── PaymentWriteDTO.cs                   [NUEVO]
│   ├── InvoiceReadDTO.cs                    [NUEVO]
│   ├── InvoiceWriteDTO.cs                   [NUEVO]
│   ├── NotificationMessageReadDTO.cs        [NUEVO]
│   └── NotificationMessageWriteDTO.cs       [NUEVO]
│
├── Services/
│   ├── IEmergencyContactService.cs          [NUEVO]
│   ├── EmergencyContactService.cs           [NUEVO]
│   ├── IInvoiceService.cs                   [EXISTE - actualizar]
│   ├── InvoiceService.cs                    [EXISTE - actualizar]
│   ├── IPaymentService.cs                   [EXISTE - actualizar]
│   └── PaymentService.cs                    [EXISTE - actualizar]
│
├── Validation/
│   ├── EmergencyContactValidator.cs         [NUEVO]
│   ├── PaymentValidator.cs                  [NUEVO]
│   ├── InvoiceValidator.cs                  [NUEVO]
│   └── NotificationMessageValidator.cs      [NUEVO]
│
├── Middleware/
│   └── ExceptionHandlingMiddleware.cs       [NUEVO]
│
└── Mapping/
    └── MappingProfile.cs                    [ACTUALIZAR]
```

---

## 📝 TAREAS DETALLADAS

### **TAREA 1: Crear DTOs para EmergencyContact**

#### EmergencyContactReadDTO
```csharp
public class EmergencyContactReadDTO
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string FullName { get; set; }
    public string Relationship { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }
    public int Priority { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

#### EmergencyContactWriteDTO
```csharp
public class EmergencyContactWriteDTO
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(255)]
    public string FullName { get; set; }

    [Required(ErrorMessage = "La relación es requerida")]
    [StringLength(100)]
    public string Relationship { get; set; }

    [Required(ErrorMessage = "El teléfono es requerido")]
    [Phone(ErrorMessage = "Formato de teléfono inválido")]
    public string Phone { get; set; }

    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; }

    [StringLength(500)]
    public string Address { get; set; }

    [Range(1, 10, ErrorMessage = "Prioridad debe estar entre 1 y 10")]
    public int Priority { get; set; } = 1;
}
```

**Ubicación**: `DTOs/EmergencyContactReadDTO.cs` y `DTOs/EmergencyContactWriteDTO.cs`  
**Tiempo Estimado**: 15 minutos

---

### **TAREA 2: Crear DTOs para Payment**

#### PaymentReadDTO
```csharp
public class PaymentReadDTO
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public decimal AmountPaid { get; set; }
    public string PaymentMethod { get; set; }
    public string TransactionReference { get; set; }
    public DateTime PaymentDate { get; set; }
    public string Notes { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

#### PaymentWriteDTO
```csharp
public class PaymentWriteDTO
{
    [Required(ErrorMessage = "El invoice es requerido")]
    public int InvoiceId { get; set; }

    [Required(ErrorMessage = "El monto es requerido")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
    public decimal AmountPaid { get; set; }

    [Required(ErrorMessage = "El método de pago es requerido")]
    [StringLength(50)]
    public string PaymentMethod { get; set; }

    [StringLength(100)]
    public string TransactionReference { get; set; }

    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    [StringLength(500)]
    public string Notes { get; set; }
}
```

**Ubicación**: `DTOs/PaymentReadDTO.cs` y `DTOs/PaymentWriteDTO.cs`  
**Tiempo Estimado**: 15 minutos

---

### **TAREA 3: Crear DTOs para Invoice**

#### InvoiceReadDTO
```csharp
public class InvoiceReadDTO
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int AppointmentId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public string Status { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<PaymentReadDTO> Payments { get; set; }
}
```

#### InvoiceWriteDTO
```csharp
public class InvoiceWriteDTO
{
    [Required(ErrorMessage = "El paciente es requerido")]
    public int PatientId { get; set; }

    [Required(ErrorMessage = "La cita es requerida")]
    public int AppointmentId { get; set; }

    [Required(ErrorMessage = "El monto total es requerido")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
    public decimal TotalAmount { get; set; }

    public DateTime? DueDate { get; set; }
}
```

**Ubicación**: `DTOs/InvoiceReadDTO.cs` y `DTOs/InvoiceWriteDTO.cs`  
**Tiempo Estimado**: 15 minutos

---

### **TAREA 4: Crear DTOs para NotificationMessage**

#### NotificationMessageReadDTO
```csharp
public class NotificationMessageReadDTO
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string Recipient { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public string Type { get; set; }
    public bool IsSent { get; set; }
    public DateTime? SentAt { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

#### NotificationMessageWriteDTO
```csharp
public class NotificationMessageWriteDTO
{
    [Required(ErrorMessage = "El destinatario es requerido")]
    [StringLength(255)]
    public string Recipient { get; set; }

    [StringLength(500)]
    public string Subject { get; set; }

    [Required(ErrorMessage = "El cuerpo del mensaje es requerido")]
    public string Body { get; set; }

    [Required(ErrorMessage = "El tipo de notificación es requerido")]
    [StringLength(50)]
    public string Type { get; set; }
}
```

**Ubicación**: `DTOs/NotificationMessageReadDTO.cs` y `DTOs/NotificationMessageWriteDTO.cs`  
**Tiempo Estimado**: 15 minutos

---

### **TAREA 5: Crear Validadores FluentValidation**

#### EmergencyContactValidator
```csharp
using FluentValidation;
using MedPal.API.DTOs;

namespace MedPal.API.Validation
{
    public class EmergencyContactValidator : AbstractValidator<EmergencyContactWriteDTO>
    {
        public EmergencyContactValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("El nombre es requerido")
                .Length(1, 255).WithMessage("El nombre debe tener entre 1 y 255 caracteres");

            RuleFor(x => x.Relationship)
                .NotEmpty().WithMessage("La relación es requerida")
                .Length(1, 100).WithMessage("La relación debe tener entre 1 y 100 caracteres");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("El teléfono es requerido")
                .Matches(@"^[\d\s\-\+\(\)]+$").WithMessage("Formato de teléfono inválido");

            RuleFor(x => x.Email)
                .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
                .WithMessage("Email inválido");

            RuleFor(x => x.Priority)
                .InclusiveBetween(1, 10).WithMessage("Prioridad debe estar entre 1 y 10");
        }
    }
}
```

#### PaymentValidator
```csharp
public class PaymentValidator : AbstractValidator<PaymentWriteDTO>
{
    public PaymentValidator()
    {
        RuleFor(x => x.AmountPaid)
            .GreaterThan(0).WithMessage("El monto debe ser mayor a 0")
            .ScalePrecision(2, 10).WithMessage("El monto debe tener máximo 2 decimales");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("El método de pago es requerido")
            .Must(IsValidPaymentMethod).WithMessage("Método de pago inválido");

        RuleFor(x => x.TransactionReference)
            .MaximumLength(100).WithMessage("Referencia de transacción no puede exceder 100 caracteres");
    }

    private bool IsValidPaymentMethod(string method)
    {
        return method switch
        {
            "Cash" or "CreditCard" or "BankTransfer" or "Insurance" => true,
            _ => false
        };
    }
}
```

#### InvoiceValidator
```csharp
public class InvoiceValidator : AbstractValidator<InvoiceWriteDTO>
{
    public InvoiceValidator()
    {
        RuleFor(x => x.TotalAmount)
            .GreaterThan(0).WithMessage("El monto total debe ser mayor a 0")
            .ScalePrecision(2, 10).WithMessage("El monto debe tener máximo 2 decimales");

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow)
            .When(x => x.DueDate.HasValue)
            .WithMessage("La fecha de vencimiento debe ser en el futuro");
    }
}
```

**Ubicación**: `Validation/EmergencyContactValidator.cs`, etc.  
**Tiempo Estimado**: 30 minutos

---

### **TAREA 6: Crear Controllers REST**

#### EmergencyContactController
```csharp
using MedPal.API.DTOs;
using MedPal.API.Models;
using MedPal.API.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedPal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmergencyContactController : ControllerBase
    {
        private readonly IEmergencyContactService _service;
        private readonly IMapper _mapper;
        private readonly ILogger<EmergencyContactController> _logger;

        public EmergencyContactController(
            IEmergencyContactService service,
            IMapper mapper,
            ILogger<EmergencyContactController> logger)
        {
            _service = service;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Obtener contactos de emergencia de un paciente
        /// </summary>
        [HttpGet("patient/{patientId}")]
        public async Task<ActionResult<IEnumerable<EmergencyContactReadDTO>>> GetByPatient(int patientId)
        {
            _logger.LogInformation("Obteniendo contactos de emergencia para paciente {PatientId}", patientId);
            var contacts = await _service.GetByPatientAsync(patientId);
            return Ok(_mapper.Map<List<EmergencyContactReadDTO>>(contacts));
        }

        /// <summary>
        /// Obtener un contacto de emergencia por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<EmergencyContactReadDTO>> GetById(int id)
        {
            _logger.LogInformation("Obteniendo contacto de emergencia {Id}", id);
            var contact = await _service.GetByIdAsync(id);
            if (contact == null)
                return NotFound();
            return Ok(_mapper.Map<EmergencyContactReadDTO>(contact));
        }

        /// <summary>
        /// Crear un nuevo contacto de emergencia
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<EmergencyContactReadDTO>> Create(EmergencyContactWriteDTO dto)
        {
            _logger.LogInformation("Creando contacto de emergencia para paciente {PatientId}", dto.FullName);
            var contact = _mapper.Map<EmergencyContact>(dto);
            await _service.CreateAsync(contact);
            return CreatedAtAction(nameof(GetById), new { id = contact.Id }, 
                _mapper.Map<EmergencyContactReadDTO>(contact));
        }

        /// <summary>
        /// Actualizar un contacto de emergencia
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, EmergencyContactWriteDTO dto)
        {
            _logger.LogInformation("Actualizando contacto de emergencia {Id}", id);
            var contact = await _service.GetByIdAsync(id);
            if (contact == null)
                return NotFound();

            _mapper.Map(dto, contact);
            await _service.UpdateAsync(contact);
            return NoContent();
        }

        /// <summary>
        /// Eliminar un contacto de emergencia (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Eliminando contacto de emergencia {Id}", id);
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}
```

**Ubicación**: `Controllers/EmergencyContactController.cs`  
**Tiempo Estimado**: 30 minutos

#### PaymentController (mejorado)
```csharp
using MedPal.API.DTOs;
using MedPal.API.Models;
using MedPal.API.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedPal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _service;
        private readonly IInvoiceService _invoiceService;
        private readonly IMapper _mapper;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IPaymentService service,
            IInvoiceService invoiceService,
            IMapper mapper,
            ILogger<PaymentController> logger)
        {
            _service = service;
            _invoiceService = invoiceService;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Registrar un nuevo pago
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<PaymentReadDTO>> CreatePayment(PaymentWriteDTO dto)
        {
            try
            {
                _logger.LogInformation("Creando pago para invoice {InvoiceId} por monto {Amount}",
                    dto.InvoiceId, dto.AmountPaid);

                var payment = _mapper.Map<Payment>(dto);
                await _service.CreatePaymentAsync(payment);

                // Sincronizar invoice
                var invoice = await _invoiceService.GetInvoiceByIdAsync(dto.InvoiceId);
                if (invoice != null)
                {
                    await _invoiceService.UpdatePaidAmountAsync(invoice);
                }

                return CreatedAtAction(nameof(GetById), new { id = payment.Id },
                    _mapper.Map<PaymentReadDTO>(payment));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Error al crear pago: {Error}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obtener un pago por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentReadDTO>> GetById(int id)
        {
            var payment = await _service.GetPaymentByIdAsync(id);
            if (payment == null)
                return NotFound();
            return Ok(_mapper.Map<PaymentReadDTO>(payment));
        }

        /// <summary>
        /// Obtener todos los pagos de una factura
        /// </summary>
        [HttpGet("invoice/{invoiceId}")]
        public async Task<ActionResult<IEnumerable<PaymentReadDTO>>> GetByInvoice(int invoiceId)
        {
            _logger.LogInformation("Obteniendo pagos para invoice {InvoiceId}", invoiceId);
            var payments = await _service.GetPaymentsByInvoiceAsync(invoiceId);
            return Ok(_mapper.Map<List<PaymentReadDTO>>(payments));
        }

        /// <summary>
        /// Cancelar un pago (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelPayment(int id)
        {
            try
            {
                _logger.LogInformation("Cancelando pago {Id}", id);
                await _service.CancelPaymentAsync(id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
```

**Ubicación**: `Controllers/PaymentController.cs`  
**Tiempo Estimado**: 30 minutos

#### InvoiceController (mejorado)
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvoiceController : ControllerBase
{
    private readonly IInvoiceService _service;
    private readonly IMapper _mapper;
    private readonly ILogger<InvoiceController> _logger;

    [HttpGet("{id}")]
    public async Task<ActionResult<InvoiceReadDTO>> GetById(int id)
    {
        var invoice = await _service.GetInvoiceByIdAsync(id);
        if (invoice == null) return NotFound();
        return Ok(_mapper.Map<InvoiceReadDTO>(invoice));
    }

    [HttpPost]
    public async Task<ActionResult<InvoiceReadDTO>> Create(InvoiceWriteDTO dto)
    {
        try
        {
            var invoice = _mapper.Map<Invoice>(dto);
            await _service.ValidateInvoiceAsync(invoice);
            // Guardar via context
            return CreatedAtAction(nameof(GetById), new { id = invoice.Id },
                _mapper.Map<InvoiceReadDTO>(invoice));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("patient/{patientId}")]
    public async Task<ActionResult<IEnumerable<InvoiceReadDTO>>> GetByPatient(int patientId)
    {
        var invoices = await _service.GetInvoicesByPatientAsync(patientId);
        return Ok(_mapper.Map<List<InvoiceReadDTO>>(invoices));
    }
}
```

**Ubicación**: `Controllers/InvoiceController.cs`  
**Tiempo Estimado**: 30 minutos

---

### **TAREA 7: Crear Servicios de Aplicación**

#### IEmergencyContactService
```csharp
namespace MedPal.API.Services
{
    public interface IEmergencyContactService
    {
        Task<EmergencyContact> GetByIdAsync(int id);
        Task<IEnumerable<EmergencyContact>> GetByPatientAsync(int patientId);
        Task CreateAsync(EmergencyContact contact);
        Task UpdateAsync(EmergencyContact contact);
        Task DeleteAsync(int id);
        Task<bool> ValidateAsync(EmergencyContact contact);
    }
}
```

#### EmergencyContactService
```csharp
using MedPal.API.Data;
using MedPal.API.Models;
using Microsoft.EntityFrameworkCore;

namespace MedPal.API.Services
{
    public class EmergencyContactService : IEmergencyContactService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<EmergencyContactService> _logger;

        public EmergencyContactService(AppDbContext context, ILogger<EmergencyContactService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<EmergencyContact> GetByIdAsync(int id)
        {
            return await _context.EmergencyContacts.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task<IEnumerable<EmergencyContact>> GetByPatientAsync(int patientId)
        {
            return await _context.EmergencyContacts
                .Where(x => x.PatientId == patientId && !x.IsDeleted)
                .OrderBy(x => x.Priority)
                .ToListAsync();
        }

        public async Task CreateAsync(EmergencyContact contact)
        {
            if (!await ValidateAsync(contact))
                throw new InvalidOperationException("Contacto de emergencia inválido");

            contact.CreatedAt = DateTime.UtcNow;
            contact.UpdatedAt = DateTime.UtcNow;
            _context.EmergencyContacts.Add(contact);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Contacto de emergencia creado: {Id}", contact.Id);
        }

        public async Task UpdateAsync(EmergencyContact contact)
        {
            if (!await ValidateAsync(contact))
                throw new InvalidOperationException("Contacto de emergencia inválido");

            contact.UpdatedAt = DateTime.UtcNow;
            _context.EmergencyContacts.Update(contact);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Contacto de emergencia actualizado: {Id}", contact.Id);
        }

        public async Task DeleteAsync(int id)
        {
            var contact = await GetByIdAsync(id);
            if (contact == null)
                throw new KeyNotFoundException($"Contacto {id} no encontrado");

            contact.IsDeleted = true;
            contact.DeletedAt = DateTime.UtcNow;
            await UpdateAsync(contact);
            _logger.LogInformation("Contacto de emergencia eliminado: {Id}", id);
        }

        public async Task<bool> ValidateAsync(EmergencyContact contact)
        {
            if (string.IsNullOrWhiteSpace(contact.FullName))
                return false;
            if (string.IsNullOrWhiteSpace(contact.Phone))
                return false;
            if (contact.Priority < 1 || contact.Priority > 10)
                return false;

            var patientExists = await _context.Patients.AnyAsync(x => x.Id == contact.PatientId);
            return patientExists;
        }
    }
}
```

**Ubicación**: `Services/IEmergencyContactService.cs` y `Services/EmergencyContactService.cs`  
**Tiempo Estimado**: 30 minutos

---

### **TAREA 8: Crear ExceptionHandlingMiddleware**

```csharp
using System.Net;
using System.Text.Json;

namespace MedPal.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new { error = "", statusCode = 0 };

            response = exception switch
            {
                ArgumentException => 
                    (new { error = exception.Message, statusCode = StatusCodes.Status400BadRequest }),
                InvalidOperationException => 
                    (new { error = exception.Message, statusCode = StatusCodes.Status400BadRequest }),
                KeyNotFoundException => 
                    (new { error = "Recurso no encontrado", statusCode = StatusCodes.Status404NotFound }),
                UnauthorizedAccessException => 
                    (new { error = "No autorizado", statusCode = StatusCodes.Status401Unauthorized }),
                _ => 
                    (new { error = "Error interno del servidor", statusCode = StatusCodes.Status500InternalServerError })
            };

            context.Response.StatusCode = response.statusCode;
            return context.Response.WriteAsJsonAsync(response);
        }
    }
}
```

**Ubicación**: `Middleware/ExceptionHandlingMiddleware.cs`  
**Tiempo Estimado**: 15 minutos

---

### **TAREA 9: Actualizar MappingProfile**

```csharp
using AutoMapper;
using MedPal.API.DTOs;
using MedPal.API.Models;

namespace MedPal.API.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // DTOs existentes...

            // EmergencyContact
            CreateMap<EmergencyContact, EmergencyContactReadDTO>();
            CreateMap<EmergencyContactWriteDTO, EmergencyContact>();

            // Payment
            CreateMap<Payment, PaymentReadDTO>();
            CreateMap<PaymentWriteDTO, Payment>();

            // Invoice
            CreateMap<Invoice, InvoiceReadDTO>()
                .ForMember(dest => dest.RemainingAmount, opt => opt.MapFrom(src => src.TotalAmount - src.PaidAmount))
                .ForMember(dest => dest.Payments, opt => opt.MapFrom(src => src.Payments.Where(p => !p.IsDeleted)));
            CreateMap<InvoiceWriteDTO, Invoice>();

            // NotificationMessage
            CreateMap<NotificationMessage, NotificationMessageReadDTO>();
            CreateMap<NotificationMessageWriteDTO, NotificationMessage>();
        }
    }
}
```

**Ubicación**: `Mapping/MappingProfile.cs`  
**Tiempo Estimado**: 15 minutos

---

### **TAREA 10: Registrar Servicios en Program.cs**

```csharp
// En Program.cs, agregar:

// Servicios de aplicación
builder.Services.AddScoped<IEmergencyContactService, EmergencyContactService>();

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// AutoMapper (si no está ya)
builder.Services.AddAutoMapper(typeof(MappingProfile));

// En la configuración de middleware, agregar:
app.UseMiddleware<ExceptionHandlingMiddleware>();
```

**Ubicación**: `Program.cs`  
**Tiempo Estimado**: 10 minutos

---

## 📊 RESUMEN DE ENTREGAS

| Entrega | Cantidad | Archivos |
|---------|----------|----------|
| **DTOs** | 8 | EmergencyContact (2), Payment (2), Invoice (2), Notification (2) |
| **Controllers** | 3 | EmergencyContact, Payment, Invoice |
| **Servicios** | 2 | IEmergencyContactService, EmergencyContactService |
| **Validadores** | 4 | EmergencyContact, Payment, Invoice, Notification |
| **Middleware** | 1 | ExceptionHandlingMiddleware |
| **Actualizar** | 2 | MappingProfile, Program.cs |
| **TOTAL** | **20 archivos** | |

---

## ⏱️ ESTIMADO DE TIEMPO

| Tarea | Tiempo |
|-------|--------|
| DTOs (4 tareas) | 1 hora |
| Validadores | 30 minutos |
| Controllers | 1.5 horas |
| Servicios de Aplicación | 45 minutos |
| Middleware | 15 minutos |
| Mapping + Program.cs | 25 minutos |
| Compilación & Testing | 1.5 horas |
| **TOTAL** | **~6.5 horas** |

---

## ✅ CHECKLIST DE IMPLEMENTACIÓN

- [ ] DTOs EmergencyContact creados
- [ ] DTOs Payment creados
- [ ] DTOs Invoice creados
- [ ] DTOs NotificationMessage creados
- [ ] Validadores FluentValidation creados (4)
- [ ] EmergencyContactController creado
- [ ] PaymentController mejorado
- [ ] InvoiceController mejorado
- [ ] IEmergencyContactService + Implementation creados
- [ ] ExceptionHandlingMiddleware creado
- [ ] MappingProfile actualizado
- [ ] Program.cs actualizado
- [ ] ✅ Compilación successful (0 errors)
- [ ] Verificar todos los endpoints con Postman/Swagger
- [ ] Tests básicos de DTOs
- [ ] Migración DB (NO REQUERIDA - solo API layer)

---

## 📌 NOTAS IMPORTANTES

1. **Sin migración DB**: Phase 4 NO requiere cambios a la BD, solo API layer
2. **Reutilizar servicios existentes**: InvoiceService y PaymentService ya existen
3. **Validación en dos niveles**: FluentValidation (API) + SaveChangesAsync (DB)
4. **Logging**: Todos los controllers deben loguear operaciones
5. **Autorización**: Todos los endpoints requieren [Authorize]
6. **Versionamiento**: Considerar agregar API versioning en futuro
7. **Documentación**: XML comments en todos los métodos públicos

---

**Documento Creado**: 09/Enero/2026  
**Última Actualización**: 09/Enero/2026  
**Próximo Paso**: Iniciar Phase 4 con creación de DTOs
