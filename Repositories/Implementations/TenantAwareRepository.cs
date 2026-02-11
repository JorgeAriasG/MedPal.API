using MedPal.API.Data;
using MedPal.API.Services;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MedPal.API.Repositories.Implementations
{
    public abstract class TenantAwareRepository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly ITenantContextService _tenantContext;
        protected readonly DbSet<T> _dbSet;

        protected TenantAwareRepository(AppDbContext context, ITenantContextService tenantContext)
        {
            _context = context;
            _tenantContext = tenantContext;
            _dbSet = _context.Set<T>();
        }

        protected IQueryable<T> ApplyTenantFilter(IQueryable<T> query)
        {
            // Aquí SÍ hay HttpContext
            if (_tenantContext.IsSuperAdmin)
                return query;

            // Filtra automáticamente por tenant si T tiene AccountId
            if (typeof(T).GetProperty("AccountId") != null)
            {
                return query.Where(e => EF.Property<int>(e, "AccountId") == _tenantContext.CurrentAccountId);
            }

            return query;
        }

        // Implementar interfaz IRepository<T>
        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await ApplyTenantFilter(_dbSet).ToListAsync();
        }

        public virtual async Task<T> GetByIdAsync(int id)
        {
            return await ApplyTenantFilter(_dbSet).FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id);
        }

        public virtual async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}