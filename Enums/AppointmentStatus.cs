namespace MedPal.API.Enums
{
    /// <summary>
    /// Estados de cita médica
    /// </summary>
    public enum AppointmentStatus
    {
        /// <summary>Cita agendada, pendiente</summary>
        Scheduled = 0,

        /// <summary>Cita en progreso (paciente en consulta)</summary>
        InProgress = 1,

        /// <summary>Cita completada exitosamente</summary>
        Completed = 2,

        /// <summary>Cita cancelada por el paciente o profesional (se puede reagendar)</summary>
        Cancelled = 3,

        /// <summary>Paciente no asistió a la cita</summary>
        NoShow = 4,

        /// <summary>Cita fue reagendada (la original está cerrada)</summary>
        Rescheduled = 5
    }
}
