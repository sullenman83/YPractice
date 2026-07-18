using Events.Application.Common;
using Events.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;

namespace Events.IntegrationTests;

public class CacheServiceTest : IClassFixture<RedisFixture>, IAsyncLifetime
{
    private readonly RedisFixture _fixture;
    private readonly ILogger<CacheService> _logger = NullLogger<CacheService>.Instance;

    public CacheServiceTest(RedisFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task SetAsync_AddValue_ReturnsTheSameValue()
    {
        // Arrange
        var key = CacheKeys.EventKey(Guid.NewGuid());
        var ttl = TimeSpan.FromSeconds(60);
        var value = "value";
        var service = new CacheService(_fixture.RedisClient, _logger);
        var db = _fixture.RedisClient.GetDatabase();

        // Act
        await service.SetAsync<string>(key, value, ttl);
        var cacheValue = await db.StringGetAsync(key);
        var res = JsonSerializer.Deserialize<string>(cacheValue.ToString());

        // Assert        
        res.Should().NotBeNull();
        res.ToString().Should().Be(value);
    }


    [Fact]
    public async Task DeleteAsync_DeletesValue()
    {
        // Arrange
        var key = CacheKeys.EventKey(Guid.NewGuid());
        var ttl = TimeSpan.FromSeconds(60);
        var value = "value";
        var db = _fixture.RedisClient.GetDatabase();
        await db.StringSetAsync(key, JsonSerializer.Serialize(value), ttl);
        var service = new CacheService(_fixture.RedisClient, _logger);

        // Act
        var res = await service.DeleteAsync(key);
        var value1 =await db.StringGetAsync(key);
        
        // Assert
        res.Should().BeTrue();
        value1.HasValue.Should().BeFalse();
    }

    [Fact]
    public async Task GetAsync_ReturnsValue()
    {
        // Arrange
         var key = CacheKeys.EventKey(Guid.NewGuid());
        var ttl = TimeSpan.FromSeconds(60);
        var value = "value";
        var db = _fixture.RedisClient.GetDatabase();
        var r = await db.StringSetAsync(key, JsonSerializer.Serialize(value), ttl);
        var service = new CacheService(_fixture.RedisClient, _logger);

        // Act
        var res = await service.GetAsync<string>(key);

        // Assert
        res.Should().NotBeNull();
        res.Should().Be(value);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task InitializeAsync()
    {
        await _fixture.ResetAsync();
    }
}
