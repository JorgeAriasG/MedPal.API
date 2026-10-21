using System.Collections.Generic;
using MedPal.API.Models;

namespace MedPal.API.Utils
{
    /// <summary>
    /// Utilidades puras para el cálculo de solapamiento entre bloques de disponibilidad
    /// (AvailabilityBlock) y citas/slots. Centraliza la matemática de intervalos
    /// half-open [start, end) en un solo lugar para que sea idéntica en repositorios,
    /// disponibilidad pública y del staff, y para poder probarla sin base de datos.
    /// </summary>
    public static class SlotAvailability
    {
        /// <summary>
        /// True si dos intervalos half-open [s1,e1) y [s2,e2) se solapan.
        /// Un intervalo que termina justo donde empieza el otro NO se considera solapamiento.
        /// </summary>
        public static bool IntervalsOverlap(TimeOnly s1, TimeOnly e1, TimeOnly s2, TimeOnly e2)
            => s1 < e2 && s2 < e1;

        /// <summary>
        /// True si el slot [slotStart, slotEnd) queda bloqueado por alguno de los bloques.
        /// </summary>
        public static bool IsSlotBlocked(TimeOnly slotStart, TimeOnly slotEnd, IEnumerable<AvailabilityBlock> blocks)
        {
            foreach (var block in blocks)
            {
                if (IntervalsOverlap(slotStart, slotEnd, block.StartTime, block.EndTime))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// True si el slot [slotStart, slotEnd) se solapa con alguna cita del doctor.
        /// </summary>
        public static bool IsSlotBlockedByAppointment(TimeOnly slotStart, TimeOnly slotEnd, IEnumerable<Appointment> appointments)
        {
            foreach (var appointment in appointments)
            {
                var existingEnd = appointment.Time.AddMinutes(appointment.DurationMinutes);
                if (IntervalsOverlap(slotStart, slotEnd, appointment.Time, existingEnd))
                    return true;
            }

            return false;
        }
    }
}