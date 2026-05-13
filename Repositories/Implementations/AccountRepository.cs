using System.Threading.Tasks;
using MedPal.API.Data;
using MedPal.API.Models;

namespace MedPal.API.Repositories.Implementations
{
    public class AccountRepository : Repository<Account>, IAccountRepository
    {
        public AccountRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Account> CreateAccountAsync(Account account)
        {
            await _dbSet.AddAsync(account);
            await _context.SaveChangesAsync();
            return account;
        }

        public async Task<Account?> GetAccountByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }
    }
}
