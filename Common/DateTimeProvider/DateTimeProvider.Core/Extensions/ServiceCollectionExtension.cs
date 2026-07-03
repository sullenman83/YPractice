using DateTimeManager.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace DateTimeManager.Core.Extensions;

/// <summary>
/// Расширение IServiceCollection для добавления провайдера времени
/// </summary>
public static class ServiceCollectionExtension
{
    /// <summary>
    /// Добавить провайдер времени в IServiceCollection. Время жизни scope
    /// </summary>
    /// <param name="services">Коллекция сервисов</param>
    /// <returns>Коллекция сервисов</returns>
    public static IServiceCollection AddDateTimeProvider(this IServiceCollection services)
    {
        services.AddScoped<IDateTimeProvider, DateTimeProvider>();

        return services;
    }
}
