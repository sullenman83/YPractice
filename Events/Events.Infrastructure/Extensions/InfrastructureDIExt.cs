using DateTimeManager.Abstractions;
using DateTimeManager.Core;
using Events.Application.Interfaces.Consumers;
using Events.Application.Interfaces.Repositories;
using Events.Infrastructure.Data;
using Events.Infrastructure.Services;
using Events.Infrastructure.Services.Consumers;
using Events.Infrastructure.Settings.ConsumerSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TransactionManager.Abstractions;
using TransactionManager.Core;

namespace Events.Infrastructure.Extensions;

/// <summary>
/// Расширение для добавления сервисов инфраструктуры
/// </summary>
public static  class InfrastructureDIExt
{
    /// <summary>
    /// Добавить сервисмы
    /// </summary>
    /// <param name="services">Коллекция сервисов</param>
    /// <param name="configuration">Конфиг</param>
    /// <param name="env">Среда окружения</param>
    /// <returns>Коллекция сервисов</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment env)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Не задана строка подключения к базе даных");

        services.Configure<BookingConfirmedConsumerSettings>(configuration.GetSection(nameof(BookingConfirmedConsumerSettings)));

        if (env.IsDevelopment())
        {
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(connectionString)                
                .UseSnakeCaseNamingConvention()
                .LogTo(Console.WriteLine)
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging();
            });
        }
        else
        {
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention();
            });
        }
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddSingleton<IBookingConfirmedConsumer, BookingConfirmedConsumer>();        
        services.AddScoped<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IInboxMessageRepository, InboxMessageRepository>();
        services.AddScoped<ITransactionService, TransactionService<AppDbContext>>();

        return services;
    }
}
