using EventManagement.Application.Interfaces;
using EventManagement.Application.Interfaces.Repositories;
using EventManagement.Application.Interfaces.Security;
using EventManagement.Application.Interfaces.Services;
using EventManagement.Domain.Models;
using EventManagement.Infrastructure.Data;
using EventManagement.Infrastructure.Services;
using EventManagement.Infrastructure.Services.BookingServices;
using EventManagement.Infrastructure.Services.EventServices;
using EventManagement.Infrastructure.Services.Securiry;
using EventManagement.Infrastructure.Services.TransactionService;
using EventManagement.Infrastructure.Services.UserServices;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EventManagement.Infrastructure.Extensions;

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
        services.AddScoped<IEventRepository<Event>, EventRepository>();
        services.AddScoped<IBookingRepository<Booking>, BookingRepository>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        
        return services;
    }
}
