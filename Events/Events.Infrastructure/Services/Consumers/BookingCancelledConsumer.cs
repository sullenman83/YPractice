using Confluent.Kafka;
using Contracts;
using Events.Application.Exceptions;
using Events.Application.Interfaces.Consumers;
using Events.Infrastructure.Settings.ConsumerSettings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Events.Infrastructure.Services.Consumers;

/// <summary>
/// Консьюмер для обработки отмены бронирования
/// </summary>
public class BookingCancelledConsumer : IBookingCancelledConsumer
{
    private readonly IConsumer<string, string> _consumer;
    private readonly ILogger<BookingCancelledConsumer> _logger;

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="options">Настройки консьюмера</param>
    /// <param name="logger">логер</param>
    /// <exception cref="ArgumentNullException">Не заданы настройки</exception>
    public BookingCancelledConsumer(IOptions<BookingCancelledConsumerSettings> options, ILogger<BookingCancelledConsumer> logger)
    {
        _logger = logger;
        var settings = options.Value ?? throw new ArgumentNullException("Не заданы настройки консьюмера 'BookingCancelledConsumer'");
        if (string.IsNullOrEmpty(settings.BootstrapServers)
            || string.IsNullOrEmpty(settings.GroupId)
            || string.IsNullOrEmpty(settings.Topic))
            throw new ArgumentNullException("Неверно заданы настройки консьюмера 'BookingCancelledConsumer'");

        var cfg = new ConsumerConfig()
        {
            BootstrapServers = settings.BootstrapServers,
            GroupId = settings.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            EnableAutoOffsetStore = false,
            AllowAutoCreateTopics = true
        };

        _consumer = new ConsumerBuilder<string, string>(cfg).Build();
        _consumer.Subscribe(settings.Topic);
    }

    ///<inheritdoc/>
    public async Task ConsumeAsync(Func<BookingCancelled, CancellationToken, Task> messageHandler, CancellationToken token)
    {
        try
        {
            var result = _consumer.Consume(token);
            if (result == null)
            {
                return;
            }

            var bookingCancelled = JsonSerializer.Deserialize<BookingCancelled>(result.Message.Value);
            if (bookingCancelled == null)
            {
                _logger.LogError("Ошибка при десериализации ссобщения Kafka {Message}.", result.Message.Value);
                throw new InvalidOperationException("Ошибка при десериализации ссобщения Kafka.");
            }

            await messageHandler(bookingCancelled, token);

            _consumer.StoreOffset(result);
            _consumer.Commit();
        }
        catch (ConsumeException ex)
        {
            if (ex.Error.IsFatal)
            {
                //ToDo: тут надо как-то перебилдить консьюмер
            }
            _logger.LogError(ex, "Ошибка получения сообщения из Kafka: {Reason}", ex.Error.Reason);
            throw new ConsumerException($"Ошибка получения сообщения из Kafka: {ex.Error.Reason}");
        }
    }

    /// <summary>
    /// Очистка ресурсов
    /// </summary>
    public void Dispose()
    {
        _consumer?.Close();
        _consumer?.Dispose();
    }
}
