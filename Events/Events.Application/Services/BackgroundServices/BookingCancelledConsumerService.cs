using Events.Application.Interfaces.Consumers;
using Events.Application.Interfaces.MessageHandlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Events.Application.Services.BackgroundServices;

/// <summary>
/// Фоновый сервис для обработки сообщений  BookingCancelled/// 
/// </summary>
internal class BookingCancelledConsumerService : BackgroundService
{
    private readonly ILogger<BookingConfirmedConsumerService> _logger;
    private readonly IServiceScopeFactory _factory;
    private readonly IBookingCancelledConsumer _consumer;

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="logger">Логер</param>
    /// <param name="consumer">Консьюмер</param>
    /// <param name="factory">фабрика сервисов</param>
    public BookingCancelledConsumerService(ILogger<BookingConfirmedConsumerService> logger,
        IServiceScopeFactory factory,
        IBookingCancelledConsumer consumer)
    {
        _logger = logger;
        _factory = factory;
        _consumer = consumer;
    }

    ///<inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Фоновый сервис BookingCancelledConsumerService запущен.");

        stoppingToken.Register(() => _consumer.Dispose());

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _factory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<IBookingCancelledHandler>();
                _consumer.Consume(handler.HandleMessage, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при освобождении мест события.");
            }
        }
        _logger.LogInformation("Фоновый сервис BookingCancelledConsumerService остановлен.");
    }
}
