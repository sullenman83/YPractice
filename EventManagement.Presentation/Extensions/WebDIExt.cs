using EventManagement.Application.Interfaces;
using EventManagement.Application.Services.EventServices;
using EventManagement.Domain.Interfaces;
using EventManagement.Domain.Services;
using System.Reflection;

namespace EventManagement.Presentation.Extensions;

/// <summary>
/// Расширение для добавления зависимостей
/// </summary>
public static class WebDIExt
{
    /// <summary>
    /// добавить зависимости 
    /// </summary>
    /// <param name="services">Коллекция сервисов</param>
    /// <param name="env">Окружение</param>
    /// <returns>Коллекция сервисов</returns>
    public static IServiceCollection AddPresentation(this IServiceCollection services, IHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            services.AddSwaggerGen(options =>
            {
                var file = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var path = Path.Combine(AppContext.BaseDirectory, file);
                options.IncludeXmlComments(path);
            });
        }
        services.AddScoped<IEventValidator, EventValidator>();
        services.AddScoped<IDateTimeProvider, DateTimeProvider>();
        services.AddControllers(options =>
        {
            options.SuppressAsyncSuffixInActionNames = false;
        });

        return services;
    }
}
