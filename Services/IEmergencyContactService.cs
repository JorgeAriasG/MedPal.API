using MedPal.API.Models;

namespace MedPal.API.Services
{
    /// <summary>
    /// Servicio de aplicación para gestionar contactos de emergencia
    /// </summary>
    public interface IEmergencyContactService
    {
        /// <summary>
        /// Obtiene un contacto de emergencia por su ID
        /// </summary>
        /// <param name="id">ID del contacto</param>
        /// <returns>El contacto si existe, null en caso contrario</returns>
        Task<EmergencyContact> GetByIdAsync(int id);

        /// <summary>
        /// Obtiene todos los contactos de emergencia de un paciente
        /// </summary>
        /// <param name="patientId">ID del paciente</param>
        /// <returns>Lista de contactos ordenados por prioridad</returns>
        Task<IEnumerable<EmergencyContact>> GetByPatientAsync(int patientId);

        /// <summary>
        /// Crea un nuevo contacto de emergencia
        /// </summary>
        /// <param name="contact">Contacto a crear</param>
        /// <exception cref="InvalidOperationException">Si el contacto es inválido o el paciente no existe</exception>
        Task CreateAsync(EmergencyContact contact);

        /// <summary>
        /// Actualiza un contacto de emergencia existente
        /// </summary>
        /// <param name="contact">Contacto a actualizar</param>
        /// <exception cref="InvalidOperationException">Si el contacto es inválido</exception>
        Task UpdateAsync(EmergencyContact contact);

        /// <summary>
        /// Elimina (soft delete) un contacto de emergencia
        /// </summary>
        /// <param name="id">ID del contacto a eliminar</param>
        /// <exception cref="KeyNotFoundException">Si el contacto no existe</exception>
        Task DeleteAsync(int id);

        /// <summary>
        /// Valida que un contacto de emergencia sea válido
        /// </summary>
        /// <param name="contact">Contacto a validar</param>
        /// <returns>true si es válido, false en caso contrario</returns>
        Task<bool> ValidateAsync(EmergencyContact contact);
    }
}
