using Bookings.Domain.Models;
using Bookings.Infrastructure.Services.Repositories.BookingRepository;
using DateTimeManager.Abstractions;
using DateTimeManager.Core;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using UserRooles;

namespace Bookings.IntegrationTests;

public class BookingRepositoryTest(DatabaseFixture fixture) : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly IDateTimeProvider _dateTimeProvider = new DateTimeProvider();
    private readonly DatabaseFixture _fixture = fixture;
    private readonly ILogger<BookingRepository> _logger = NullLogger<BookingRepository>.Instance;

    [Fact]
    public async Task GetBookingById_ReturnsBooking()
    {
        // Arrange        
        await using var context = _fixture.Context;
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var seatsCount = 1;
        var status = BookingStatus.Pending;
        var booking = new Booking(status, eventId, userId, seatsCount, _dateTimeProvider.GetUtcNow());
        await context.Bookings.AddAsync(booking);
        await context.SaveChangesAsync();
        var id = booking.Id;

        // Act
        var rep = new BookingRepository(_fixture.Context, _logger);
        var res = await rep.GetByIdAsync(id, CancellationToken.None);

        // Assert
        res.Should().NotBeNull();
        res.Should().BeEquivalentTo(booking);        
    }

    [Fact]
    public async Task GetBookingById_IncorrectId_ReturnsNull()
    {
        // Arrange
        await using var context = _fixture.Context;
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var seatsCount = 1;
        var status = BookingStatus.Pending;
        var booking = new Booking(status, eventId, userId, seatsCount, _dateTimeProvider.GetUtcNow());
        await context.Bookings.AddAsync(booking);
        await context.SaveChangesAsync();        

        // Act
        var rep = new BookingRepository(_fixture.Context, _logger);
        var res = await rep.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        res.Should().BeNull();
    }

    [Fact]
    public async Task AddBooking_SavesBookingToDataBase()
    {
        // Arrange                
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var seatsCount = 1;
        var status = BookingStatus.Pending;
        var booking = new Booking(status, eventId, userId, seatsCount, _dateTimeProvider.GetUtcNow());                

        // Act
        var rep = new BookingRepository(_fixture.Context, _logger);
        var res = await rep.AddAsync(booking, CancellationToken.None);

        // Assert
        await using var ctx = _fixture.Context;
        var savedBooking = await ctx.Bookings
            .FirstOrDefaultAsync(e => e.Id == booking.Id);
        savedBooking.Should().NotBeNull();
        savedBooking.Should().BeEquivalentTo(booking);
    }
    
    [Fact]
    public async Task SaveChanges_SavesBookingToDataBase()
    {
        // Arrange
        await using var context = _fixture.Context;
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var seatsCount = 1;
        var status = BookingStatus.Pending;
        var booking = new Booking(status, eventId, userId, seatsCount, _dateTimeProvider.GetUtcNow());
        await context.Bookings.AddAsync(booking);
        await context.SaveChangesAsync();

        // Act
        var rep = new BookingRepository(_fixture.Context, _logger);
        var b = await rep.GetByIdAsync(booking.Id, CancellationToken.None);
        if (b == null)
            throw new InvalidOperationException("Что-то работает не так");
        b.Confirm(_dateTimeProvider.GetUtcNow());
        await rep.SaveChangesAsync(CancellationToken.None);


        // Assert
        await using var ctx = _fixture.Context;
        var changedBooking = await ctx.Bookings.FirstOrDefaultAsync(o => o.Id == b.Id);
        changedBooking.Should().NotBeNull();
        changedBooking.Status.Should().Be(BookingStatus.Confirmed);
    }
    
    [Fact]
    public async Task SaveBooking_IncorrectEnum_ThrowsDbUpdateException()
    {
        // Arrange
        await using var context = _fixture.Context;
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var seatsCount = 1;        
        var booking = new Booking((BookingStatus)256, eventId, userId, seatsCount, _dateTimeProvider.GetUtcNow());

        // Act
        context.Bookings.Add(booking);
        Func<Task> act = async () => await context.SaveChangesAsync();

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task SaveBooking_NegativeSeatsCount_ThrowsDbUpdateException()
    {
        // Arrange
        await using var context = _fixture.Context;
        var id = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var status = BookingStatus.Pending;
        var seatsCount = -1;
        var createdAt = _dateTimeProvider.GetUtcNow();
        var processedAt = _dateTimeProvider.GetUtcNow();

        // Act        
        Func<Task> act = () => context.Database.ExecuteSqlInterpolatedAsync(
$@"INSERT INTO bookings(id, status, seats_count, created_at, processed_at, event_id, user_id)
     VALUES({id}, {status}, {seatsCount} , {createdAt}, {processedAt}, {eventId}, {userId})");

        // Assert
        await act.Should().ThrowAsync<PostgresException>();
    }

    [Fact]
    public async Task GetActiveBookingForUser_ReturnsCorrectBookingCount()
    {
        // Arrange
        var events = new List<Guid>() { Guid.NewGuid(), Guid.NewGuid() };
        var users = new List<Guid>() { Guid.NewGuid(), Guid.NewGuid() };
        var ctx = _fixture.Context;

        var bookings = new List<Booking>()
        {
            new Booking(BookingStatus.Pending, events[0], users[0], 1, DateTimeOffset.UtcNow),
            new Booking(BookingStatus.Pending, events[0], users[1], 1, DateTimeOffset.UtcNow),

            new Booking(BookingStatus.Pending, events[1], users[0], 1, DateTimeOffset.UtcNow),
            new Booking(BookingStatus.Pending, events[1], users[1], 1, DateTimeOffset.UtcNow),
            new Booking(BookingStatus.Confirmed, events[1], users[1], 1, DateTimeOffset.UtcNow),
            new Booking(BookingStatus.Cancelled, events[1], users[1], 1, DateTimeOffset.UtcNow),
            new Booking(BookingStatus.Rejected, events[1], users[1], 1, DateTimeOffset.UtcNow),
        };
        await ctx.Bookings.AddRangeAsync(bookings);
        await ctx.SaveChangesAsync();

        var bookingRepository = new BookingRepository(_fixture.Context, _logger);

        // Act
        var res = await bookingRepository.GetActiveUserBookingAsync(users[1]);

        // Assert
        res.Should().HaveCount(3);
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
