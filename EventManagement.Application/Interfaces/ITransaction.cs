namespace EventManagement.Application.Interfaces;

/// <summary>
/// Интекрфейс транзакции
/// </summary>
public interface ITransaction : IAsyncDisposable, IDisposable
{
    /// <summary>
    /// Коммит изменений
    /// </summary>
    /// <param name="token">Токен отмены</param>
    /// <returns></returns>
    Task CommitAsync(CancellationToken token = default);

    /// <summary>
    /// Откат изменений
    /// </summary>
    /// <param name="token">Токен отмены</param>
    /// <returns></returns>
    Task RollbackAsync(CancellationToken token = default);
}
