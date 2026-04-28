namespace MedPal.API.DTOs
{
    public class TimeSlotDTO
    {
        public DateOnly Date { get; set; }
        public TimeOnly Time { get; set; }
        public bool IsAvailable { get; set; }
    }
}