using Bookings.Application.AppSettings;
using Bookings.Application.Common;
using Bookings.Application.Interfaces.BookingServices;
using Bookings.Application.Services;
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
        services.Configure<OutboxMessageSettings>(configuration.GetSection(nameof(OutboxMessageSettings)));

        services.Configure<BookingProdicerSettings>(configuration.GetSection("BookingHandlerSettings"));
        services.Configure<BookingSettings>(configuration.GetSection("BookingSettings"));


        var bbsSettigs = new BackgroundBookingServiceRepeaterSettigs();
        configuration.GetSection("BackgroundBookingServiceRepeaterSettigs").Bind(bbsSettigs);
        services.AddResiliencePipeline(Consts.BackgroundBookingServiceRepeater, builder =>
        {
            builder.AddRetry(new RetryStrategyOptions()
            {
                //ShouldHandle = new PredicateBuilder().Handle<DbOperationWithBlockingRowException>(),
                MaxRetryAttempts = bbsSettigs.MaxRetryAttempts,
                Delay = TimeSpan.FromMilliseconds(bbsSettigs.Delay),
                BackoffType = DelayBackoffType.Constant
            });
        });

        var cbSettings = new BookingServiceRepeaterSettings();
        configuration.GetSection("BookingServiceRepeaterSettings").Bind(cbSettings);
        services.AddResiliencePipeline(Consts.BookingServiceRepeater, builder =>
        {
            builder.AddTimeout(new TimeoutStrategyOptions() { Timeout = TimeSpan.FromMilliseconds(cbSettings.Timeout) });
            builder.AddRetry(new RetryStrategyOptions()
            {
               // ShouldHandle = new PredicateBuilder().Handle<DbOperationWithBlockingRowException>(),
                MaxRetryAttempts = cbSettings.MaxRetryAttempts,
                Delay = TimeSpan.FromMilliseconds(cbSettings.Delay),
                BackoffType = DelayBackoffType.Constant
            });
        });

        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IBookingValidator, BookingValidator>();
        
        return services;
    }
}
