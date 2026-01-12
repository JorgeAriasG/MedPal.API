using System.Collections.Generic;
using System.Threading.Tasks;
using MedPal.API.Models;

namespace MedPal.API.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> GetUserByIdAsync(int id);
        Task<User> GetUserByEmailAsync(string email);
        Task<User> AddUserAsync(User user);
        Task UpdateUserAsync(int id, User user);
        Task DeleteUserAsync(int id);
        Task<User?> ValidateUserAsync(string email, string password);
        Task SoftDeleteUserAsync(int userId, int deletedByUserId);
        Task RestoreUserAsync(int userId);
        Task UpdateUserLastAccessAtAsync(int userId);
    }
}