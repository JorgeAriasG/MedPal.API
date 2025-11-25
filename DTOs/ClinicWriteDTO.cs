namespace MedPal.API.DTOs
{
    public class ClinicWriteDTO
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public string ContactInfo { get; set; }
        public TimeOnly Open { get; set; }
        public TimeOnly Close { get; set; }
    }
}