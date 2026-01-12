namespace MedPal.API.DTOs
{
    public class AllergyReadDTO
    {
        public int Id { get; set; }
        public int PatientDetailsId { get; set; }
        public string AllergyName { get; set; }
        public string Severity { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
