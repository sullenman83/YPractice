using EventManagement.Infrastructure.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


namespace EventManagement.Presentation.Extensions;

/// <summary>
/// Расширение для добавления зависимостей
/// </summary>
public static class PresentationDIExt
{
    /// <summary>
    /// добавить зависимости 
    /// </summary>
    /// <param name="services">Коллекция сервисов</param>
    /// <param name="env">Окружение</param>
    /// <param name="configuration">Конфигурация</param>
    /// <returns>Коллекция сервисов</returns>
    public static IServiceCollection AddPresentation(this IServiceCollection services, IHostEnvironment env, IConfiguration configuration)
    {
        var toketSettings = configuration.GetSection(nameof(JwtTokenSettings)).Get<JwtTokenSettings>() 
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
                ValidIssuer = toketSettings.Issuer,

                ValidateAudience = true,
                ValidAudience = toketSettings.Audience,

                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization();


        if (env.IsDevelopment())
        {
            services.AddSwaggerGen(options =>
            {
                var baseDirectory = AppContext.BaseDirectory;
                var xmlFiles = Directory.GetFiles(baseDirectory, "*.xml", SearchOption.TopDirectoryOnly);
                foreach(var f in xmlFiles)
                {
                    options.IncludeXmlComments(f);
                }
            });
        }
        services.AddControllers(options =>
        {
            options.SuppressAsyncSuffixInActionNames = false;
        });

        services.AddHttpContextAccessor();

        return services;
    }
}
