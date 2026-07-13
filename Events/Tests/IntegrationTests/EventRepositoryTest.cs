using DateTimeManager.Abstractions;
using DateTimeManager.Core;
using Events.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using TestData;

namespace Events.IntegrationTests;


public class EventRepositoryTest : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private readonly IDateTimeProvider _dateTimeProvider = new DateTimeProvider();
    private readonly ILogger<EventRepository> _logger = NullLogger<EventRepository>.Instance;

    public EventRepositoryTest(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetEventById_ReturnsEvent()
    {
        // Arrange
        await using var context = _fixture.Context;
        var ev = EventTestData.GetTestEvent();
        var id = ev.Id;
        await context.Events.AddAsync(ev);
        await context.SaveChangesAsync();

        // Act
        var rep = new EventRepository(_fixture.Context, _logger);
        var res = await rep.GetByIdAsync(id, CancellationToken.None);

        // Assert
        res.Should().NotBeNull();
        res.Should().BeEquivalentTo(ev);
    }

    [Fact]
    public async Task GetEventById_IncorrectId_ReturnsNull()
    {
        // Arrange
        await using var context = _fixture.Context;
        var ev = EventTestData.GetTestEvent();
        var id = ev.Id;
        await context.Events.AddAsync(ev);
        await context.SaveChangesAsync();

        // Act
        var rep = new EventRepository(_fixture.Context, _logger);
        var res = await rep.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        res.Should().BeNull();
    }

    [Fact]
    public async Task AddEvent_SavesEventToDataBase()
    {
        // Arrange
        await using var context = _fixture.Context;
        var ev = EventTestData.GetTestEvent();
        var id = ev.Id;

        // Act
        var rep = new EventRepository(context, _logger);
        var res = await rep.AddAsync(ev, CancellationToken.None);


        // Assert
        await using var ctx = _fixture.Context;
        var savedEvent = await ctx.Events.FirstOrDefaultAsync(e => e.Id == id);
        savedEvent.Should().NotBeNull();
        savedEvent.Should().BeEquivalentTo(ev);
    }

    [Fact]
    public async Task DeleteEvent_DeleletesBooking()
    {
        // Arrange
        await using var context = _fixture.Context;
        var ev = EventTestData.GetTestEvent();        
        await context.SaveChangesAsync();
        
        // Act
        var rep = new EventRepository(_fixture.Context, _logger);
        await rep.DeleteAsync(ev.Id, CancellationToken.None);

        // Assert
        await using var ctx = _fixture.Context;        
        var e = await ctx.Events.FirstOrDefaultAsync(e => e.Id == ev.Id);
        e.Should().BeNull();
    }

    [Fact]
    public async Task DeleteEvent_IncorrectId_ReturnsFalse()
    {
        // Arrange            

        // Act
        var rep = new EventRepository(_fixture.Context, _logger);
        var res = await rep.DeleteAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        res.Should().BeFalse();
    }   

    [Fact]
    public async Task SaveChanges()
    {
        // Arrange
        await using var context = _fixture.Context;
        var ev = EventTestData.GetTestEvent();
        await context.Events.AddAsync(ev);
        await context.SaveChangesAsync();
        var title = ev.Title + "Changed";

        // Act
        var rep = new EventRepository(_fixture.Context, _logger);
        var e = await rep.GetByIdAsync(ev.Id, CancellationToken.None);
        if (e == null)
            throw new InvalidOperationException("Что-то работает не так");
        e.Title = title;
        await rep.SaveChangesAsync(CancellationToken.None);


        // Assert
        await using var ctx = _fixture.Context;
        var changedEvent = await ctx.Events.FirstOrDefaultAsync(o => o.Id == ev.Id);
        changedEvent.Should().NotBeNull();
        changedEvent.Title.Should().Be(title);
    }

    
    [Fact]
    public async Task SaveEvent_EndAtLessStartAt_ThrowsDbUpdateException()
    {
        // Arrange
        var ev = EventTestData.GetTestEvent();
        ev.EndAt = ev.StartAt.AddDays(-1);
        var ctx = _fixture.Context;

        // Act
        await ctx.Events.AddAsync(ev);
        Func<Task> act = async () => await ctx.SaveChangesAsync();

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task SaveEvent_NegativeTotalSeats_ThrowsDbUpdateException()
    {
        // Arrange
        var ev = EventTestData.GetTestEvent();
        var ctx = _fixture.Context;
        var seats = -1;

        // Act
        Func<Task> act = async () => await ctx.Database.ExecuteSqlInterpolatedAsync(
$@"INSERT INTO events(id, title, description, start_at, end_at, total_seats, available_seats)
        VALUES({ev.Id}, {ev.Title}, {ev.Description}, {ev.StartAt}, {ev.EndAt}, {seats}, {ev.AvailableSeats})");

        // Assert
        await act.Should().ThrowAsync<PostgresException>();
    }

    [Fact]
    public async Task SaveEvent_TotalSeatsLessAvailableSeats_ThrowsDbUpdateException()
    {
        // Arrange
        var ev = EventTestData.GetTestEvent();
        var ctx = _fixture.Context;
        var availableSeats = ev.TotalSeats + 1;

        // Act
        Func<Task> act = async () => await ctx.Database.ExecuteSqlInterpolatedAsync(
$@"INSERT INTO events(id, title, description, start_at, end_at, total_seats, available_seats)
        VALUES({ev.Id}, {ev.Title}, {ev.Description}, {ev.StartAt}, {ev.EndAt}, {ev.TotalSeats}, {availableSeats})");

        // Assert
        await act.Should().ThrowAsync<PostgresException>();
    }

    [Fact]
    public async Task SaveEvent_EmptyTitle_ThrowsDbUpdateException()
    {
        // Arrange
        var ev = EventTestData.GetTestEvent();
        ev.Title = "";
        var ctx = _fixture.Context;

        // Act
        await ctx.Events.AddAsync(ev);
        Func<Task> act = async () => await ctx.SaveChangesAsync();

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();
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

