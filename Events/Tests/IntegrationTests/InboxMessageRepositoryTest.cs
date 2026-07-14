using Events.Application.Exceptions;
using Events.Application.Models.Messages;
using Events.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Events.IntegrationTests;

public class InboxMessageRepositoryTest : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private readonly ILogger<InboxMessageRepository> _logger = NullLogger<InboxMessageRepository>.Instance;

    public InboxMessageRepositoryTest(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AddAsync_SavesInboxMessageToDataBase()
    {
        // Arrange
        var id = Guid.NewGuid();
        var inboxMessage = new InboxMessage(id);
        var rep = new InboxMessageRepository(_fixture.Context, _logger);

        // Act        
        var res = await rep.AddAsync(inboxMessage, CancellationToken.None);

        // Assert
        res.Should().NotBeNull();
        res.Should().BeEquivalentTo(inboxMessage);
    }

    [Fact]
    public async Task AddAsync_DublicateId_ThrowsDublicateInsertionException()
    {
        // Arrange
        using var context = _fixture.Context;
        var id = Guid.NewGuid();
        var inboxMessage = new InboxMessage(id);
        await context.AddAsync(inboxMessage);
        await context.SaveChangesAsync();
        var rep = new InboxMessageRepository(_fixture.Context, _logger);

        // Act        
        Func<Task> act = async () => await rep.AddAsync(inboxMessage, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DublicateInsertionException>();
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