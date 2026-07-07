using Bookings.Application.Interfaces;
using Bookings.Application.Interfaces.Repositories;
using Bookings.Infrastructure.Data;
using Bookings.Infrastructure.Services.Repositories.BookingRepository;
using Bookings.Infrastructure.Services.Repositories.MessageRepositories;
using Bookings.Infrastructure.Services.UserServices;
using Bookings.Infrastructure.Settings;
using Bookings.Infrastructure.Settings.ConsumersSettings;
using DateTimeManager.Abstractions;
using DateTimeManager.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TransactionManager.Abstractions;
using TransactionManager.Core;
using Bookings.Infrastructure.Services.Producers;

namespace Bookings.Infrastructure.Extensions;

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

        services.Configure<BookingProducerSettings>(configuration.GetSection(nameof(BookingProducerSettings)));
        services.Configure<EventSeatsReservedConsumerSettings>(configuration.GetSection(nameof(EventSeatsReservedConsumerSettings)));
        services.Configure<EventSeatsNotReservedConsumerSettings>(configuration.GetSection(nameof(EventSeatsNotReservedConsumerSettings)));
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
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();
        services.AddScoped<IInboxMessageRepository, InboxMessageRepository>();
        services.AddScoped<ITransactionService, TransactionService<AppDbContext>>();
        services.AddSingleton<IBookingProduсer, BookingProducer>();

        return services;
    }
}
