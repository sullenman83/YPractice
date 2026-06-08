namespace EventManagement.Application.Interfaces.Services;

/// <summary>
/// Сервис управления транзакциями
/// </summary>
public interface ITransactionService
{
    /// <summary>
    /// Создать транзакции
    /// </summary>
    /// <param name="token">Токен отмены</param>
    /// <returns>Возвращает транзакцию</returns>
    Task<ITransaction> BeginTransactionAsync(CancellationToken token = default);
}
