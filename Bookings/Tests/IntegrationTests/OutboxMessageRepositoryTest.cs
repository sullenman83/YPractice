using Bookings.Application.Models.Messages;
using Bookings.Infrastructure.Services.Repositories.MessageRepositories;
using DateTimeManager.Abstractions;
using DateTimeManager.Core;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bookings.IntegrationTests;

public class OutboxMessageRepositoryTest(DatabaseFixture fixture) : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly IDateTimeProvider _dateTimeProvider = new DateTimeProvider();
    private readonly DatabaseFixture _fixture = fixture;
    private readonly ILogger<OutboxMessageRepository> _logger = NullLogger<OutboxMessageRepository>.Instance;

    [Fact]
    public async Task GetUnprocessed_ReturnsOutboxMEssagesWithProcessedISFalse()
    {
        // Arrange        
        await using var context = _fixture.Context;
        var id = Guid.NewGuid();
        var key = Guid.NewGuid();
        var messageType = "mesagetype";
        var payload = "payload";

        var outboxMessage = new OutboxMessage(id, key, messageType, _dateTimeProvider.GetUtcNow(), payload, 0, false);
        await context.OutboxMessages.AddAsync(outboxMessage);
        var outboxMessage1 = new OutboxMessage(Guid.NewGuid(), key, messageType, _dateTimeProvider.GetUtcNow(), payload, 0, true);
        await context.OutboxMessages.AddAsync(outboxMessage1);
        await context.SaveChangesAsync();
        
        // Act
        var rep = new OutboxMessageRepository(_fixture.Context, _logger);
        var res = await rep.GetUnprocessed(CancellationToken.None);

        // Assert
        res.Should().NotBeEmpty();
        res.Should().Contain(outboxMessage);
    }

    [Fact]
    public async Task AddOutboxMessage_OutboxMessageToDataBase()
    {
        // Arrange
        var id = Guid.NewGuid();
        var key = Guid.NewGuid();
        var messageType = "mesagetype";
        var payload = "payload";        
        var rep = new OutboxMessageRepository(_fixture.Context, _logger);        
        var outboxMessage = new OutboxMessage(id, key, messageType, _dateTimeProvider.GetUtcNow(), payload, 0, false);

        // Act
        var res = await rep.AddAsync(outboxMessage, CancellationToken.None);
        
        // Assert
        res.Should().NotBeNull();
        res.Should().BeEquivalentTo(outboxMessage);
    }

    [Fact]
    public async Task SaveChanges_SavesBookingToDataBase()
    {
        // Arrange
        var context = _fixture.Context;
        var id = Guid.NewGuid();
        var key = Guid.NewGuid();
        var messageType = "mesagetype";
        var payload = "payload";        
        var outboxMessage = new OutboxMessage(id, key, messageType, _dateTimeProvider.GetUtcNow(), payload, 0, false);
        await context.OutboxMessages.AddAsync(outboxMessage);
        await context.SaveChangesAsync();
        var rep = new OutboxMessageRepository(_fixture.Context, _logger);

        // Act
        var list = await rep.GetUnprocessed(CancellationToken.None);
        var m = list.FirstOrDefault(o => o.Id == id);
        if (m == null)
            throw new InvalidOperationException("Что-то работает не так");
        m.Processed = true;
        await rep.SaveChangesAsync(CancellationToken.None);


        // Assert
        await using var ctx = _fixture.Context;
        var changedMessage = await ctx.OutboxMessages.FirstOrDefaultAsync(o => o.Id == id);
        changedMessage.Should().NotBeNull();
        changedMessage.Processed.Should().BeTrue();
    }

    public async Task InitializeAsync()
    {
        await _fixture.ResetDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}