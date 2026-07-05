namespace MedPal.API.Enums
{
    /// <summary>
    /// Enumeración de roles de sistema.
    /// Define la jerarquía de permisos en toda la aplicación.
    /// </summary>
    public enum SystemRole
    {
        /// <summary>
        /// Administrador del sistema completo.
        /// Acceso a todo excepto Medical Records por normativa NOM-004.
        /// </summary>
        SuperAdmin = 1,

        /// <summary>
        /// Administrador de cuenta.
        /// Acceso a todas las clínicas y usuarios de su cuenta.
        /// </summary>
        AccountAdmin = 2,

        /// <summary>
        /// Administrador de clínica (opcional).
        /// Acceso a personal y usuarios de su clínica específica.
        /// </summary>
        ClinicAdmin = 3,

        /// <summary>
        /// Profesional de salud (médicos, nutricionistas, terapeutas, etc.).
        /// Rol clínico único consolidado. El acceso a datos se filtra por asignación (MedicalHistory.HealthcareProfessionalId)
        /// y por especialidad (User.Specialty) según el módulo correspondiente.
        /// </summary>
        HealthProfessional = 4,

        /// <summary>
        /// Recepcionista o personal administrativo.
        /// Acceso a datos de contacto y citas, NO a Medical Records.
        /// </summary>
        Receptionist = 6,

        /// <summary>
        /// Paciente.
        /// Acceso a sus propios registros y gestión de consentimientos.
        /// </summary>
        Patient = 7
    }
}
