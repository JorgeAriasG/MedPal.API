using System;

namespace MedPal.API.Utils
{
    /// <summary>
    /// Resuelve el email del paciente. Cuando no se proporciona uno, asigna un
    /// placeholder único e inalcanzable (dominio .temp) para respetar la unicidad
    /// del índice sin requerir email real. El paciente puede completarlo después.
    /// </summary>
    public static class PatientEmailResolver
    {
        public static string Resolve(string? email)
        {
            if (!string.IsNullOrWhiteSpace(email))
                return email.Trim().ToLower();

            return $"pendiente_{Guid.NewGuid():N}@clinicflow.temp";
        }
    }
}