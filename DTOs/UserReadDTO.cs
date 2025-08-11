namespace MedPal.API.DTOs
{
    public class UserReadDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public int DefaultClinicId { get; set; }
    }
}