using DateTimeManager.Abstractions;
using DateTimeManager.Core;
using Events.Application.Interfaces;
using Events.Application.Interfaces.Consumers;
using Events.Application.Interfaces.Repositories;
using Events.Infrastructure.Data;
using Events.Infrastructure.Services;
using Events.Infrastructure.Services.Consumers;
using Events.Infrastructure.Settings;
using Events.Infrastructure.Settings.ConsumerSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using StackExchange.Redis;
using System.ComponentModel;
using TransactionManager.Abstractions;
using TransactionManager.Core;

namespace Events.Infrastructure.Extensions;

/// <summary>
/// Расширение для добавления сервисов инфраструктуры
/// </summary>
public static class InfrastructureDIExt
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
        var loggerFactory = LoggerFactory.Create(logging => logging.AddConfiguration(configuration));
        var logger = loggerFactory.CreateLogger<IServiceCollection>();

        var baseConnectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Не задана строка подключения к базе даных");        
        services.Configure<BookingConfirmedConsumerSettings>(configuration.GetSection(nameof(BookingConfirmedConsumerSettings)));
        services.Configure<BookingCancelledConsumerSettings>(configuration.GetSection(nameof(BookingCancelledConsumerSettings)));
        var redisSettings = configuration.GetSection(nameof(RedisSettings)).Get<RedisSettings>();
        var redisPassword = Environment.GetEnvironmentVariable("REDIS_PASSWORD");        
        var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? throw new InvalidOperationException("Не задана переменная окружения с паролем Postgres");
        var connectionString = new NpgsqlConnectionStringBuilder(baseConnectionString)
        {
            Password = dbPassword,
        }.ConnectionString;


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
        services.AddSingleton<IBookingCancelledConsumer, BookingCancelledConsumer>();
        services.AddScoped<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IInboxMessageRepository, InboxMessageRepository>();
        services.AddScoped<ITransactionService, TransactionService<AppDbContext>>();

        if (redisPassword == null || redisSettings == null)
        {
            logger.LogInformation("Не заданы настройки Redis");            
            services.AddSingleton<ICacheService, FakeCacheService>();
        }        
        else
        {
            services.AddSingleton<IConnectionMultiplexer>(f =>
            {
                var logger = f.GetRequiredService<ILogger<ConnectionMultiplexer>>();
                var endPoint = redisSettings.EndPoints.Split(",", StringSplitOptions.RemoveEmptyEntries);
                var cfg = new ConfigurationOptions()
                {
                    ConnectTimeout = redisSettings.ConnectTimeout,
                    SyncTimeout = redisSettings.SyncTimeout,
                    AsyncTimeout = redisSettings.AsyncTimeout,
                    AbortOnConnectFail = redisSettings.AbortOnConnectFail,
                    Password = redisPassword
                };

                foreach (var e in endPoint)
                {
                    cfg.EndPoints.Add(e);
                }

                var multiplexer = ConnectionMultiplexer.Connect(cfg);
                if (multiplexer.IsConnected == false)
                    logger.LogWarning("Ошибка при создании соединения к Redis");

                multiplexer.ConnectionFailed += (sender, e) => logger.LogWarning(e.Exception, "Потеря соединения с Redis.");
                multiplexer.ConnectionRestored += (sender, e) => logger.LogInformation("Соединение с Redis успешно установлено/восстановлено.");

                return multiplexer;
            });
        }

        services.AddScoped<ICacheService, CacheService>();
        
        return services;
    }
}
