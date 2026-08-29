using System;

namespace MedPal.API.Exceptions
{
    /// <summary>
    /// Indica que el principal autenticado no tiene permiso de ejecutar la operación
    /// (HTTP 403). Distinto de UnauthorizedAccessException (401).
    /// </summary>
    public class ForbiddenAccessException : Exception
    {
        public ForbiddenAccessException()
        {
        }

        public ForbiddenAccessException(string message)
            : base(message)
        {
        }

        public ForbiddenAccessException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}