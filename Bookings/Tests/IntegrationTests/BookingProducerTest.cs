
using Contracts;

namespace Bookings.IntegrationTests;

public class BookingProducerTest(DatabaseFixture databaseFixture, KafkaFixture kafkaFixture) : IClassFixture<DatabaseFixture>, 
    IClassFixture<KafkaFixture>, IAsyncLifetime
{
    private readonly DatabaseFixture _databaseFixture = databaseFixture;
    private readonly KafkaFixture _kafkaFixture = kafkaFixture;
    






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
