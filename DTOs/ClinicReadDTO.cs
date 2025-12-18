namespace MedPal.API.DTOs
{
    public class ClinicReadDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string ContactInfo { get; set; }
        public TimeOnly Open { get; set; }
        public TimeOnly Close { get; set; }
    }
}