namespace MedPal.API.DTOs
{
    public class MedicalHistoryReadDTO
    {
        public int Id { get; set; }
        public string SpecialtyType { get; set; }
        public string Diagnosis { get; set; }
        public DateTime DiagnosisDate { get; set; }
        public string ClinicalNotes { get; set; }
        public int? HealthcareProfessionalId { get; set; }
        public DateTime? FollowUpDate { get; set; }
        public string SpecialtyData { get; set; }
        public int? PrescriptionId { get; set; }
        // No expongas CreatedAt/UpdatedAt/LastModifiedBy a menos que sea admin
    }
}