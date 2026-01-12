namespace MedPal.API.Enums
{
    /// <summary>
    /// Estados de tareas/pendientes
    /// </summary>
    public enum TaskStatus
    {
        /// <summary>Tarea pendiente (no iniciada)</summary>
        Pending = 0,

        /// <summary>Tarea en progreso</summary>
        InProgress = 1,

        /// <summary>Tarea completada</summary>
        Completed = 2,

        /// <summary>Tarea cancelada</summary>
        Cancelled = 3,

        /// <summary>Tarea bloqueada/en espera</summary>
        OnHold = 4
    }
}
