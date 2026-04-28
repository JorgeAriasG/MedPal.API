namespace MedPal.API.DTOs
{
    public class LoginResponseDTO
    {
        public UserReadDTO User { get; set; }
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}