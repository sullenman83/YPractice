using EventManagement.Common;
using EventManagement.Models.BookingModels;
using EventManagement.Models.Events;
using EventManagement.Models.FilterModels;
using EventManagement.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace EventApi.IntegrationTest;

public class BookingRepositoryTest: BaseTest
{
    
    [Fact]
    public async Task GetBookingById_ReturnsBooking()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = await CreateContextAsync();
        var ev = TestData.GetTestEvent();
        var booking = TestData.GetTestBooking(ev);
        await context.Events.AddAsync(ev);
        await context.Bookings.AddAsync(booking);
        await context.SaveChangesAsync();
        var id = booking.Id;

        // Act
        var rep = new BookingRepository(await CreateContextAsync());
        var res = await rep.GetByIdAsync(id, CancellationToken.None);

        // Assert
        res.Should().NotBeNull();
        res.Should().BeEquivalentTo(booking);
    }

    [Fact]
    public async Task GetBookingById_IncorrectId_ReturnsNull()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = await CreateContextAsync();
        var ev = TestData.GetTestEvent();
        var booking = TestData.GetTestBooking(ev);
        await context.Events.AddAsync(ev);
        await context.Bookings.AddAsync(booking);
        await context.SaveChangesAsync();
        var id = booking.Id;

        // Act
        var rep = new BookingRepository(await CreateContextAsync());
        var res = await rep.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        res.Should().BeNull();
    }

    [Fact]
    public async Task AddBooking_SavesBookingToDataBase()
    {
        // Arrange
        // Arrange
        await ResetDatabaseAsync();
        await using var context = await CreateContextAsync();
        var ev = TestData.GetTestEvent();
        var booking = TestData.GetTestBooking(ev);
        await context.Events.AddAsync(ev);
        await context.Bookings.AddAsync(booking);
        await context.SaveChangesAsync();
        var id = booking.Id;

        // Act
        var rep = new BookingRepository(context);
        var res = await rep.AddAsync(booking, CancellationToken.None);


        // Assert
        await using var ctx = await CreateContextAsync();
        var savedBooking = ctx.Bookings.SingleOrDefault(e => e.Id == id);
        savedBooking.Should().NotBeNull();
        savedBooking.Should().BeEquivalentTo(booking);
    }

    [Fact]
    public async Task DeleteEvent_DeletesBooking()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = await CreateContextAsync();
        var ev = TestData.GetTestEvent();        
        var booking = TestData.GetTestBooking(ev);
        await context.Events.AddAsync(ev);
        await context.SaveChangesAsync();
        await context.Bookings.AddAsync(booking);
        await context.SaveChangesAsync();
        var id = booking.Id;

        // Act
        var rep = new BookingRepository(await CreateContextAsync());
        await rep.DeleteAsync(id, CancellationToken.None);

        // Assert
        await using var ctx = await CreateContextAsync();
        var b = await ctx.Bookings.SingleOrDefaultAsync(o => o.Id == id);
        b.Should().BeNull();
    }

    [Fact]
    public async Task DeleteBooking_IncorrectId_ReturnsFalse()
    {
        // Arrange
        await ResetDatabaseAsync();

        // Act
        var rep = new BookingRepository(await CreateContextAsync());
        var res = await rep.DeleteAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        res.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllBooking_ReturnsAllBooking()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = await CreateContextAsync();
        var ev = TestData.GetTestEvent();
        await context.Events.AddAsync(ev);
        await context.SaveChangesAsync();
        var b1 = TestData.GetTestBooking(ev);
        var b2 = TestData.GetTestBooking(ev);
        var list = new List<Booking>() { b1, b2 };        
        await context.Bookings.AddRangeAsync(list);
        await context.SaveChangesAsync();

        // Act
        var rep = new BookingRepository(await CreateContextAsync());
        var res = await rep.GetAllAsync(CancellationToken.None);

        // Assert
        res.Should().BeEquivalentTo(list);
    }

    [Fact]
    public async Task GetBookingsCount_ReturnsCount()
    {
        // Arrange
        // Arrange
        await ResetDatabaseAsync();
        await using var context = await CreateContextAsync();
        var ev = TestData.GetTestEvent();
        await context.Events.AddAsync(ev);
        await context.SaveChangesAsync();
        var b1 = TestData.GetTestBooking(ev);
        var b2 = TestData.GetTestBooking(ev);
        var list = new List<Booking>() { b1, b2 };
        await context.Bookings.AddRangeAsync(list);
        await context.SaveChangesAsync();

        // Act
        var rep = new BookingRepository(await CreateContextAsync());
        var res = await rep.GetCountAsync(CancellationToken.None);

        // Assert
        res.Should().Be(list.Count);
    }

    [Fact]
    public async Task SaveChanges()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = await CreateContextAsync();
        var ev = TestData.GetTestEvent();
        await context.Events.AddAsync(ev);
        await context.SaveChangesAsync();
        var booking = TestData.GetTestBooking(ev);
        await context.Bookings.AddAsync(booking);
        await context.SaveChangesAsync();
        var title = ev.Title + "Changed";
        
        // Act
        var rep = new BookingRepository(await CreateContextAsync());
        var b = await rep.GetByIdAsync(booking.Id, CancellationToken.None);
        if (b == null)
            throw new InvalidOperationException("Что-то работает не так");
        b.Confirm();
        await rep.SaveChangesAsync(CancellationToken.None);


        // Assert
        await using var ctx = await CreateContextAsync();
        var changedBooking = await ctx.Bookings.FirstOrDefaultAsync(o => o.Id == b.Id);
        changedBooking.Should().NotBeNull();
        changedBooking.Status.Should().Be(BookingStatus.Confirmed);
    }

    [Fact]
    public async Task BeginTransaction_ReturnsTransaction()
    {
        // Arrange
        await ResetDatabaseAsync();

        // Act
        var ctx = await CreateContextAsync();
        var rep = new BookingRepository(ctx);
        await using var tr = await rep.BeginTransactionAsync(CancellationToken.None);

        // Assert
        tr.Should().NotBeNull();
        tr.Should().BeOfType<IDbContextTransaction>();
        ctx.Database.CurrentTransaction.Should().Be(tr);
    }

    [Fact]
    public async Task GetBookingWithBlocking_ReturnsBlockedBooking()
    {
        // Arrange
        await ResetDatabaseAsync();
        var events = TestData.GetTestEvents();
        var id1 = events[0].Id;
        var id2 = events[1].Id;
        await using var ctx = await CreateContextAsync();
        await ctx.Events.AddRangeAsync(events);
        await ctx.SaveChangesAsync();

        var rep1 = new EventRepository(await CreateContextAsync());
        using var tr1 = await rep1.BeginTransactionAsync(CancellationToken.None);
        var rep2 = new EventRepository(await CreateContextAsync());
        using var tr2 = await rep1.BeginTransactionAsync(CancellationToken.None);

        // Act
        var res1 = await rep1.GetEventWithBlockingAsync(id1, CancellationToken.None);
        var res2 = rep2.GetByIdAsync(id1);
        Func<Task<Event?>> act = async () => await rep2.GetEventWithBlockingAsync(id2, CancellationToken.None);

        // Assert
        res1.Should().BeEquivalentTo(events[0]);
        res2.Should().BeEquivalentTo(events[0]);
        await act.Should().ThrowAsync<TimeoutException>();
        tr1.Rollback();
        var res3 = await rep2.GetEventWithBlockingAsync(id1, CancellationToken.None);
        res3.Should().BeEquivalentTo(events[0]);
    }
    
}
