using EventManagement.Application.Interfaces.Repositories;
using EventManagement.Application.Interfaces.Security;
using EventManagement.Application.Interfaces.Services;
using EventManagement.Application.Models.UserModels;
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
    public Task<UserResponseDTO> CreateUser(UserRequestDTO user)
    {
        throw new NotImplementedException();
    }

    ///<inheritdoc/>
    public Task<string> Login(string login, string password)
    {
        throw new NotImplementedException();
    }
}
