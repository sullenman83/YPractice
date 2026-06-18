using EventManagement.Application.Models.UserModels;

namespace EventManagement.Application.Interfaces.Services;

/// <summary>
/// Интерфейс сервиса пользователей
/// </summary>
public  interface IUserService
{
    /// <summary>
    /// Создать пользователя
    /// </summary>
    /// <param name="user">Данные пользователя</param>
    /// <param name="token">Токен отмены</param>
    /// <returns>Созданный пользователь</returns>
    Task<UserResponseDTO> CreateUserAsync(UserRequestDTO user, CancellationToken token = default);

    /// <summary>
    /// Залогиниться
    /// </summary>
    /// <param name="login">Логин пользователя</param>
    /// <param name="password">Пароль пользователя</param>
    /// <param name="token">Токен отмены</param>
    /// <returns>Jwt Токен</returns>
    Task<string> LoginAsync(string login, string password, CancellationToken token = default);
}
