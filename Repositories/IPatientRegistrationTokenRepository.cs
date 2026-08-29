using System.Collections.Generic;
using System.Threading.Tasks;
using MedPal.API.Models;

namespace MedPal.API.Repositories
{
    public interface IPatientRegistrationTokenRepository
    {
        /// <summary>
        /// Registrar (trackear) un token sin persistir; la persistencia la decide el
        /// IUnitOfWork del caso de uso.
        /// </summary>
        Task<PatientRegistrationToken> CreateAsync(PatientRegistrationToken token);

        Task<PatientRegistrationToken?> GetByHashAsync(string tokenHash);

        Task<IEnumerable<PatientRegistrationToken>> GetPendingByPatientIdAsync(int patientId);

        Task UpdateAsync(PatientRegistrationToken token);

        /// <summary>
        /// Consume atómicamente un token pendiente (pending → used). Devuelve 1 si el
        /// consumo se realizó, 0 si ya no estaba pendiente. Ejecuta SQL condicional,
        /// por lo que participa en la transacción activa del IUnitOfWork.
        /// </summary>
        Task<int> ConsumeAsync(string tokenHash);

        /// <summary>
        /// Revoca un token pendiente específico (pending → revoked), p. ej. por expiración.
        /// </summary>
        Task<int> RevokeAsync(string tokenHash);

        /// <summary>
        /// Revoca todos los tokens pendientes de un paciente (pending → revoked),
        /// usado antes de emitir un reenvío.
        /// </summary>
        Task<int> RevokePendingByPatientAsync(int patientId);
    }
}