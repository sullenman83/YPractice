using EventManagement.Application.Interfaces;
using EventManagement.Application.Interfaces.Repositories;
using EventManagement.Application.Interfaces.Services;
using EventManagement.Domain.Models;
using EventManagement.Infrastructure.Data;
using EventManagement.Infrastructure.Services;
using EventManagement.Infrastructure.Services.BookingServices;
using EventManagement.Infrastructure.Services.EventServices;
using EventManagement.Infrastructure.Services.TransactionService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EventManagement.Infrastructure.Extensions;

public static  class InfrastructureDIExt
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment env)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Не задана строка подключения к базе даных");

        if (env.IsDevelopment())
        {
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(connectionString)
                .LogTo(Console.WriteLine)
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging();
            });
        }
        else
        {
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });
        }
        services.AddScoped<IEventRepository<Event>, EventRepository>();
        services.AddScoped<IBookingRepository<Booking>, BookingRepository>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IDateTimeProvider, DateTimeProvider>();

        return services;
    }
}
