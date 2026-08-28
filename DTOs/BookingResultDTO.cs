namespace MedPal.API.DTOs
{
    public class BookingResultDTO
    {
        public int AppointmentId { get; set; }
        public bool PendingRegistration { get; set; }
        public string? Message { get; set; }
    }
}