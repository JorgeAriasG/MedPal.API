namespace MedPal.API.DTOs
{
    public class PatientDetailsReadDTO
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        // You can include other properties if needed
    }
}
