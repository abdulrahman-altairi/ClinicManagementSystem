using System.Data;

namespace ClinicManagementSystem.Application.Common.Interfaces;

public interface IUnitOfWork
{
    IDbTransaction? Transaction { get; }

    Task<IDbConnection> GetConnectionAsync(CancellationToken cancellationToken = default);

    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    Task CommitAsync(CancellationToken cancellationToken = default);

    Task RollbackAsync(CancellationToken cancellationToken = default);

    Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default);
}
