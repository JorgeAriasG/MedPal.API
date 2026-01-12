namespace MedPal.API.Enums
{
    /// <summary>
    /// Tipos de reportes médicos
    /// </summary>
    public enum ReportType
    {
        /// <summary>Reporte clínico de consulta</summary>
        Clinical = 0,

        /// <summary>Reporte de diagnóstico</summary>
        Diagnostic = 1,

        /// <summary>Reporte de imagenología (radiografía, resonancia, etc.)</summary>
        Imaging = 2,

        /// <summary>Reporte de laboratorio (análisis, pruebas)</summary>
        Laboratory = 3,

        /// <summary>Reporte ARCO (Acceso, Rectificación, Cancelación, Oposición)</summary>
        ARCO = 4,

        /// <summary>Reporte de referencia a otro especialista</summary>
        ReferralLetter = 5,

        /// <summary>Reporte de alta médica</summary>
        Discharge = 6
    }
}
