using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Events.Application.Services.BackgroundServices;

/// <summary>
/// Фоновый сервис для обработки сообщений  BookingConfirmed 
/// </summary>
public class BookingConfirmedConsumerBackgroundService : BackgroundService
{
    private readonly ILogger<BookingConfirmedConsumerBackgroundService> _logger;

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="logger">Логер</param>
    public BookingConfirmedConsumerBackgroundService(ILogger<BookingConfirmedConsumerBackgroundService> logger)
    {
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Фоновый сервис BookingConfirmedConsumerBackgroundService запущен.");
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {

                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при бронировании события.");
                }

            }
        }
        finally
        {

        }


        _logger.LogInformation("Фоновый сервис BookingConfirmedConsumerBackgroundService остановлен.");
    }
}
