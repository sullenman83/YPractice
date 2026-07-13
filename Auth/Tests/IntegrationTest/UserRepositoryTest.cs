using Auth.Application.Exceptions;
using Auth.Domain.Models;
using Auth.Infrastructure.Services.UserServices;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Org.BouncyCastle.Crypto.Signers;
using UserRooles;

namespace Auth.IntegrationTest;

public class UserRepositoryTest: IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private readonly ILogger<UserRepository> _logger = NullLogger<UserRepository>.Instance;

    public UserRepositoryTest(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AddUser_SevesUserToDataBase()
    {
        // Arrange
        await using var context = _fixture.Context;
        var user = new User("user1", "password1", UserRole.Admin);
        var repository = new UserRepository(context, _logger);
        
        // Act
        var res = await repository.AddUserAsync(user);

        // Assert
        await using var ctx = _fixture.Context;
        var u = ctx.Users.FirstOrDefault(o => o.Id == user.Id);
        u.Should().NotBeNull();
        u.Should().BeEquivalentTo(res);
    }

    [Fact]
    public async Task AddUser_NotUniqueLogin_ThrowsDbOperationException()
    {
        // Arrange
        await using var context = _fixture.Context;
        var user = new User("user1", "password1", UserRole.Admin);
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        // Act
        var repository = new UserRepository(_fixture.Context, _logger);
        Func<Task<User>> act = async () =>  await repository.AddUserAsync(user);

        // Assert
        await act.Should().ThrowAsync<DbOperationException>().WithMessage("Ошибка добавления элемента в БД");
    }

    [Fact]
    public async Task AddUser_LoginLenthLessThreeSymbols_ThrowsDbOperationException()
    {
        // Arrange
        await using var context = _fixture.Context;
        var user = new User("us", "password1", UserRole.Admin);
        
        // Act
        var repository = new UserRepository(_fixture.Context, _logger);
        Func<Task<User>> act = async () => await repository.AddUserAsync(user);

        // Assert
        await act.Should().ThrowAsync<DbOperationException>().WithMessage("Ошибка добавления элемента в БД");
    }

    [Fact]
    public async Task AddUser_EmptyPassword_ThrowsDbOperationException()
    {
        // Arrange
        await using var context = _fixture.Context;
        var user = new User("user1", "", UserRole.Admin);

        // Act
        var repository = new UserRepository(_fixture.Context, _logger);
        Func<Task<User>> act = async () => await repository.AddUserAsync(user);

        // Assert
        await act.Should().ThrowAsync<DbOperationException>().WithMessage("Ошибка добавления элемента в БД");
    }

    [Fact]
    public async Task AddUser_IncorrectRole_ThrowsDbOperationException()
    {
        // Arrange
        await using var context = _fixture.Context;
        var user = new User("user1", "password", (UserRole)256);

        // Act
        var repository = new UserRepository(_fixture.Context, _logger);
        Func<Task<User>> act = async () => await repository.AddUserAsync(user);

        // Assert
        await act.Should().ThrowAsync<DbOperationException>().WithMessage("Ошибка добавления элемента в БД");
    }

    [Fact]
    public async Task GetUserByLoginAsync_RetursUser()
    {
        // Arrange
        var login = "user1";
        var user = new User(login, "password1", UserRole.Admin);
        await using var context = _fixture.Context;
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
        
        // Act
        var repository = new UserRepository(_fixture.Context, _logger);
        var res = await repository.GetUserByLoginAsync(login);

        // Assert
        res.Should().NotBeNull();
        res.Should().BeEquivalentTo(user);
    }

    [Fact]
    public async Task GetUserByLoginAsync_IncorrectLogin_RetursNULL()
    {
        // Arrange
        var login = "user11";
        var user = new User("user1", "password1", UserRole.Admin);
        await using var context = _fixture.Context;
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        // Act
        var repository = new UserRepository(_fixture.Context, _logger);
        var res = await repository.GetUserByLoginAsync(login);

        // Assert
        res.Should().BeNull();
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
