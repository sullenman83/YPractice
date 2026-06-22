using EventManagement.Application.Interfaces.Repositories;
using EventManagement.Application.Interfaces.Security;
using EventManagement.Application.Models;
using EventManagement.Application.Models.UserModels;
using EventManagement.Application.Services.UserService;
using EventManagement.Common;
using EventManagement.Domain.Exceptions;
using EventManagement.Domain.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EventServiceTest;

public class UserTest
{
    private readonly Mock<IPasswordHasher> _mockPasswordHasher = new Mock<IPasswordHasher>();
    private readonly Mock<IUserRepository> _mockUserRepository = new Mock<IUserRepository>();
    private readonly Mock<IJwtTokenGenerator> _mockTokenGenerator = new Mock<IJwtTokenGenerator>();
    private readonly ILogger<UserService> _logger = NullLogger<UserService>.Instance;
    
    public UserTest()
    {
        _mockPasswordHasher.Setup(o => o.GenerateHash(It.IsAny<string>())).Returns((string o) => o);
    }

    [Fact]
    public async Task CreateUser_ReturnsUser()
    {
        // Arrange 
        var requestUser = TestData.getRequestUser();
        
        _mockUserRepository.Setup(o => o.AddUserAsync(It.IsAny<User>())).ReturnsAsync((User user, CancellationToken token) => user);

        var userService = new UserService(_mockUserRepository.Object, _mockPasswordHasher.Object, _mockTokenGenerator.Object, _logger);

        // Act
        var res = await userService.CreateUserAsync(requestUser, CancellationToken.None);

        // Assert
        res.Login.Should().Be(requestUser.Login);
        res.Role.Should().Be(requestUser.Role);
        _mockPasswordHasher.Verify(o => o.GenerateHash(requestUser.Password), Times.Once);
        _mockUserRepository.Verify(o => o.AddUserAsync(It.IsAny<User>()), Times.Once);
    }


    [Fact]
    public async Task LoginUser_ReturnsToken()
    {
        // Arrange 
        var login = "user";
        var passsword = "password";
        var token = "token";
        var user = new User(login, passsword, UserRole.User);

        _mockUserRepository.Setup(o => o.GetUserByLoginAsync(It.IsAny<string>())).ReturnsAsync(user);
        _mockPasswordHasher.Setup(o => o.VerifyPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        _mockTokenGenerator.Setup(o => o.CreateJwtToken(It.IsAny<JwtToketDTO>())).Returns(token);

        var userService = new UserService(_mockUserRepository.Object, _mockPasswordHasher.Object, _mockTokenGenerator.Object, _logger);

        // Act
        var res = await userService.LoginAsync(login, passsword, CancellationToken.None);

        // Assert
        res.Should().Be(token);
        _mockUserRepository.Verify(o => o.GetUserByLoginAsync(It.IsAny<string>()), Times.Once);
        _mockPasswordHasher.Verify(o => o.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        _mockTokenGenerator.Verify(o => o.CreateJwtToken(It.IsAny<JwtToketDTO>()), Times.Once);
    }

    [Fact]    
    public async Task LoginUser_IncorrectLogin_ThrowsInvalidCredentialsException()
    {
        // Arrange 
        var login = "user";
        var passsword = "password";
        
        _mockUserRepository.Setup(o => o.GetUserByLoginAsync(It.IsAny<string>())).Throws<InvalidCredentialsException>();
        
        var userService = new UserService(_mockUserRepository.Object, _mockPasswordHasher.Object, _mockTokenGenerator.Object, _logger);

        // Act
        Func<Task> act = async () => await userService.LoginAsync(login, passsword, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidCredentialsException>();
        
        _mockUserRepository.Verify(o => o.GetUserByLoginAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task LoginUser_IncorrectPassword_ThrowsInvalidCredentialsException()
    {
        // Arrange 
        var login = "user";
        var passsword = "password";        
        var user = new User(login, passsword, UserRole.User);

        _mockUserRepository.Setup(o => o.GetUserByLoginAsync(It.IsAny<string>())).ReturnsAsync(user);
        _mockPasswordHasher.Setup(o => o.VerifyPassword(It.IsAny<string>(), It.IsAny<string>())).Throws<InvalidCredentialsException>();
        

        var userService = new UserService(_mockUserRepository.Object, _mockPasswordHasher.Object, _mockTokenGenerator.Object, _logger);

        // Act
        Func<Task> act = async () => await userService.LoginAsync(login, passsword, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidCredentialsException>();
        _mockUserRepository.Verify(o => o.GetUserByLoginAsync(It.IsAny<string>()), Times.Once);
        _mockPasswordHasher.Verify(o => o.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

}
