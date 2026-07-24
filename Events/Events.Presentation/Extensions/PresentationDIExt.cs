using CommonServiceCollectionExtensions;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

namespace Events.Presentation.Extensions;

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
        services.AddSecurity(configuration);
        services.AddSwager(env);
                
        services.AddControllers(options =>
        {
            options.SuppressAsyncSuffixInActionNames = false;
        });

        services.AddOpenTelemetry()
           .ConfigureResource(r => r.AddService(env.ApplicationName))
           .WithMetrics(metrics =>
               metrics
                   .AddAspNetCoreInstrumentation()
                   .AddHttpClientInstrumentation()
                   .AddRuntimeInstrumentation()
                   .AddPrometheusExporter()
               )
           ;

        return services;
    }
}
