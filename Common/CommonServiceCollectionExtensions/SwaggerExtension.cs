
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;

namespace CommonServiceCollectionExtensions;

/// <summary>
/// Расширение для добавления зависимостей
/// </summary>
public static class SwaggerExtension
{
    /// <summary>
    /// добавить зависимости 
    /// </summary>
    /// <param name="services">Коллекция сервисов</param>
    /// <param name="env">Окружение</param>
    /// <returns>Коллекция сервисов</returns>
    public static IServiceCollection AddSwager(this IServiceCollection services, IHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            services.AddSwaggerGen(options =>
            {
                var baseDirectory = AppContext.BaseDirectory;
                var xmlFiles = Directory.GetFiles(baseDirectory, "*.xml", SearchOption.TopDirectoryOnly);
                foreach (var f in xmlFiles)
                {
                    options.IncludeXmlComments(f);
                }

                options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme()
                {
                    In = ParameterLocation.Header,
                    Description = "Ведите ваш токен",
                    Name = "Аутентификация",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = JwtBearerDefaults.AuthenticationScheme
                });

                options.AddSecurityRequirement(document => new OpenApiSecurityRequirement()
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                });
            });
        }
        services.AddControllers(options =>
        {
            options.SuppressAsyncSuffixInActionNames = false;
        });

        services.AddHttpContextAccessor();

        return services;
    }
}
