namespace MedPal.API.DTOs
{
    public class AppointmentReadDTO
    {
        public int Id { get; set; }
        public PatientWriteDTO Patient { get; set; }
        public int UserId { get; set; }
        public int? ClinicId { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
    }
}