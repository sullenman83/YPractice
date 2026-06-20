using EventManagement.Common;
using EventManagement.Domain.Models;
using EventManagement.Infrastructure.Services.UserServices;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace EventApi.IntegrationTest;

public class UserRepositoryTest(DatabaseFixture fixture) : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly DatabaseFixture _fixture = fixture;
    private readonly ILogger<UserRepository> _logger = NullLogger<UserRepository>.Instance;

    [Fact]
    public async Task AddUser_SavesUserToDataBase()
    {
        // Arrange
        await using var context = _fixture.Context;
        var user = TestData.GetTestUser();
        var id = user.Id;
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
                
        // Act
        var rep = new UserRepository(_fixture.Context, _logger);

        // Assert
        await using var ctx = _fixture.Context;
        var res = ctx.Users.FirstOrDefault(o => o.Id == user.Id);
        res.Should().NotBeNull();
        res.Should().BeEquivalentTo(user);
    }

    [Fact]
    public async Task GetUserByLoginAsync_ReturnsUser()
    {
        // Arrange
        await using var context = _fixture.Context;
        var user = TestData.GetTestUser();
        var id = user.Id;
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        // Act
        var rep = new UserRepository(_fixture.Context, _logger);
        var res = await rep.GetUserByLoginAsync(user.Login, CancellationToken.None);

        // Assert
        res.Should().BeEquivalentTo(user);
    }


    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task InitializeAsync()
    {
        await _fixture.ResetDatabaseAsync();
    }
}
