using EventManagement.Data;
using EventManagement.Interfaces.Services;

namespace EventManagement.Services.TransactionService;

/// <summary>
/// Сервис управления транзакциями
/// </summary>
public class TransactionService(AppDbContext context) : ITransactionService
{
    private readonly AppDbContext _appDbContext = context;

    /// <inheritdoc/>
    public async Task<ITransaction> BeginTransactionAsync(CancellationToken token)
    {
        return new Transaction(await _appDbContext.Database.BeginTransactionAsync(token));
    }
}
