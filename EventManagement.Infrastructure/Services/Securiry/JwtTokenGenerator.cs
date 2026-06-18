using EventManagement.Application.Interfaces;
using EventManagement.Application.Interfaces.Security;
using EventManagement.Application.Models;
using EventManagement.Infrastructure.Common;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EventManagement.Infrastructure.Services.Securiry;

/// <summary>
/// Генератор токенов
/// </summary>
public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtTokenSettings _jwtTokenSettings;
    private readonly IDateTimeProvider _dateTimeProvider;

    /// <summary>
    /// Конструкотр
    /// </summary>
    /// <param name="jwtToketSettings">Настройки для генератора токенов</param>
    /// <param name="dateTimeProvider">Провайдер времени</param>
    public JwtTokenGenerator(IOptions<JwtTokenSettings> jwtToketSettings, IDateTimeProvider dateTimeProvider)
    {
        _jwtTokenSettings = jwtToketSettings.Value;
        _dateTimeProvider = dateTimeProvider;
    }

    ///<inheritdoc/>
    ///<exception cref="InvalidOperationException">НЕ найден ключ</exception>
    public string CreateJwtToken(JwtToketDTO data)
    {
        var claims = new Dictionary<string, object>()
        {
            [JwtRegisteredClaimNames.Sub] = data.Id,
            [JwtRegisteredClaimNames.Name] = data.Login,
            ["role"] = data.Role
        };

        var key = Environment.GetEnvironmentVariable("JWT_KEY") ?? throw new InvalidOperationException("Не найден секретный ключ.");

        var signingkey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(signingkey, SecurityAlgorithms.HmacSha256);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _jwtTokenSettings.Issuer,
            Audience = _jwtTokenSettings.Audience,
            Expires = _dateTimeProvider.GetUtcNow().DateTime.AddMinutes(_jwtTokenSettings.Expires),
            Claims = claims,
            SigningCredentials = creds
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}