using CommonServiceCollectionExtensions;
using Events.Presentation.Settings;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Events.Presentation.Extensions;

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
    /// <param name="configuration">Конфигурация</param>
    /// <returns>Коллекция сервисов</returns>
    public static IServiceCollection AddPresentation(this IServiceCollection services, IHostEnvironment env, IConfiguration configuration)
    {
        services.AddSecurity(configuration);
        services.AddSwager(env);
        var otlpSettings = configuration.GetSection(nameof(OtlpSettings)).Get<OtlpSettings>() ?? throw new InvalidOperationException("Не заданы настройки для OLTP");

        services.AddControllers(options =>
        {
            options.SuppressAsyncSuffixInActionNames = false;
        });
        if (!Enum.TryParse<OtlpExportProtocol>(otlpSettings.Protocol, true, out var parsedProtocol))
        {
            parsedProtocol = OtlpExportProtocol.HttpProtobuf;
        }

        services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService("events-service"))
            .WithMetrics(metrics =>
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddPrometheusExporter()
                )
            .WithTracing(t => t
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.Filter = httpContext =>
                        !httpContext.Request.Path.StartsWithSegments("/health")
                         && !httpContext.Request.Path.StartsWithSegments("/metrics");
                })
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(otlpSettings.Endpoint);
                    options.Protocol = parsedProtocol;
                    options.BatchExportProcessorOptions.ScheduledDelayMilliseconds = otlpSettings.ScheduledDelayMilliseconds;
                    options.BatchExportProcessorOptions.ExporterTimeoutMilliseconds = otlpSettings.ExporterTimeoutMilliseconds;
                }));

        return services;
    }
}
