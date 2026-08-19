namespace MedPal.API.DTOs
{
    public class MedicalHistorySummaryReadDTO
    {
        public int Id { get; set; }
        public string SpecialtyType { get; set; }
        public string Diagnosis { get; set; }
        public string ClinicalNotes { get; set; }
        public int? HealthcareProfessionalId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
