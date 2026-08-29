using System.Threading;
using System.Threading.Tasks;
using MedPal.API.Data;
using MedPal.API.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace MedPal.API.Infrastructure
{
    /// <summary>
    /// Unit of Work sobre el AppDbContext scoped. Todos los repositorios comparten la
    /// misma instancia por request, por lo que sus SaveChanges participan en la
    /// transacción explícita iniciada aquí.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public bool HasActiveTransaction => _transaction != null;

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
                return;

            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task<int> CompleteAsync(CancellationToken cancellationToken = default)
        {
            var saved = await _context.SaveChangesAsync(cancellationToken);

            if (_transaction != null)
            {
                await _transaction.CommitAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }

            return saved;
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction == null)
                return;

            try
            {
                await _transaction.RollbackAsync(cancellationToken);
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }
}