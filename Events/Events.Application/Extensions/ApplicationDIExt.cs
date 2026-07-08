using Events.Application.Interfaces;
using Events.Application.Services;
using Events.Application.Services.BackgroundServices;
using Events.Application.Services.MessageHandlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Events.Application.Interfaces.MessageHandlers;

namespace Events.Application.Extensions;

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
        //services.Configure<BookingHandlerSettings>(configuration.GetSection("BookingHandlerSettings"));
        //services.Configure<BookingSettings>(configuration.GetSection("BookingSettings"));
        
        //var bbsSettigs = new BackgroundBookingServiceRepeaterSettigs();
        //configuration.GetSection("BackgroundBookingServiceRepeaterSettigs").Bind(bbsSettigs);        
        //services.AddResiliencePipeline(Consts.BackgroundBookingServiceRepeater, builder =>
        //{
        //    builder.AddRetry(new RetryStrategyOptions()
        //    {
        //        ShouldHandle = new PredicateBuilder().Handle<DbOperationWithBlockingRowException>(),
        //        MaxRetryAttempts = bbsSettigs.MaxRetryAttempts,
        //        Delay = TimeSpan.FromMilliseconds(bbsSettigs.Delay),
        //        BackoffType = DelayBackoffType.Constant
        //    });
        //});

        //var cbSettings = new BookingServiceRepeaterSettings();
        //configuration.GetSection("BookingServiceRepeaterSettings").Bind(cbSettings);
        //services.AddResiliencePipeline(Consts.BookingServiceRepeater, builder =>
        //{
        //    builder.AddTimeout(new TimeoutStrategyOptions() { Timeout = TimeSpan.FromMilliseconds(cbSettings.Timeout) });
        //    builder.AddRetry(new RetryStrategyOptions()
        //    {
        //        ShouldHandle = new PredicateBuilder().Handle<DbOperationWithBlockingRowException>(),
        //        MaxRetryAttempts = cbSettings.MaxRetryAttempts,
        //        Delay = TimeSpan.FromMilliseconds(cbSettings.Delay),
        //        BackoffType = DelayBackoffType.Constant
        //    });
        //});

        services.AddScoped<IEventValidator, EventValidator>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IBookingConfirmedHandler, BookingConfirmedHandler>();
        services.AddHostedService<BookingConfirmedConsumerService>();
        
        return services;
    }
}
