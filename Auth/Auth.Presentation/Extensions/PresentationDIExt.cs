using CommonServiceCollectionExtensions;

namespace Auth.Presentation.Extensions;

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
        services.Configure<JwtTokenSettings>(configuration.GetSection(nameof(JwtTokenSettings)));        
        services.AddSecurity(configuration);
        services.AddSwager(env);
                
        services.AddControllers(options =>
        {
            options.SuppressAsyncSuffixInActionNames = false;
        });

        return services;
    }
}
