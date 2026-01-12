namespace MedPal.API.Services
{
    public interface IQrCodeService
    {
        byte[] GenerateQrCode(string content);
    }
}
