namespace MedPal.API.DTOs
{
    public class AllergyWriteDTO
    {
        public int PatientDetailsId { get; set; }
        public string AllergyName { get; set; }
        public string Severity { get; set; }
        public string Notes { get; set; }
    }
}
