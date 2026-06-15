using System.Diagnostics.CodeAnalysis;

namespace EventManagement.Domain.Models;

/// <summary>
/// Клас спользователя
/// </summary>
public class User
{
    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="login">Логин пользователя</param>
    /// <param name="password">Хэш пароля пользователя</param>
    /// <param name="role">Роль пользователя</param>
    [SetsRequiredMembers]
    public User(string login, string password, UserRole role)
    {
        Id = Guid.NewGuid();
        Login = login;
        Password = password;
        Role = role;
    }

    /// <summary>
    /// Конструктор
    /// </summary>
    private User() {}

    /// <summary>
    /// Идентификатр пользователя
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Логин пользователя
    /// </summary>
    public required string Login { get; init; }

    /// <summary>
    /// Хэш пароля
    /// </summary>
    public required string Password { get; init; }

    /// <summary>
    /// Роль пользователя
    /// </summary>
    public required UserRole Role { get; init; }

    /// <summary>
    /// Список бронирований
    /// </summary>
    public List<Booking>? Bookings{ get; set; }
}
