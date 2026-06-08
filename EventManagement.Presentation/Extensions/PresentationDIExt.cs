using EventManagement.Application.Interfaces;
using EventManagement.Application.Services.EventServices;
using EventManagement.Infrastructure.Services;
using System.Reflection;

namespace EventManagement.Presentation.Extensions;

/// <summary>
/// Расширение для добавления зависимостей
/// </summary>
public static class PresentationDIExt
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
                var baseDirectory = AppContext.BaseDirectory;
                var xmlFiles = Directory.GetFiles(baseDirectory, "*.xml", SearchOption.TopDirectoryOnly);
                foreach(var f in xmlFiles)
                {
                    options.IncludeXmlComments(f);
                }
            });
        }
        services.AddControllers(options =>
        {
            options.SuppressAsyncSuffixInActionNames = false;
        });

        return services;
    }
}
