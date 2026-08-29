using System.Collections.Generic;
using System.Threading.Tasks;
using MedPal.API.DTOs;

namespace MedPal.API.Services
{
    /// <summary>
    /// Casos de uso de reserva pública de citas (T02c): completar booking, disponibilidad
    /// y generación del link de reserva por staff. No contiene detalles de HTTP ni EF.
    /// </summary>
    public interface IBookingService
    {
        Task<BookingResultDTO> CompleteBookingAsync(int? authPatientId, string? shareToken, BookingCompleteDTO dto);

        Task<IEnumerable<TimeSlotDTO>> GetPublicAvailabilityAsync(
            string? shareToken, int? clinicId, int? doctorId, DateOnly date, int? authPatientId);

        Task<BookingLinkDTO> GenerateStaffLinkAsync(int userId, BookingLinkStaffDTO dto);
    }
}