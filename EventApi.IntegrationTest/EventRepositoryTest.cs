using EventManagement.Common;
using EventManagement.Models.Events;
using EventManagement.Models.FilterModels;
using EventManagement.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Signers;

namespace EventApi.IntegrationTest
{
    public class EventRepositoryTest : BaseTest
    {
        [Fact]
        public async Task GetEventById_ReturnEvent()
        {
            // Arrange
            await ResetDatabaseAsync();
            await using var context = await CreateContextAsync();
            var ev = TestData.GetTestEvent();
            var id = ev.Id;
            await context.Events.AddAsync(ev);
            await context.SaveChangesAsync();

            // Act
            var rep = new EventRepository(await CreateContextAsync());
            var res = await rep.GetEventByIdAsync(id, CancellationToken.None);
            
            // Assert
            res.Should().NotBeNull();
            res.Should().BeEquivalentTo(ev);
        }

        [Fact]
        public async Task GetEventById_IncorrectId_ReturnNull()
        {
            // Arrange
            await ResetDatabaseAsync();
            await using var context = await CreateContextAsync();
            var ev = TestData.GetTestEvent();
            var id = ev.Id;
            await context.Events.AddAsync(ev);
            await context.SaveChangesAsync();

            // Act
            var rep = new EventRepository(await CreateContextAsync());
            var res = await rep.GetEventByIdAsync(Guid.NewGuid(), CancellationToken.None);

            // Assert
            res.Should().BeNull();
        }

        [Fact]
        public async Task AddEvent_SaveEventToDataBase()
        {
            // Arrange
            await ResetDatabaseAsync();
            await using var context = await CreateContextAsync();
            var ev = TestData.GetTestEvent();
            var id = ev.Id;
            
            // Act
            var rep = new EventRepository(context);
            var res = await rep.AddEventAsync(ev, CancellationToken.None);


            // Assert
            await using var ctx = await CreateContextAsync();
            var savedEvent = ctx.Events.SingleOrDefault(e => e.Id == id);
            savedEvent.Should().NotBeNull();
            savedEvent.Should().BeEquivalentTo(ev);
        }

        [Fact]
        public async Task DeleteEvent_CascadeDeleteBooking()
        {
            // Arrange
            await ResetDatabaseAsync();
            await using var context = await CreateContextAsync();
            var ev = TestData.GetTestEvent();
            var id = ev.Id;
            var booking = TestData.GetTestBooking(ev);
            await context.Events.AddAsync(ev);
            await context.SaveChangesAsync();
            await context.Bookings.AddAsync(booking);
            await context.SaveChangesAsync();

            // Act
            var rep = new EventRepository(await CreateContextAsync());
            await rep.DeleteEventAsync(id, CancellationToken.None);

            // Assert
            await using var ctx = await CreateContextAsync();
            var b = await ctx.Bookings.Where(o => o.EventId == id).ToListAsync();
            b.Should().BeEmpty();
            var e = ctx.Events.SingleOrDefaultAsync(e => e.Id == id);
            e.Should().BeNull();
        }

        [Fact]
        public async Task GetEvents_ReturnAllEvents()
        {
            // Arrange
            await ResetDatabaseAsync();
            await using var context = await CreateContextAsync();
            var events = TestData.GetTestEvents();
            await context.Events.AddRangeAsync(events);
            await context.SaveChangesAsync();
            var filter = new EventFilterRequestDTO();

            // Act
            var rep = new EventRepository(await CreateContextAsync());
            var res = await rep.GetEventsAsync(filter, CancellationToken.None);

            // Assert
            res.Should().BeEquivalentTo(events);
        }

        [Fact]
        public async Task GetEventsCount_ReturnCount()
        {
            // Arrange
            await ResetDatabaseAsync();
            await using var context = await CreateContextAsync();
            var events = TestData.GetTestEvents();
            await context.Events.AddRangeAsync(events);
            await context.SaveChangesAsync();

            // Act
            var rep = new EventRepository(await CreateContextAsync());
            var res = await rep.GetEventsCountAsync(CancellationToken.None);

            // Assert
            res.Should().Be(events.Count);
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
            var title = ev.Title + "Changed";

            // Act
            var rep = new EventRepository(await CreateContextAsync());
            var e = await rep.GetEventByIdAsync(ev.Id, CancellationToken.None);
            if (e == null)
                throw new InvalidOperationException("Что-то работает не так");            
            e.Title = title;
            await rep.SaveChangesAsync(CancellationToken.None);


            // Assert
            await using var ctx = await CreateContextAsync();
            var changedEvent = await ctx.Events.FirstOrDefaultAsync(o => o.Id == ev.Id);
            changedEvent.Should().NotBeNull();
            changedEvent.Title.Should().Be(title);            
        }

        [Fact]
        public async Task BeginTransaction_ReturnTransaction()
        {
            // Arrange
            await ResetDatabaseAsync();
            //await using var context = await CreateContextAsync();
            //var ev = TestData.GetTestEvent();
            //await context.Events.AddAsync(ev);
            //await context.SaveChangesAsync();

            // Act
            var rep = new EventRepository(await CreateContextAsync());
            using var tr = await rep.BeginTransactionAsync(CancellationToken.None);            

            // Assert
            tr.Should().NotBeNull();
        }
    }
}
