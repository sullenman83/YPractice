using Bookings.Application.AppSettings;
using Bookings.Application.Common;
using Bookings.Application.Interfaces.BookingServices;
using Bookings.Application.Services.BackgrounServices.Producers;
using Bookings.Application.Services.BookingServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;
using Polly.Timeout;

namespace Bookings.Application.Extensions;

/// <summary>
/// Регистратор сервисов для application слоя
/// </summary>
public static class ApplicationDIExt
{
    /// <summary>
    /// Зарегистрировать сервисы
    /// </summary>
    /// <param name="services">Коллекция сервисов</param>
    /// <param name="configuration">Конфигурация</param>
    /// <returns>Коллекция сервисов</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {        
        services.Configure<BackgroundProducerServiceSettings>(configuration.GetSection("BackgroundProducerServiceSettings"));
        services.Configure<BookingSettings>(configuration.GetSection("BookingSettings"));

        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IBookingValidator, BookingValidator>();
        services.AddHostedService<BackgroundProducerService>();

        return services;
    }
}
