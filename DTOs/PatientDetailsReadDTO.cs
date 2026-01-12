namespace MedPal.API.DTOs
{
    public class PatientDetailsReadDTO
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Patient demographics
        public PatientReadDTO Patient { get; set; }
        
        // Medical information
        public ICollection<MedicalHistoryReadDTO> MedicalHistories { get; set; }
        public ICollection<AllergyReadDTO> Allergies { get; set; }
    }
}
