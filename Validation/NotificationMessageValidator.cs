using FluentValidation;
using MedPal.API.DTOs;

namespace MedPal.API.Validation
{
    /// <summary>
    /// Validador para mensajes de notificación
    /// </summary>
    public class NotificationMessageValidator : AbstractValidator<NotificationMessageWriteDTO>
    {
        public NotificationMessageValidator()
        {
            RuleFor(x => x.Recipient)
                .NotEmpty().WithMessage("El destinatario es requerido")
                .Length(1, 255).WithMessage("El destinatario debe tener entre 1 y 255 caracteres");

            RuleFor(x => x.Subject)
                .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Subject))
                .WithMessage("El asunto no puede exceder 500 caracteres");

            RuleFor(x => x.Body)
                .NotEmpty().WithMessage("El cuerpo del mensaje es requerido")
                .MinimumLength(1).WithMessage("El cuerpo no puede estar vacío");

            RuleFor(x => x.Type)
                .NotEmpty().WithMessage("El tipo de notificación es requerido")
                .Must(IsValidType).WithMessage("Tipo de notificación inválido (Email, SMS, WhatsApp, Push)");
        }

        /// <summary>
        /// Valida que el tipo de notificación sea uno de los permitidos
        /// </summary>
        private bool IsValidType(string type)
        {
            return type switch
            {
                "Email" or "SMS" or "WhatsApp" or "Push" => true,
                _ => false
            };
        }
    }
}
