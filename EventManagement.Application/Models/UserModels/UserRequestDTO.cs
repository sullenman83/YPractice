using EventManagement.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace EventManagement.Application.Models.UserModels;

/// <summary>
/// DTO класс для передачи данных пользователя в WEB API
/// </summary>
public class UserRequestDTO
{
    /// <summary>
    /// Логин пользователя
    /// </summary>
    [Required]
    [MinLength(3)]

    public required string Login {  get; set; }

    /// <summary>
    /// Пароль пользователя
    /// </summary>
    [Required]
    [MinLength(1)]
    public required string Password { get; set; }

    /// <summary>
    /// Роль пользователя
    /// </summary>
    [Required]
    public required UserRole Role{ get; set; }
}
