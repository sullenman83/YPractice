using EventManagement.Common;
using EventManagement.Interfaces;
using EventManagement.Models.BookingModels;
using EventManagement.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using System.Runtime.CompilerServices;

namespace EventApi.IntegrationTest;

public class BookingRepositoryTest(DatabaseFixture fixture) : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly IDateTimeProvider _dateTimeProvider = new DateTimeProvider();
    private readonly DatabaseFixture _fixture = fixture;
        
    [Fact]
    public async Task GetBookingById_ReturnsBooking()
    {
        // Arrange        
        await using var context = _fixture.Context;
        var ev = TestData.GetTestEvent();
        await context.Events.AddAsync(ev);
        await context.SaveChangesAsync();
        var booking = TestData.GetTestBooking(ev, _dateTimeProvider.UtcNow);        
        await context.Bookings.AddAsync(booking);
        await context.SaveChangesAsync();
        var id = booking.Id;

        // Act
        var rep = new BookingRepository(_fixture.Context);
        var res = await rep.GetByIdAsync(id, CancellationToken.None);

        // Assert
        res.Should().NotBeNull();
        res.EventId.Should().Be(booking.EventId);
        res.Id.Should().Be(booking.Id);
        res.Status.Should().Be(booking.Status);
        res.CreatedAt.Should().Be(booking.CreatedAt);
    }

    [Fact]
    public async Task GetBookingById_IncorrectId_ReturnsNull()
    {
        // Arrange
        await using var context = _fixture.Context;
        var ev = TestData.GetTestEvent();
        var booking = TestData.GetTestBooking(ev, _dateTimeProvider.UtcNow);
        await context.Events.AddAsync(ev);
        await context.SaveChangesAsync();
        await context.Bookings.AddAsync(booking);
        await context.SaveChangesAsync();
        var id = booking.Id;

        // Act
        var rep = new BookingRepository(_fixture.Context);
        var res = await rep.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        res.Should().BeNull();
    }

    [Fact]
    public async Task AddBooking_SavesBookingToDataBase()
    {
        // Arrange        
        await using var context = _fixture.Context;
        var ev = TestData.GetTestEvent();        
        await context.Events.AddAsync(ev);
        await context.SaveChangesAsync();
        var booking = TestData.GetTestBooking(ev, _dateTimeProvider.UtcNow);
        var id = booking.Id;

        // Act
        var rep = new BookingRepository(context);
        var res = await rep.AddAsync(booking, CancellationToken.None);


        // Assert
        await using var ctx = _fixture.Context;
        var savedBooking = await ctx.Bookings            
            .FirstOrDefaultAsync(e => e.Id == id);
        savedBooking.Should().NotBeNull();
        savedBooking.EventId.Should().Be(booking.EventId);
        savedBooking.Id.Should().Be(booking.Id);
        savedBooking.Status.Should().Be(booking.Status);
        savedBooking.CreatedAt.Should().Be(booking.CreatedAt);
    }

    [Fact]
    public async Task DeleteEvent_DeletesBooking()
    {
        // Arrange
        await using var context = _fixture.Context;
        var ev = TestData.GetTestEvent();        
        var booking = TestData.GetTestBooking(ev, _dateTimeProvider.UtcNow);
        await context.Events.AddAsync(ev);
        await context.SaveChangesAsync();
        await context.Bookings.AddAsync(booking);
        await context.SaveChangesAsync();
        var id = booking.Id;

        // Act
        var rep = new BookingRepository(_fixture.Context);
        await rep.DeleteAsync(id, CancellationToken.None);

        // Assert
        await using var ctx = _fixture.Context;
        var b = await ctx.Bookings.FirstOrDefaultAsync(o => o.Id == id);
        b.Should().BeNull();
    }

    [Fact]
    public async Task DeleteBooking_IncorrectId_ReturnsFalse()
    {
        // Arrange

        // Act
        var rep = new BookingRepository(_fixture.Context);
        var res = await rep.DeleteAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        res.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllBooking_ReturnsAllBooking()
    {
        // Arrange
        await using var context = _fixture.Context;
        var ev = TestData.GetTestEvent();
        await context.Events.AddAsync(ev);
        await context.SaveChangesAsync();
        var b1 = TestData.GetTestBooking(ev, _dateTimeProvider.UtcNow);
        var b2 = TestData.GetTestBooking(ev, _dateTimeProvider.UtcNow);
        var list = new List<Booking>() { b1, b2 }; 
        await context.Bookings.AddRangeAsync(list);
        await context.SaveChangesAsync();
        var ids = list.Select(o => o.Id).ToList();

        // Act
        var rep = new BookingRepository(_fixture.Context);
        var res = (await rep.GetAllAsync(CancellationToken.None)).Select(o => o.Id).ToList();

        // Assert
        res.Should().BeEquivalentTo(ids);
    }

    [Fact]
    public async Task GetBookingsCount_ReturnsCount()
    {
        // Arrange
        await using var context = _fixture.Context;
        var ev = TestData.GetTestEvent();
        await context.Events.AddAsync(ev);
        await context.SaveChangesAsync();
        var b1 = TestData.GetTestBooking(ev, _dateTimeProvider.UtcNow);
        var b2 = TestData.GetTestBooking(ev, _dateTimeProvider.UtcNow);
        var list = new List<Booking>() { b1, b2 };
        await context.Bookings.AddRangeAsync(list);
        await context.SaveChangesAsync();

        // Act
        var rep = new BookingRepository(_fixture.Context);
        var res = await rep.GetCountAsync(CancellationToken.None);

        // Assert
        res.Should().Be(list.Count);
    }

    [Fact]
    public async Task SaveChanges_SavesBookingToDataBase()
    {
        // Arrange
        await using var context = _fixture.Context;
        var ev = TestData.GetTestEvent();
        await context.Events.AddAsync(ev);
        await context.SaveChangesAsync();
        var booking = TestData.GetTestBooking(ev, _dateTimeProvider.UtcNow);
        await context.Bookings.AddAsync(booking);
        await context.SaveChangesAsync();        
        
        // Act
        var rep = new BookingRepository(_fixture.Context);
        var b = await rep.GetByIdAsync(booking.Id, CancellationToken.None);
        if (b == null)
            throw new InvalidOperationException("Что-то работает не так");
        b.Confirm(_dateTimeProvider);
        await rep.SaveChangesAsync(CancellationToken.None);


        // Assert
        await using var ctx = _fixture.Context;
        var changedBooking = await ctx.Bookings.FirstOrDefaultAsync(o => o.Id == b.Id);
        changedBooking.Should().NotBeNull();
        changedBooking.Status.Should().Be(BookingStatus.Confirmed);
    }

    [Fact]
    public async Task BeginTransaction_ReturnsTransaction()
    {
        // Arrange

        // Act
        var ctx = _fixture.Context;
        var rep = new BookingRepository(ctx);
        await using var tr = await rep.BeginTransactionAsync(CancellationToken.None);

        // Assert
        tr.Should().NotBeNull();
        tr.Should().BeAssignableTo<IDbContextTransaction>();
        ctx.Database.CurrentTransaction.Should().Be(tr);
    }

    [Fact]
    public async Task GetBookingWithBlocking_ReturnsBlockedBooking()
    {
        // Arrange
        var events = TestData.GetTestEvents();
        await using var ctx = _fixture.Context;
        await ctx.Events.AddRangeAsync(events);
        await ctx.SaveChangesAsync();
        var b1 = TestData.GetTestBooking(events[0], _dateTimeProvider.UtcNow);
        var b2 = TestData.GetTestBooking(events[1], _dateTimeProvider.UtcNow);
        await ctx.Bookings.AddRangeAsync(b1, b2);
        await ctx.SaveChangesAsync();


        var rep1 = new BookingRepository(_fixture.Context);
        using var tr1 = await rep1.BeginTransactionAsync(CancellationToken.None);
        var rep2 = new BookingRepository(_fixture.Context);
        using var tr2 = await rep2.BeginTransactionAsync(CancellationToken.None);
        
        // Act
        var res1 = await rep1.GetBookingWithBlockingAsync(b1.Id, CancellationToken.None);        
        Func<Task<Booking?>> act = async () => await rep2.GetBookingWithBlockingAsync(b1.Id, CancellationToken.None);
        
        // Assert
        res1.Should().NotBeNull();        
        res1.Id.Should().Be(b1.Id);        
        await act.Should().ThrowAsync<InvalidOperationException>();
        tr1.Rollback();
        tr2.Rollback();
        await using var tr4 = await rep2.BeginTransactionAsync();
        var res4 = await rep2.GetBookingWithBlockingAsync(b1.Id, CancellationToken.None);
        res4.Should().NotBeNull();
        res4.Id.Should().Be(b1.Id);
    }

    [Fact]
    public async Task SaveBooking_IncorrectEnum_ThrowsDbUpdateException()
    {
        // Arrange
        var ev = TestData.GetTestEvent();
        var ctx = _fixture.Context;
        await ctx.SaveChangesAsync();
        var booking = TestData.GetTestBooking(ev, _dateTimeProvider.UtcNow, status: (BookingStatus)10);

        // Act
        ctx.Bookings.Add(booking);
        Func<Task> act = async () => await ctx.SaveChangesAsync();

        //        Func<Task> act = async () => await ctx.Database.ExecuteSqlInterpolatedAsync(
        //$@"INSERT INTO bookings(id, status, seats_count, created_at, processed_at,event_id)
        // VALUES({booking.Id}, 'status', {booking.SeatsCount}, {booking.CreatedAt}, {booking.ProcessedAt}, {ev.Id})");

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task SaveBooking_NegativeSeatsCount_ThrowsDbUpdateException()
    {
        // Arrange
        var ev = TestData.GetTestEvent();
        var ctx = _fixture.Context;
        await ctx.SaveChangesAsync();
        var booking = TestData.GetTestBooking(ev, _dateTimeProvider.UtcNow);
        var seatsCount = -1;

        // Act        
        Func<Task> act = () => ctx.Database.ExecuteSqlInterpolatedAsync(
$@"INSERT INTO bookings(id, status, seats_count, created_at, processed_at,event_id)
 VALUES({booking.Id}, {booking.Status}, {seatsCount} , {booking.CreatedAt}, {booking.ProcessedAt}, {ev.Id})");

        // Assert
        await act.Should().ThrowAsync<PostgresException>();
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
