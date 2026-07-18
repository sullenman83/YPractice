
using Events.Application.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace Events.Infrastructure.Services;

/// <summary>
/// Класс реализующий работу с кешем
/// </summary>
/// <param name="multiplexer">Центральный объект редис</param>
/// <param name="logger">Логер</param>
public class CacheService(IConnectionMultiplexer multiplexer, ILogger<CacheService> logger) : ICacheService
{
    private readonly IConnectionMultiplexer _multiplexor = multiplexer;

    private readonly IDatabase _db = multiplexer.GetDatabase();
    private readonly ILogger<CacheService> _logger = logger;
    ///<inheritdoc/>
    public async Task<bool> DeleteAsync(string key)
    {
        if (!_multiplexor.IsConnected)
            return false;

        try
        {
            return await _db.KeyDeleteAsync(key);
        }
        catch (RedisException ex)
        {
            _logger.LogError(ex, "Ошибка удаления ключа");
            return false;
        }
    }

    ///<inheritdoc/>
    public async Task<T?> GetAsync<T>(string key)
    {
        if (!_multiplexor.IsConnected)
            return default(T);

        try
        {
            var res = await _db.StringGetAsync(key);
            if (!res.HasValue)
                return default(T);

            return JsonSerializer.Deserialize<T>(res.ToString());
        }
        catch (RedisException ex)
        {
            _logger.LogError(ex, "Ошибка получения данных из кеша.");
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Непредвиденная ошибка.");
        }
        return default(T);
    }

    ///<inheritdoc/>
    public async Task<bool> SetAsync<T>(string key, T value, TimeSpan ttl)
    {
        if (!_multiplexor.IsConnected)
            return false;

        try
        {
            var val = JsonSerializer.Serialize(value);
            return await _db.StringSetAsync(key, val, ttl);
        }
        catch (RedisException ex)
        {
            _logger.LogError(ex, "Ошбика сохранения в кеш.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Непредвиденная ошибка.");
        }

        return false;
    }
}
