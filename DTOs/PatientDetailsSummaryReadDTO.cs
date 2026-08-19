namespace MedPal.API.DTOs
{
    public class PatientDetailsSummaryReadDTO
    {
        public int Id { get; set; }
        public int PatientId { get; set; }

        // Antecedentes médicos del paciente (JSON)
        public string? AntecedentsData { get; set; }

        // Datos mínimos del paciente (sin historiales ni alergias)
        public PatientSummaryReadDTO Patient { get; set; }
    }

    public class PatientSummaryReadDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Middlename { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Gender { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Height { get; set; }
    }
}
