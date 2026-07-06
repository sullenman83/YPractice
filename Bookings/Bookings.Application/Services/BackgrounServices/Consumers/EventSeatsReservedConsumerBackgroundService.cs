using Bookings.Application.Interfaces.Consumers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bookings.Application.Services.BackgrounServices.Consumers;

/// <summary>
/// Фоновый сервис для обработки сообщений об успешном списание мест в событии
/// </summary>
public class EventSeatsReservedConsumerBackgroundService : BackgroundService
{
    private readonly IEventSeatsReservedConsumer _consumer;
    private readonly ILogger<EventSeatsReservedConsumerBackgroundService> _logger;

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="consumer">Консьюмер</param>
    /// <param name="logger">Логер</param>
    public EventSeatsReservedConsumerBackgroundService(IEventSeatsReservedConsumer consumer, ILogger<EventSeatsReservedConsumerBackgroundService> logger)
    {
        _consumer = consumer;
        _logger = logger;
    }

    /// <summary>
    /// Метод запуска сервиса
    /// </summary>
    /// <param name="stoppingToken">Токен отмены</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Фоновый сервис BookingProducer запущен.");
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


        _logger.LogInformation("Фоновый сервис обработки бронирований остановлен.");
    }
}
