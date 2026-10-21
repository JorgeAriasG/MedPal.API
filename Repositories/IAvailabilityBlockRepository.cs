using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MedPal.API.Models;

namespace MedPal.API.Repositories
{
    public interface IAvailabilityBlockRepository
    {
        /// <summary>
        /// Bloques activos de una clínica cuyo rango de fechas intersecta [from, to].
        /// Solo incluye bloques no eliminados.
        /// </summary>
        Task<IEnumerable<AvailabilityBlock>> GetBlocksByClinicAsync(int clinicId, DateOnly from, DateOnly to);

        Task<AvailabilityBlock> GetBlockByIdAsync(int id);

        /// <summary>
        /// Bloques activos que afectan la disponibilidad de un doctor en una fecha:
        /// bloqueos de clínica (DoctorId = null) o del propio doctor.
        /// </summary>
        Task<IEnumerable<AvailabilityBlock>> GetBlocksForAvailabilityAsync(int clinicId, int? doctorId, DateOnly date);

        /// <summary>
        /// True si [startTime, endTime) se solapa con una cita activa del doctor
        /// (o de cualquier doctor de la clínica cuando doctorId es null).
        /// </summary>
        Task<bool> HasOverlapWithAppointmentsAsync(int clinicId, int? doctorId, DateOnly date, TimeOnly startTime, TimeOnly endTime);

        /// <summary>
        /// True si [startTime, endTime) se solapa con otro bloqueo activo del mismo doctor
        /// (o de la clínica: DoctorId = null, o cualquier bloqueo si doctorId es null).
        /// </summary>
        Task<bool> HasOverlapWithBlocksAsync(int clinicId, int? doctorId, DateOnly date, TimeOnly startTime, TimeOnly endTime, int? excludeBlockId = null);

        Task<AvailabilityBlock> AddAsync(AvailabilityBlock block);

        void Update(AvailabilityBlock block);

        Task<int> CompleteAsync();
    }
}