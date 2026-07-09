using Events.Application.Interfaces;
using Events.Application.Services;
using Events.Application.Services.BackgroundServices;
using Events.Application.Services.MessageHandlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Events.Application.Interfaces.MessageHandlers;
using Events.Application.Interfaces.Validators;
using Events.Application.Services.Validators;

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
        services.AddScoped<IEventValidator, EventValidator>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IBookingConfirmedHandler, BookingConfirmedHandler>();
        services.AddScoped<IBookingCancelledHandler, BookingCancelledHandler>();
        services.AddHostedService<BookingConfirmedConsumerService>();
        services.AddScoped<IBookingConfirmedValidator, BookingConfirmedValidator>();
        
        return services;
    }
}
