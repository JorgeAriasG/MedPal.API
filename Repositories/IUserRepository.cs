using System.Collections.Generic;
using System.Threading.Tasks;
using MedPal.API.DTOs;
using MedPal.API.Models;

namespace MedPal.API.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> GetUserByIdAsync(int id);
        Task<User?> GetByIdIgnoreTenantAsync(int id);
        Task<User> GetOwnProfileAsync(int userId);
        Task<User> GetUserByEmailAsync(string email);
        Task<User> AddUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(int id);
        Task<User?> ValidateUserAsync(string email, string password);
        Task SoftDeleteUserAsync(int userId, int deletedByUserId);
        Task RestoreUserAsync(int userId);
        Task UpdateUserLastAccessAtAsync(int userId);
        Task<List<string>> GetUserRolesAsync(int userId);
        Task<Clinic?> GetUserClinicAsync(int userId);
        Task<List<User>> GetAllUsersByAccountId(int accountId);
        Task<List<User>> GetDoctorsByClinicAsync(int clinicId);
    }
}
