namespace MedPal.API.DTOs
{
    public class MedicalHistoryReadDTO
    {
        public int Id { get; set; }
        public string SpecialtyType { get; set; }
        public string Diagnosis { get; set; }
        public DateTime DiagnosisDate { get; set; }
        public string TreatmentPlan { get; set; }
        public string ClinicalNotes { get; set; }
        public int? HealthcareProfessionalId { get; set; }
        public string TreatmentStatus { get; set; }
        public DateTime? FollowUpDate { get; set; }
        // No expongas CreatedAt/UpdatedAt/LastModifiedBy a menos que sea admin
    }
}