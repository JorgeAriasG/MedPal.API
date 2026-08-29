using System.Threading;
using System.Threading.Tasks;

namespace MedPal.API.Repositories
{
    /// <summary>
    /// Límite de transacción de la capa de aplicación. La implementación vive en
    /// infraestructura (EF Core); las operaciones de un caso de uso se cometen en un
    /// solo <see cref="CompleteAsync"/>. Prohibido inyectar AppDbContext en controllers.
    /// </summary>
    public interface IUnitOfWork
    {
        bool HasActiveTransaction { get; }

        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Persiste los cambios pendientes del contexto y confirma la transacción activa.
        /// </summary>
        Task<int> CompleteAsync(CancellationToken cancellationToken = default);

        Task RollbackAsync(CancellationToken cancellationToken = default);
    }
}