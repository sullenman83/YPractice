
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace CommonServiceCollectionExtensions;

/// <summary>
/// Расширение для добавления зависимостей
/// </summary>
public static class SecurityExtension
{
    /// <summary>
    /// добавить зависимости 
    /// </summary>
    /// <param name="services">Коллекция сервисов</param>
    /// <param name="configuration">Конфигурация</param>
    /// <returns>Коллекция сервисов</returns>
    public static IServiceCollection AddSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        var tokenSettings = configuration.GetSection(nameof(JwtTokenSettings)).Get<JwtTokenSettings>()
            ?? throw new InvalidOperationException("не найдены настройки для токена");
        var key = Environment.GetEnvironmentVariable("JWT_KEY") ?? throw new InvalidOperationException("Не найден секретный ключ.");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidIssuer = tokenSettings.Issuer,

                ValidateAudience = true,
                ValidAudience = tokenSettings.Audience,

                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ClockSkew = TimeSpan.Zero,
                RoleClaimType = "role"
            };
            options.MapInboundClaims = false;
        });

        services.AddAuthorization();

        return services;
    }
}
