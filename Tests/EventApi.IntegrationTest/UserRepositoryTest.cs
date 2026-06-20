using EventManagement.Application.Common.Exceptions;
using EventManagement.Common;
using EventManagement.Domain.Models;
using EventManagement.Infrastructure.Services.UserServices;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
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

    [Fact]
    public async Task AddUser_NotUniqueLogin_ThrowsDbOperationException()
    {        
        // Arrange
        await using var context = _fixture.Context;
        var user = TestData.GetTestUser();
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        // Act
        var rep = new UserRepository(_fixture.Context, _logger);
        Func<Task> act = async () => await rep.AddUserAsync(user, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DbOperationException>();
    }

    [Fact]
    public async Task AddUser_LoginLEssThreeSymbols_ThrowsDbUpdateException()
    {
        // Arrange
        await using var context = _fixture.Context;
        var user = new User("Us", "password", UserRole.User);                
        await context.Users.AddAsync(user);
        

        // Act        
        Func<Task> act = async () => await context.SaveChangesAsync();

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task AddUser_EmptyPassword_ThrowsDbUpdateException()
    {
        // Arrange
        await using var context = _fixture.Context;
        var user = new User("User", "", UserRole.User);
        await context.Users.AddAsync(user);


        // Act        
        Func<Task> act = async () => await context.SaveChangesAsync();

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task AddUser_IncorrectRole_ThrowsDbUpdateException()
    {
        // Arrange
        await using var context = _fixture.Context;
        var user = new User("User", "password", (UserRole)10);
        await context.Users.AddAsync(user);


        // Act        
        Func<Task> act = async () => await context.SaveChangesAsync();

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();
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
