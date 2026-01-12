using FluentValidation;
using MedPal.API.DTOs;

namespace MedPal.API.Validation
{
    /// <summary>
    /// Validador para pagos
    /// </summary>
    public class PaymentValidator : AbstractValidator<PaymentWriteDTO>
    {
        public PaymentValidator()
        {
            RuleFor(x => x.InvoiceId)
                .GreaterThan(0).WithMessage("El invoice ID debe ser válido");

            RuleFor(x => x.AmountPaid)
                .GreaterThan(0).WithMessage("El monto debe ser mayor a 0")
                .PrecisionScale(10, 2, ignoreTrailingZeros: true)
                .WithMessage("El monto debe tener máximo 2 decimales");

            RuleFor(x => x.PaymentMethod)
                .NotEmpty().WithMessage("El método de pago es requerido")
                .Must(IsValidPaymentMethod).WithMessage("Método de pago inválido (Cash, CreditCard, BankTransfer, Insurance)");

            RuleFor(x => x.TransactionReference)
                .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.TransactionReference))
                .WithMessage("Referencia de transacción no puede exceder 100 caracteres");

            RuleFor(x => x.PaymentDate)
                .NotEmpty().WithMessage("La fecha de pago es requerida");

            RuleFor(x => x.Notes)
                .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Notes))
                .WithMessage("Las notas no pueden exceder 500 caracteres");
        }

        /// <summary>
        /// Valida que el método de pago sea uno de los permitidos
        /// </summary>
        private bool IsValidPaymentMethod(string method)
        {
            return method switch
            {
                "Cash" or "CreditCard" or "BankTransfer" or "Insurance" => true,
                _ => false
            };
        }
    }
}
