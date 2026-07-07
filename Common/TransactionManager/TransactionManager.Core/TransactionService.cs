

using Microsoft.EntityFrameworkCore;
using TransactionManager.Abstractions;

namespace TransactionManager.Core;

/// <summary>
/// Сервис управления транзакциями
/// </summary>
public class TransactionService<TContext>(TContext context) : ITransactionService where TContext : DbContext
{
    private readonly TContext _appDbContext = context;

    /// <inheritdoc/>
    public async Task<ITransaction> BeginTransactionAsync(CancellationToken token)
    {
        return new Transaction(await _appDbContext.Database.BeginTransactionAsync(token));
    }
}
