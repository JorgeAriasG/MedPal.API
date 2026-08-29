using MedPal.API.Models;

namespace MedPal.API.Services
{
    /// <summary>
    /// Emite el JWT de sesión de paciente (patient_id, user_type=patient, role=Patient).
    /// Compartido por registro completo, signup y login de pacientes.
    /// </summary>
    public interface IPatientTokenService
    {
        string GeneratePatientToken(Patient patient, string email);
    }
}