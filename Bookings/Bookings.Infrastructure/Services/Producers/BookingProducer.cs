using Bookings.Application.Exceptions;
using Bookings.Application.Interfaces;
using Bookings.Infrastructure.Settings;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bookings.Infrastructure.Services.Producers;

/// <summary>
/// Класс для публикации сообщение в брокер сообщений
/// </summary>
public class BookingProducer: IDisposable, IBookingProduсer
{    
    private readonly ProducerConfig _config;
    private readonly IProducer<string,string> _producer;
    private readonly ILogger _logger;

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="options">Настройки продюсера</param>
    /// <exception cref="ArgumentNullException"></exception>
    public BookingProducer(IOptions<BookingProducerSettings> options, ILogger<BookingProducer> logger)
    {
        _logger = logger;
        var settings = options.Value ?? throw new ArgumentNullException("Не заданы настройки продюсера.");

        _config = new ProducerConfig()
        {
            BootstrapServers = settings.BootstrapServers,
            Acks = Acks.All
        };

        _producer = new ProducerBuilder<string, string>(_config).Build();
    }

    /// <summary>
    /// Очистить ресурсы
    /// </summary>
    public void Dispose()
    {
        _producer?.Flush(TimeSpan.FromSeconds(10));
        _producer?.Dispose();
    }

    ///<inheritdoc/>
    public async Task ProduceAsync(string topic, string key, string value, CancellationToken token)
    {
        try
        {
            await _producer.ProduceAsync(topic, new Message<string, string> { Key = key, Value = value }, token);
        }
        catch(ProduceException<Null, string> ex)
        {
            _logger.LogError("Ошибка отправки сообщения {Key}, {Value} в Kafka топик {Topic} причина: {Reason}", key, value, topic, ex.Error.Reason);
            throw new BookingProducerException($"Ошибка отправки сообщения в Kafka: {ex.Error.Reason}");
        }
    }
}
