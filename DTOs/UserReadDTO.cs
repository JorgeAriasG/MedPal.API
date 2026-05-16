namespace MedPal.API.DTOs
{
    public class UserReadDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public int ClinicId { get; set; }
        public string Token { get; set; }
    }
}