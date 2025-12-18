namespace MedPal.API.Repositories
{
    public interface IUserService
    {
        string UserId { get; }
        string Role { get; set; }
        string Username { get; set; }
    }
}