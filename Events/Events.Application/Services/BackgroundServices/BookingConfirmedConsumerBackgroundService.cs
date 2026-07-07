using Events.Application.Interfaces.Consumers;
using Events.Application.Interfaces.MessageHandlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Events.Application.Services.BackgroundServices;

/// <summary>
/// Фоновый сервис для обработки сообщений  BookingConfirmed 
/// </summary>
public class BookingConfirmedConsumerBackgroundService : BackgroundService
{
    private readonly ILogger<BookingConfirmedConsumerBackgroundService> _logger;
    private readonly IServiceScopeFactory _factory;
    private readonly IBookingConfirmedConsumer _consumer;

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="logger">Логер</param>
    /// <param name="consumer">Консьюмер</param>
    /// <param name="factory">фабрика сервисов</param>
    public BookingConfirmedConsumerBackgroundService(ILogger<BookingConfirmedConsumerBackgroundService> logger,
        IServiceScopeFactory factory,
        IBookingConfirmedConsumer consumer)
    {
        _logger = logger;
        _factory = factory;
        _consumer = consumer;
    }

    ///<inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Фоновый сервис BookingConfirmedConsumerBackgroundService запущен.");

        stoppingToken.Register(() => _consumer.Dispose());

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _factory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<IBookingConfirmedHandler>();
                _consumer.Consume(handler.HandleMessage, stoppingToken);
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
        _logger.LogInformation("Фоновый сервис BookingConfirmedConsumerBackgroundService остановлен.");
    }
}
