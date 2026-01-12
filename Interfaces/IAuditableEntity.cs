namespace MedPal.API.Interfaces
{
    /// <summary>
    /// Interface para implementar auditoría consistente de creación y modificación.
    /// Permite rastrear quién creó y modificó cada registro, así como cuándo ocurrieron esos cambios.
    /// </summary>
    public interface IAuditableEntity
    {
        /// <summary>
        /// Fecha y hora en que se creó el registro.
        /// </summary>
        DateTime CreatedAt { get; set; }

        /// <summary>
        /// ID del usuario que creó el registro.
        /// </summary>
        int? CreatedByUserId { get; set; }

        /// <summary>
        /// Fecha y hora en que se actualizó por última vez el registro.
        /// </summary>
        DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// ID del usuario que actualizó por última vez el registro.
        /// </summary>
        int? UpdatedByUserId { get; set; }

        /// <summary>
        /// Fecha y hora en que se modificó por última vez el registro (alias para UpdatedAt).
        /// Útil para cambios específicos de dominio.
        /// </summary>
        DateTime? LastModifiedAt { get; set; }

        /// <summary>
        /// ID del usuario que realizó la última modificación (alias para UpdatedByUserId).
        /// Útil para cambios específicos de dominio.
        /// </summary>
        int? LastModifiedByUserId { get; set; }
    }
}
