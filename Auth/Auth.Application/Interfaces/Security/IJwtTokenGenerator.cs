using Auth.Application.Models;

namespace Auth.Application.Interfaces.Security;

/// <summary>
/// Генератор Jwt токенов
/// </summary>
public interface IJwtTokenGenerator
{
    /// <summary>
    /// Создать токен
    /// </summary>
    /// <param name="data">Дагнные для создания токена</param>
    /// <returns>Токен</returns>
    string CreateJwtToken(JwtToketDTO data);
}
