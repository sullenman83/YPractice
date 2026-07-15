using Bookings.Application.AppSettings;
using Bookings.Application.Common;
using Bookings.Application.Interfaces.BookingServices;
using Bookings.Application.Services;
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
        services.Configure<BackgroundProducerServiceSettings>(configuration.GetSection(nameof(BackgroundProducerServiceSettings)));
        services.Configure<BookingSettings>(configuration.GetSection(nameof(BookingSettings)));
        services.Configure<BackgroundBookingServiceSettings>(configuration.GetSection(nameof(BackgroundBookingServiceSettings)));

        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IBookingValidator, BookingValidator>();
        services.AddScoped<IBookingHandlerService, BookingHandlerService>();
        services.AddHostedService<BackgroundProducerService>();
        services.AddHostedService<BackgroundBookingService>();

        return services;
    }
}
