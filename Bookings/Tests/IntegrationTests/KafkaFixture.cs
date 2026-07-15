using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Testcontainers.Kafka;

namespace Bookings.IntegrationTests;

public class KafkaFixture : IAsyncLifetime
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

    public async Task ResetTopicsAsync(params string[] topicNames)
    {
        var config = new AdminClientConfig { BootstrapServers = BootstrapServers };
        var adminClient = new AdminClientBuilder(config).Build();
        try
        {
            await adminClient.DeleteTopicsAsync(topicNames).ConfigureAwait(false);
            await Task.Delay(200);           
        }
        catch (DeleteTopicsException)
        {           
        }

        var specifications = topicNames.Select(name => new TopicSpecification()
        {
            Name = name,
            NumPartitions = 1,
            ReplicationFactor = 1
        });
        await adminClient.CreateTopicsAsync(specifications);
    }
}