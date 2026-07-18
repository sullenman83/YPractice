using StackExchange.Redis;
using Testcontainers.Redis;

namespace Events.IntegrationTests;

public class RedisFixture : IAsyncLifetime
{
    private readonly RedisContainer _redisContainer;

    public RedisFixture()
    {
        _redisContainer = new RedisBuilder("redis:7.2-alpine")
            .Build();
    }

    public IConnectionMultiplexer RedisClient { get; private set; } = null!;

    public async Task DisposeAsync()
    {
        if (RedisClient != null)
            await RedisClient.DisposeAsync();

        await _redisContainer.DisposeAsync();
    }

    public async Task InitializeAsync()
    {
        await _redisContainer.StartAsync();

        var options = ConfigurationOptions.Parse(_redisContainer.GetConnectionString());
        options.AllowAdmin = true;

        RedisClient = await ConnectionMultiplexer.ConnectAsync(options);
    }

    public async Task ResetAsync()
    {
        var endPoints = RedisClient.GetEndPoints();

        foreach (var endPoint in endPoints)
        {
            var server = RedisClient.GetServer(endPoint);
            await server.FlushDatabaseAsync();
        }
    }
}
