using EventManagement.Application.Interfaces.Services;
using EventManagement.Application.Models.UserModels;
using EventManagement.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EventManagement.Presentation.Controllers;

/// <summary>
/// Контроллер для создания пользователей и аутентификации
/// </summary>
/// <param name="userService">Сервис пользователей</param>
[ApiController]
[Route("[controller]")]
public class AuthController(IUserService userService): ControllerBase
{
    private readonly IUserService _userService = userService;

    /// <summary>
    /// Создать нового пользователя
    /// </summary>
    /// <param name="login">Логин пользователя</param>
    /// <param name="password">Пароль пользователя</param>
    /// <param name="role">Роль пользователя</param>
    /// <param name="token">Токен отмены</param>
    [Produces("application/json")]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status500InternalServerError)]
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [Required]
        [MinLength(3)]
        string login,
        [Required]
        [MinLength(1)]
        string password,
        UserRole role = UserRole.User,
        CancellationToken token = default)
    {
        var user = new UserRequestDTO()
        {
            Login = login,
            Password = password,
            Role = role
        };
        await _userService.CreateUserAsync(user, token);

        return NoContent();
    }

    /// <summary>
    /// Залогиниться
    /// </summary>
    /// <param name="login">Логин пользователя</param>
    /// <param name="password">Пароль пользователя</param>
    /// <param name="token">Токен отмены</param>
    [Produces("application/json")]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status404NotFound)]
    [HttpPost("login")]
    public async Task<IActionResult> Login(string login, string password, CancellationToken token)
    {
        var t = await _userService.LoginAsync(login, password, token);

        return Ok(new { token = t});
    }
}
