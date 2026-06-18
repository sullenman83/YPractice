using EventManagement.Domain.Models;

namespace EventManagement.Application.Interfaces.Repositories;

/// <summary>
/// Интейрфейс репозитория пользователей
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Добавить нового пользователя
    /// </summary>
    /// <param name="user">Данные пользователя</param>
    ///<param name="token">Токен отмены</param>
    /// <returns>Добавленный пользователь</returns>
    Task<User> AddUserAsync(User user, CancellationToken token = default);

    /// <summary>
    /// Получить пользователя по логину
    /// </summary>
    /// <param name="login">Логин пользователя</param>
    ///<param name="token">Токен отмены</param>
    /// <returns>Пользователь или null, если не найден</returns>
    Task<User?> GetUserByLoginAsync(string login, CancellationToken token = default);
}
