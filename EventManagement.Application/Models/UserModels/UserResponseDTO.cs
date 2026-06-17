using EventManagement.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace EventManagement.Application.Models.UserModels;

/// <summary>
/// DTO класс для передачи данных из Web API
/// </summary>
public class UserResponseDTO
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public required Guid Id {  get; set; }

    /// <summary>
    /// Логин пользователя
    /// </summary>    
    public required string Login { get; set; }

    /// <summary>
    /// Пароль пользователя
    /// </summary>
    public required string Password { get; set; }

    /// <summary>
    /// Роль пользователя
    /// </summary>    
    public required UserRole Role { get; set; }

}
