using Auth.Application.Interfaces.Repositories;
using Auth.Application.Interfaces.Security;
using Auth.Application.Interfaces.Services;
using Auth.Application.Models;
using Auth.Application.Models.UserModels;
using Auth.Application.Services;
using Auth.Domain.Exceptions;
using Auth.Domain.Models;
using FluentAssertions;
using Moq;
using UserRooles;

namespace Auth.Test
{
    public class UserServiceTest
    {
        private readonly Mock<IUserRepository> _mockUserRepository = new Mock<IUserRepository>();
        private readonly Mock<IPasswordHasher> _mockPasswordHasher= new Mock<IPasswordHasher>();
        private readonly Mock<IJwtTokenGenerator> _mockTokenGenerator = new Mock<IJwtTokenGenerator>();

        [Fact]
        public async Task CreateUser_ReturnsNewUSer()
        {
            // Arrange
            var user = new UserRequestDTO() { Login = "user1", Password = "password", Role = UserRole.Admin};
            var response = new UserResponseDTO() { Id = Guid.NewGuid(), Login = user.Login, Role = user.Role };

            _mockUserRepository.Setup(o => o.AddUserAsync(It.IsAny<User>())).ReturnsAsync((User u, CancellationToken t) => u);
            var service = new UserService(_mockUserRepository.Object, _mockPasswordHasher.Object, _mockTokenGenerator.Object);

            // Act
            var res = await service.CreateUserAsync(user, CancellationToken.None);

            // Assert
            res.Should().NotBeNull();
            res.Login.Should().Be(user.Login);
            res.Role.Should().Be(user.Role);
            _mockPasswordHasher.Verify(o => o.GenerateHash(user.Password), Times.Once);
            _mockUserRepository.Verify(o => o.AddUserAsync(It.IsAny<User>()), Times.Once);                        
        }


        [Fact]
        public async Task LoginUser_ReturnsToken()
        {
            // Arrange
            var login = "user1";
            var password = "password";
            var response = "token";
            var user = new User(login, password, UserRole.Admin);

            _mockUserRepository.Setup(o => o.GetUserByLoginAsync(login)).ReturnsAsync((string loogin, CancellationToken t) => user);
            _mockPasswordHasher.Setup(o => o.VerifyPassword(password, user.Password)).Returns(true);
            _mockTokenGenerator.Setup(o => o.CreateJwtToken(It.IsAny<JwtToketDTO>())).Returns(response);
            var service = new UserService(_mockUserRepository.Object, _mockPasswordHasher.Object, _mockTokenGenerator.Object);

            // Act
            var res = await service.LoginAsync(login, password, CancellationToken.None);

            // Assert
            res.Should().NotBeNull();
            res.Should().Be(response);
            _mockUserRepository.Verify(o => o.GetUserByLoginAsync(user.Login), Times.Once);
            _mockPasswordHasher.Verify(o => o.VerifyPassword(password, user.Password), Times.Once);
            _mockTokenGenerator.Verify(o => o.CreateJwtToken(It.IsAny<JwtToketDTO>()), Times.Once);
        }

        [Fact]
        public async Task LoginUser_IncorrectLogin_ThrowsInvalidCredentialsException()
        {
            // Arrange
            var login = "user1";
            var password = "password";
            var user = new User(login, password, UserRole.Admin);

            _mockUserRepository.Setup(o => o.GetUserByLoginAsync(login)).ReturnsAsync((string loogin, CancellationToken t) => user);
            

            var service = new UserService(_mockUserRepository.Object, _mockPasswordHasher.Object, _mockTokenGenerator.Object);

            // Act
            Func<Task> act = async () => await service.LoginAsync(login, password, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidCredentialsException>();
            _mockUserRepository.Verify(o => o.GetUserByLoginAsync(user.Login), Times.Once);
        }

        [Fact]
        public async Task LoginUser_IncorrectPassword_ThrowsInvalidCredentialsException()
        {
            // Arrange
            var login = "user1";
            var password = "password";
            var user = new User(login, password, UserRole.Admin);

            _mockUserRepository.Setup(o => o.GetUserByLoginAsync(login)).ReturnsAsync((string loogin, CancellationToken t) => user);
            _mockPasswordHasher.Setup(o => o.VerifyPassword(password, user.Password)).Returns(false);
            var service = new UserService(_mockUserRepository.Object, _mockPasswordHasher.Object, _mockTokenGenerator.Object);

            // Act
            Func<Task> act = async () => await service.LoginAsync(login, password, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidCredentialsException>();
            _mockUserRepository.Verify(o => o.GetUserByLoginAsync(user.Login), Times.Once);
            _mockPasswordHasher.Verify(o => o.VerifyPassword(password, user.Password), Times.Once);
        }

    }
}
