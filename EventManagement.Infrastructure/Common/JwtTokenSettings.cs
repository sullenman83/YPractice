namespace EventManagement.Infrastructure.Common;

/// <summary>
/// Настройки для генератора токенов
/// </summary>
public class JwtTokenSettings
{
    /// <summary>
    /// Издатель токена
    /// </summary>
    public string Issuer { get; set; } = "";

    /// <summary>
    /// Для кого предназначен
    /// </summary>
    public string Audience { get; set; } = "";

    /// <summary>
    /// Время жизни в минутах
    /// </summary>
    public int Expires { get; set; } = 15;
}
