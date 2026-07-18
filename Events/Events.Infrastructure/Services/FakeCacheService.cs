
using Events.Application.Interfaces;

namespace Events.Infrastructure.Services;

/// <summary>
/// Фековый кеш, если не передан пароль к redis используеся заглушка
/// </summary>
public class FakeCacheService : ICacheService
{
    ///<inheritdoc/>
    public Task<bool> DeleteAsync(string key)
    {
        return Task.FromResult(true);
    }

    ///<inheritdoc/>
    public Task<T?> GetAsync<T>(string key)
    {
        return Task.FromResult(default(T));
    }

    ///<inheritdoc/>
    public Task<bool> SetAsync<T>(string key, T value, TimeSpan ttl)
    {
        return Task.FromResult(true);
    }
}
