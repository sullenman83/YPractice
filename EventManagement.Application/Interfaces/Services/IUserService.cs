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
    /// <returns>Созданный пользователь</returns>
    Task<UserResponseDTO> CreateUser(UserRequestDTO user);

    /// <summary>
    /// Залогиниться
    /// </summary>
    /// <param name="login">Логин пользователя</param>
    /// <param name="password">Пароль пользователя</param>
    /// <returns>Jwt Токен</returns>
    Task<string> Login(string login, string password);
}
