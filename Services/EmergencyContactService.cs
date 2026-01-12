using MedPal.API.Data;
using MedPal.API.Models;
using Microsoft.EntityFrameworkCore;

namespace MedPal.API.Services
{
    /// <summary>
    /// Implementación del servicio de contactos de emergencia
    /// </summary>
    public class EmergencyContactService : IEmergencyContactService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<EmergencyContactService> _logger;

        public EmergencyContactService(AppDbContext context, ILogger<EmergencyContactService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene un contacto de emergencia por su ID (excluye eliminados)
        /// </summary>
        public async Task<EmergencyContact> GetByIdAsync(int id)
        {
            _logger.LogInformation("Buscando contacto de emergencia con ID {Id}", id);
            return await _context.EmergencyContacts
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        /// <summary>
        /// Obtiene todos los contactos activos de un paciente, ordenados por prioridad
        /// </summary>
        public async Task<IEnumerable<EmergencyContact>> GetByPatientAsync(int patientId)
        {
            _logger.LogInformation("Buscando contactos de emergencia para paciente {PatientId}", patientId);
            return await _context.EmergencyContacts
                .Where(x => x.PatientId == patientId && !x.IsDeleted && x.IsActive)
                .OrderBy(x => x.Priority)
                .ToListAsync();
        }

        /// <summary>
        /// Crea un nuevo contacto de emergencia
        /// </summary>
        public async Task CreateAsync(EmergencyContact contact)
        {
            if (contact == null)
                throw new ArgumentNullException(nameof(contact));

            if (!await ValidateAsync(contact))
                throw new InvalidOperationException("El contacto de emergencia es inválido");

            contact.CreatedAt = DateTime.UtcNow;
            contact.UpdatedAt = DateTime.UtcNow;
            contact.IsActive = true;
            contact.IsDeleted = false;

            _context.EmergencyContacts.Add(contact);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Contacto de emergencia creado: {Id} para paciente {PatientId}", 
                contact.Id, contact.PatientId);
        }

        /// <summary>
        /// Actualiza un contacto de emergencia existente
        /// </summary>
        public async Task UpdateAsync(EmergencyContact contact)
        {
            if (contact == null)
                throw new ArgumentNullException(nameof(contact));

            if (!await ValidateAsync(contact))
                throw new InvalidOperationException("El contacto de emergencia es inválido");

            contact.UpdatedAt = DateTime.UtcNow;
            _context.EmergencyContacts.Update(contact);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Contacto de emergencia actualizado: {Id}", contact.Id);
        }

        /// <summary>
        /// Elimina (soft delete) un contacto de emergencia
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            var contact = await GetByIdAsync(id);
            if (contact == null)
            {
                _logger.LogWarning("Intento de eliminar contacto inexistente: {Id}", id);
                throw new KeyNotFoundException($"El contacto de emergencia con ID {id} no fue encontrado");
            }

            contact.IsDeleted = true;
            contact.IsActive = false;
            contact.DeletedAt = DateTime.UtcNow;
            await UpdateAsync(contact);
            _logger.LogInformation("Contacto de emergencia eliminado: {Id}", id);
        }

        /// <summary>
        /// Valida que un contacto de emergencia sea válido
        /// Verifica que:
        /// - Nombre no esté vacío
        /// - Teléfono no esté vacío
        /// - Prioridad esté entre 1 y 10
        /// - El paciente exista
        /// </summary>
        public async Task<bool> ValidateAsync(EmergencyContact contact)
        {
            if (contact == null)
                return false;

            if (string.IsNullOrWhiteSpace(contact.FullName))
            {
                _logger.LogWarning("Validación fallida: Nombre vacío");
                return false;
            }

            if (string.IsNullOrWhiteSpace(contact.Phone))
            {
                _logger.LogWarning("Validación fallida: Teléfono vacío");
                return false;
            }

            if (contact.Priority < 1 || contact.Priority > 10)
            {
                _logger.LogWarning("Validación fallida: Prioridad inválida {Priority}", contact.Priority);
                return false;
            }

            var patientExists = await _context.Patients
                .AnyAsync(x => x.Id == contact.PatientId && !x.IsDeleted);
            
            if (!patientExists)
            {
                _logger.LogWarning("Validación fallida: Paciente {PatientId} no existe o fue eliminado", contact.PatientId);
                return false;
            }

            return true;
        }
    }
}
