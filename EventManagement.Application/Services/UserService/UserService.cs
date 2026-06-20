using EventManagement.Application.Common.Exceptions;
using EventManagement.Application.Interfaces.Repositories;
using EventManagement.Application.Interfaces.Security;
using EventManagement.Application.Interfaces.Services;
using EventManagement.Application.Models;
using EventManagement.Application.Models.UserModels;
using EventManagement.Application.Models.UserModels.Extensions;
using EventManagement.Domain.Exceptions;
using EventManagement.Domain.Models;
using Microsoft.Extensions.Logging;

namespace EventManagement.Application.Services.UserService;

/// <summary>
/// Сервис для работы с пользователями
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly ILogger<UserService> _logger;
    
    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="userRepository">Репозиторий пользователе</param>
    /// <param name="passwordHasher">объект для работы с паролями</param>
    /// <param name="tokenGenerator">Генератор токенов</param>
    /// <param name="logger">Логгер</param>
    public UserService(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtTokenGenerator tokenGenerator, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
        _logger = logger;
    }

    ///<inheritdoc/>
    ///<exception cref="DbOperationException">Ошмбки при работе с БД</exception>
    public async Task<UserResponseDTO> CreateUserAsync(UserRequestDTO user, CancellationToken token)
    {        
        var password = _passwordHasher.GenerateHash(user.Password);

        var u = new User(user.Login, password, user.Role);

        u = await _userRepository.AddUserAsync(u, token);

        return u.ToResponse();
    }

    ///<inheritdoc/>
    ///<exception cref="InvalidCredentialsException">Ошибка входа</exception>    
    public async Task<string> LoginAsync(string login, string password, CancellationToken token)
    {
        var message = "Ошибка авторизации. Неверный логин или пароль.";
        var u = await _userRepository.GetUserByLoginAsync(login, token);
        if (u == null)
            throw new InvalidCredentialsException(message);

        if (!_passwordHasher.VerifyPassword(password, u.Password))
            throw new InvalidCredentialsException(message);

        var jwtData = new JwtToketDTO()
        {
            Id = u.Id,
            Login = login,
            Role = u.Role,
        };

        return _tokenGenerator.CreateJwtToken(jwtData);
    }
}
