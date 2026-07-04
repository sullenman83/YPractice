

using Microsoft.EntityFrameworkCore;
using TransactionManager.Abstractions;

namespace TransactionManager.Core;

/// <summary>
/// Сервис управления транзакциями
/// </summary>
public class TransactionService(DbContext context) : ITransactionService
{
    private readonly DbContext _appDbContext = context;

    /// <inheritdoc/>
    public async Task<ITransaction> BeginTransactionAsync(CancellationToken token)
    {
        return new Transaction(await _appDbContext.Database.BeginTransactionAsync(token));
    }
}
