using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using MedPal.API.Data;
using MedPal.API.Models;
using Microsoft.EntityFrameworkCore;

namespace MedPal.API.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public UserRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with Id {id} not found.");
            }
            return user;
        }

        public async Task<User> AddUserAsync(User user)
        {
            // Hash the password before saving the user
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task UpdateUserAsync(int id, User user)
        {
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                throw new KeyNotFoundException($"User with Id {id} not found.");
            }
            _mapper.Map(user, existingUser);
            _context.Users.Update(existingUser);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with Id {id} not found.");
            }
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task<User?> ValidateUserAsync(string email, string password)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .SingleOrDefaultAsync(u => u.Email == email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return null;
            }
            return user;
        }

        // Soft delete: marcar usuario como eliminado sin borrar física
        public async Task SoftDeleteUserAsync(int userId, int deletedByUserId)
        {
            var user = await GetUserByIdAsync(userId);

            user.IsDeleted = true;
            user.IsActive = false;
            user.DeletedAt = DateTime.UtcNow;
            user.DeactivatedByUserId = deletedByUserId;
            user.UpdatedAt = DateTime.UtcNow;

            await UpdateUserAsync(userId, user);
        }

        // Restaurar usuario (si es necesario)
        public async Task RestoreUserAsync(int userId)
        {
            var user = await GetUserByIdAsync(userId);

            user.IsDeleted = false;
            user.IsActive = true;
            user.DeletedAt = null;
            user.DeactivatedByUserId = null;
            user.UpdatedAt = DateTime.UtcNow;

            await UpdateUserAsync(userId, user);
        }

        public async Task UpdateUserLastAccessAtAsync(int userId)
        {
            var user = await GetUserByIdAsync(userId);
            user.LastAccessAt = DateTime.UtcNow;
            await UpdateUserAsync(userId, user);
        }
    }
}