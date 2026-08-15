using System.Data;
using ClinicManagementSystem.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace ClinicManagementSystem.Infrastructure.Persistence.Transactions;

public sealed class AdoNetUnitOfWork : IUnitOfWork
{
    private readonly IDbConnectionFactory       _factory;
    private readonly ILogger<AdoNetUnitOfWork>  _logger;

    private IDbConnection?  _connection;
    private IDbTransaction? _transaction;
    private bool            _disposed;

    public AdoNetUnitOfWork(IDbConnectionFactory factory, ILogger<AdoNetUnitOfWork> logger)
    {
        _factory = factory;
        _logger  = logger;
    }


    public IDbTransaction? Transaction => _transaction;

    public async Task<IDbConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (_connection is null || _connection.State != ConnectionState.Open)
        {
            _connection?.Dispose();
            _connection = await _factory.CreateOpenConnectionAsync(cancellationToken);
        }

        return _connection;
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (_transaction is not null)
            throw new InvalidOperationException(
                "A transaction is already active. Commit or rollback it before beginning a new one.");

        var connection = await GetConnectionAsync(cancellationToken);
        _transaction   = connection.BeginTransaction(IsolationLevel.ReadCommitted);

        _logger.LogDebug("Database transaction started.");
    }

    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        EnsureActiveTransaction();

        try
        {
            _transaction!.Commit();
            _logger.LogDebug("Database transaction committed.");
        }
        finally
        {
            ResetTransaction();
        }

        return Task.CompletedTask;
    }

    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        EnsureActiveTransaction();

        try
        {
            _transaction!.Rollback();
            _logger.LogDebug("Database transaction rolled back.");
        }
        finally
        {
            ResetTransaction();
        }

        return Task.CompletedTask;
    }


    public async Task ExecuteInTransactionAsync(
        Func<Task> operation, CancellationToken cancellationToken = default)
    {
        await BeginTransactionAsync(cancellationToken);
        try
        {
            await operation();
            await CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction operation failed. Rolling back.");
            await RollbackAsync(cancellationToken);
            throw;
        }
    }


    public void Dispose()
    {
        if (_disposed) return;

        _transaction?.Dispose();
        _connection?.Dispose();

        _transaction = null;
        _connection  = null;
        _disposed    = true;

        _logger.LogDebug("AdoNetUnitOfWork disposed synchronously.");
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        if (_transaction is not null)
        {
            _transaction.Dispose();
            _transaction = null;
        }

        if (_connection is IAsyncDisposable asyncConnection)
            await asyncConnection.DisposeAsync();
        else
            _connection?.Dispose();

        _connection = null;
        _disposed   = true;

        _logger.LogDebug("AdoNetUnitOfWork disposed asynchronously.");
    }


    private void ResetTransaction()
    {
        _transaction?.Dispose();
        _transaction = null;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(AdoNetUnitOfWork));
    }

    private void EnsureActiveTransaction()
    {
        if (_transaction is null)
            throw new InvalidOperationException("No active transaction. Call BeginTransactionAsync first.");
    }
}   