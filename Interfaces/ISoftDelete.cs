namespace MedPal.API.Interfaces
{
    /// <summary>
    /// Interface para implementar soft delete pattern.
    /// Permite marcar registros como eliminados sin borrarlos físicamente de la base de datos.
    /// Importante para auditoría y cumplimiento normativo.
    /// </summary>
    public interface ISoftDelete
    {
        /// <summary>
        /// Indica si el registro ha sido eliminado (soft delete).
        /// </summary>
        bool IsDeleted { get; set; }

        /// <summary>
        /// Fecha y hora en que se eliminó el registro (soft delete).
        /// Null si el registro no ha sido eliminado.
        /// </summary>
        DateTime? DeletedAt { get; set; }

        /// <summary>
        /// ID del usuario que eliminó el registro.
        /// Null si el registro no ha sido eliminado.
        /// </summary>
        int? DeletedByUserId { get; set; }
    }
}
