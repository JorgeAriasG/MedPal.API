namespace MedPal.API.DTOs
{
    public class UserWriteDTO
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public int? ClinicId { get; set; }
    }
}