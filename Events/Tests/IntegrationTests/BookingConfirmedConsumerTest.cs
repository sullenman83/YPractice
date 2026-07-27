using Confluent.Kafka;
using Contracts;
using Events.Infrastructure.Services.Consumers;
using Events.Infrastructure.Settings.ConsumerSettings;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Text.Json;


namespace Events.IntegrationTests;

public class BookingConfirmedConsumerTest : IClassFixture<DatabaseFixture>, IClassFixture<KafkaFixture>, IAsyncLifetime
{
    private readonly DatabaseFixture _databaseFixture;
    private readonly KafkaFixture _kafkaFixture;
    private readonly IProducer<string, string> _producer;
    private readonly IOptions<BookingConfirmedConsumerSettings> _options;
    private readonly string _groupId = "BookingConfirmedConsumers";
    private readonly ILogger<BookingConfirmedConsumer> _logger = NullLogger<BookingConfirmedConsumer>.Instance;

    public BookingConfirmedConsumerTest(DatabaseFixture databaseFixture, KafkaFixture kafkaFixture)
    {
        _databaseFixture = databaseFixture;
        _kafkaFixture = kafkaFixture;

        var config = new ProducerConfig()
        {
            BootstrapServers = _kafkaFixture.BootstrapServers,
            Acks = Acks.All
        };
        _producer = new ProducerBuilder<string, string>(config).Build();

        var settings = new BookingConfirmedConsumerSettings()
        {
            BootstrapServers = _kafkaFixture.BootstrapServers,
            GroupId = _groupId,
            Topic = TopicNames.BookingConfirmed
        };
        _options = Options.Create(settings);
    }

    [Fact]
    public async Task ConsumeAsync_NoThrows()
    {
        // Arrange
        var key = Guid.NewGuid().ToString();
        var messageId = Guid.NewGuid();
        var bookingConfirmedMessage = new BookingConfirmed(messageId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1, DateTimeOffset.UtcNow);
        var payload = JsonSerializer.Serialize(bookingConfirmedMessage);
        using var consumer = new BookingConfirmedConsumer(_options, _logger);
        var config = new AdminClientConfig() { BootstrapServers = _kafkaFixture.BootstrapServers };
        var admin = new AdminClientBuilder(config).Build();
        var isIdentical = false;
        var message = new Message<string, string> { Key = key, Value = payload };

        // Act
        await _producer.ProduceAsync(TopicNames.BookingConfirmed, message);
        await Task.Delay(400);
        Func<Task> act = async () => await consumer.ConsumeAsync((BookingConfirmed m, CancellationToken token) =>
        {
            if (bookingConfirmedMessage.UserId == m.UserId
                && bookingConfirmedMessage.BookingId == m.BookingId
                && bookingConfirmedMessage.ConfirmDate == m.ConfirmDate
                && bookingConfirmedMessage.EventId == m.EventId
                && bookingConfirmedMessage.MessageId == m.MessageId
                && bookingConfirmedMessage.SeatsCount == m.SeatsCount)
                isIdentical = true;

            return Task.CompletedTask;
        }
            , CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
        isIdentical.Should().BeTrue();
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

    private async Task<bool> IsEmptyConsumerGroup(IAdminClient adminClient)
    {
        var describe = await adminClient.DescribeConsumerGroupsAsync(new[] { _groupId });
        var metaData = describe.ConsumerGroupDescriptions.FirstOrDefault();

        if (metaData == null)
        {
            throw new ArgumentException("что-то работает не так");
        }

        return metaData.State == ConsumerGroupState.Empty;
    }
}
