using MedPal.API.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MedPal.API.Services
{
    public interface IUserManagementService
    {
        Task<UserReadDTO> RegisterUserAsync(UserRegisterDTO request);
        Task<LoginResponseDTO> LoginAsync(UserLoginDTO request);
        Task<UserReadDTO> GetUserByIdAsync(int id);
        Task<IEnumerable<UserReadDTO>> GetAllUsersAsync();
        Task<UserReadDTO> UpdateUserAsync(int id, UserUpdateDTO request);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> ChangePasswordAsync(int id, ChangePasswordDTO request);
        Task<bool> AssignRoleToUserAsync(int userId, int roleId, int? accountId = null);
    }
}