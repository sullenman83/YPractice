using EventManagement.Interfaces.Services;
using Microsoft.EntityFrameworkCore.Storage;

namespace EventManagement.Services.TransactionService;

/// <summary>
/// Обертк  над IDbContexttransaction
/// </summary>
/// <param name="transaction">Транзакция</param>
public class Transaction(IDbContextTransaction transaction): ITransaction
{
    private IDbContextTransaction? _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
    private bool _isDisposed = false;

    /// <inheritdoc/>    
    public async Task CommitAsync(CancellationToken token = default)
    {
        if (_transaction == null)
            throw new InvalidOperationException("Транзакция уже закрыта");
        await _transaction.CommitAsync();
    }

    /// <inheritdoc/>
    public async Task RollbackAsync(CancellationToken token = default)
    {
        if (_transaction == null)
            throw new InvalidOperationException("Транзакция уже закрыта");

        await _transaction.RollbackAsync();
    }

    /// <summary>
    /// Очистить ресурсы
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed || _transaction == null)
            return;

        _isDisposed = true;

        _transaction.Dispose();
    }

    /// <summary>
    /// Очистить ресурсы
    /// </summary>
    /// <returns></returns>
    public async ValueTask DisposeAsync()
    {
        if (_isDisposed || _transaction == null)
            return;

        _isDisposed = true;

        await _transaction.DisposeAsync();
    }
}
