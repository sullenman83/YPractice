
using EventManagement.Application.Common;
using EventManagement.Application.Common.AppSettings;
using EventManagement.Application.Common.Exceptions;
using EventManagement.Application.Interfaces;
using EventManagement.Application.Interfaces.Services;
using EventManagement.Application.Services;
using EventManagement.Application.Services.BookingServices;
using EventManagement.Application.Services.EventServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;
using Polly.Timeout;

namespace EventManagement.Application.Extensions;

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
        var bbsSettigs = new BackgroundBookingServiceRetrySettigs();
        configuration.GetSection("BackgroundBookingServiceRetrySettigs").Bind(bbsSettigs);
        services.Configure<BookingHandlerSettings>(configuration.GetSection("BookingHandlerSettings"));
        services.AddResiliencePipeline(Consts.BackgroundBookingServiceRepeater, builder =>
        {
            builder.AddRetry(new RetryStrategyOptions()
            {
                ShouldHandle = new PredicateBuilder().Handle<DbOperationWithBlockingRowException>(),
                MaxRetryAttempts = bbsSettigs.MaxRetryAttempts,
                Delay = TimeSpan.FromMilliseconds(bbsSettigs.Delay),
                BackoffType = DelayBackoffType.Constant
            });
        });

        var cbSettings = new CreateBookingRetrySettigs();
        configuration.GetSection("CreateBookingRetrySettigs").Bind(bbsSettigs);
        services.AddResiliencePipeline(Consts.CreateBookingRepeater, builder =>
        {
            builder.AddTimeout(new TimeoutStrategyOptions() { Timeout = TimeSpan.FromMilliseconds(cbSettings.Timeout) });
            builder.AddRetry(new RetryStrategyOptions()
            {
                ShouldHandle = new PredicateBuilder().Handle<DbOperationWithBlockingRowException>(),
                MaxRetryAttempts = cbSettings.MaxRetryAttempts,
                Delay = TimeSpan.FromMilliseconds(cbSettings.Delay),
                BackoffType = DelayBackoffType.Constant
            });
        });
        services.AddScoped<IEventValidator, EventValidator>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IBookingService, BookingService>();        
        services.AddScoped<IBackgroundBookingService, BackgroundBookingService>();        
        services.AddHostedService<BookingHandlerService>();

        return services;
    }
}
