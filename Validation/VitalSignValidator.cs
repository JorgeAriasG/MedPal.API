using FluentValidation;
using MedPal.API.DTOs;

namespace MedPal.API.Validation
{
    public class VitalSignValidator : AbstractValidator<VitalSignWriteDTO>
    {
        public VitalSignValidator()
        {
            RuleFor(x => x.PatientDetailsId)
                .NotEmpty().WithMessage("El ID de PatientDetails es requerido")
                .GreaterThan(0).WithMessage("El ID de PatientDetails debe ser mayor a 0");

            RuleFor(x => x.SystolicBP)
                .InclusiveBetween(0, 300).When(x => x.SystolicBP.HasValue)
                .WithMessage("Presión sistólica debe estar entre 0 y 300 mmHg");

            RuleFor(x => x.DiastolicBP)
                .InclusiveBetween(0, 200).When(x => x.DiastolicBP.HasValue)
                .WithMessage("Presión diastólica debe estar entre 0 y 200 mmHg");

            RuleFor(x => x.HeartRate)
                .InclusiveBetween(0, 300).When(x => x.HeartRate.HasValue)
                .WithMessage("Frecuencia cardíaca debe estar entre 0 y 300 lpm");

            RuleFor(x => x.Temperature)
                .InclusiveBetween(32m, 45m).When(x => x.Temperature.HasValue)
                .WithMessage("Temperatura debe estar entre 32 y 45 °C");

            RuleFor(x => x.RespiratoryRate)
                .InclusiveBetween(0, 100).When(x => x.RespiratoryRate.HasValue)
                .WithMessage("Frecuencia respiratoria debe estar entre 0 y 100 rpm");

            RuleFor(x => x.OxygenSaturation)
                .InclusiveBetween(0, 100).When(x => x.OxygenSaturation.HasValue)
                .WithMessage("Saturación de oxígeno debe estar entre 0 y 100%");

            RuleFor(x => x.Weight)
                .InclusiveBetween(0m, 500m).When(x => x.Weight.HasValue)
                .WithMessage("Peso debe estar entre 0 y 500 kg");

            RuleFor(x => x.Height)
                .InclusiveBetween(0m, 300m).When(x => x.Height.HasValue)
                .WithMessage("Altura debe estar entre 0 y 300 cm");

            RuleFor(x => x.Bmi)
                .InclusiveBetween(5m, 100m).When(x => x.Bmi.HasValue)
                .WithMessage("IMC debe estar entre 5 y 100");

            RuleFor(x => x.BloodGlucose)
                .InclusiveBetween(0, 1000).When(x => x.BloodGlucose.HasValue)
                .WithMessage("Glucosa debe estar entre 0 y 1000 mg/dL");

            RuleFor(x => x.Notes)
                .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Notes))
                .WithMessage("Las notas no pueden exceder 500 caracteres");

            RuleFor(x => x.RecordedAt)
                .NotEmpty().WithMessage("La fecha de registro es requerida");
        }
    }
}
