using EventManagement.Domain.Models;

namespace EventManagement.Application.Models.UserModels.Extensions;

/// <summary>
/// Расширение лдя класса пользователей
/// </summary>
public static class UserExtension
{
    /// <summary>
    /// Конвертировать User в DTO ответа
    /// </summary>
    /// <param name="user">Пользователь</param>
    /// <returns>DTO ответа</returns>
    public static UserResponseDTO ToResponse(this User user)
    {
        return new UserResponseDTO()
        {
            Id = user.Id,
            Login = user.Login,
            Role = user.Role
        };
    }
}
