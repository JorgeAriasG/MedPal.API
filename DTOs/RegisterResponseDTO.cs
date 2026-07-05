namespace MedPal.API.DTOs
{
    public class RegisterResponseDTO
    {
        public UserReadDTO User { get; set; }
        public string? CheckoutUrl { get; set; }
        public string? SessionId { get; set; }
    }
}
