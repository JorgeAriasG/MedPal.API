using FluentValidation;
using MedPal.API.DTOs;

namespace MedPal.API.Validation
{
    /// <summary>
    /// Validador para facturas
    /// </summary>
    public class InvoiceValidator : AbstractValidator<InvoiceWriteDTO>
    {
        public InvoiceValidator()
        {
            RuleFor(x => x.PatientId)
                .GreaterThan(0).WithMessage("El paciente ID debe ser válido");

            RuleFor(x => x.AppointmentId)
                .GreaterThan(0).WithMessage("El appointment ID debe ser válido");

            RuleFor(x => x.TotalAmount)
                .GreaterThan(0).WithMessage("El monto total debe ser mayor a 0")
                .PrecisionScale(10, 2, ignoreTrailingZeros: true)
                .WithMessage("El monto debe tener máximo 2 decimales");

            RuleFor(x => x.DueDate)
                .GreaterThan(DateTime.UtcNow).When(x => x.DueDate.HasValue)
                .WithMessage("La fecha de vencimiento debe ser en el futuro");
        }
    }
}
