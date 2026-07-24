
using Bookings.Infrastructure.Services.Producers;
using Bookings.Infrastructure.Settings;
using Confluent.Kafka;
using Contracts;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Bookings.IntegrationTests;

public class BookingProducerTest: IClassFixture<DatabaseFixture>, IClassFixture<KafkaFixture>, IAsyncLifetime
{
    private readonly DatabaseFixture _databaseFixture;
    private readonly KafkaFixture _kafkaFixture;
    private readonly IOptions<BookingProducerSettings> _options;
    private readonly ILogger<BookingProducer> _logger = NullLogger<BookingProducer>.Instance;

    public BookingProducerTest(DatabaseFixture databaseFixture, KafkaFixture kafkaFixture)
    {
        _databaseFixture = databaseFixture;
        _kafkaFixture = kafkaFixture;

        var settings = new BookingProducerSettings()
        {
            BootstrapServers = _kafkaFixture.BootstrapServers,
        };
        _options = Options.Create(settings);
    }

    [Fact]
    public async Task Produce_DeliveredMessage()
    {
        // Arrange
        var producer = new BookingProducer(_options, _logger);
        var key = "testKey";
        var value = "testvalue";
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _kafkaFixture.BootstrapServers,
            GroupId = "test-group",
            AutoOffsetReset = AutoOffsetReset.Earliest 
        };
        using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
        consumer.Subscribe(TopicNames.BookingConfirmed);

        // Act
        await producer.ProduceAsync(TopicNames.BookingConfirmed, key, value, CancellationToken.None);
        var consumeResult = consumer.Consume(TimeSpan.FromSeconds(10));
        
        // Assert:                
        consumeResult.Should().NotBeNull();
        consumeResult.Message.Value.Should().Be(value);
        consumer.Close();
    }    

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task InitializeAsync()
    {
        await _kafkaFixture.ResetTopicsAsync(TopicNames.BookingConfirmed, TopicNames.BookingCancelled);
        await _databaseFixture.ResetDatabaseAsync();
    }
}
