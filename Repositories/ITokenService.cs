using MedPal.API.Models;

namespace MedPal.API.Repositories
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}