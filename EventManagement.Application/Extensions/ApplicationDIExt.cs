
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

namespace EventManagement.Application.Extensions;

public static class ApplicationDIExt
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        var retrySettings = new RetrySettings();
        configuration.GetSection("RetrySettings").Bind(retrySettings);
        services.Configure<BookingHandlerSettings>(configuration.GetSection("BookingHandlerSettings"));
        services.AddResiliencePipeline(Consts.CreateBookingRetry, builder =>
        {
            builder.AddRetry(new RetryStrategyOptions()
            {
                ShouldHandle = new PredicateBuilder().Handle<DbOperationWithBlockingRowException>(),
                MaxRetryAttempts = retrySettings.MaxRetryAttempts,
                Delay = TimeSpan.FromMilliseconds(retrySettings.Delay),
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
