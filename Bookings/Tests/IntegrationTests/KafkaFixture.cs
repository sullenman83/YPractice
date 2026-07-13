using Testcontainers.Kafka;

namespace Bookings.IntegrationTests;

internal class KafkaFixture : IAsyncLifetime
{
    private readonly KafkaContainer _kafkaContainer;

    public KafkaFixture()
    {
        _kafkaContainer = new KafkaBuilder("confluentinc/cp-kafka:7.6.0")            
            .Build();
    }

    public string BootstrapServers => _kafkaContainer.GetBootstrapAddress();

    public async Task DisposeAsync()
    {
        await _kafkaContainer.StopAsync().ConfigureAwait(false);
        await _kafkaContainer.DisposeAsync().ConfigureAwait(false);
    }

    public async Task InitializeAsync()
    {
        await _kafkaContainer.StartAsync().ConfigureAwait(false);
    }
}
