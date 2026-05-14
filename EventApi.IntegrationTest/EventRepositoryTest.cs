using EventManagement.Common;
using EventManagement.Models.Events;
using EventManagement.Models.FilterModels;
using EventManagement.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Org.BouncyCastle.Crypto.Signers;

namespace EventApi.IntegrationTest
{
    public class EventRepositoryTest : BaseTest
    {
        [Fact]
        public async Task GetEventById_ReturnsEvent()
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
            var res = await rep.GetByIdAsync(id, CancellationToken.None);
            
            // Assert
            res.Should().NotBeNull();
            res.Should().BeEquivalentTo(ev);
        }

        [Fact]
        public async Task GetEventById_IncorrectId_ReturnsNull()
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
            var res = await rep.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

            // Assert
            res.Should().BeNull();
        }

        [Fact]
        public async Task AddEvent_SavesEventToDataBase()
        {
            // Arrange
            await ResetDatabaseAsync();
            await using var context = await CreateContextAsync();
            var ev = TestData.GetTestEvent();
            var id = ev.Id;
            
            // Act
            var rep = new EventRepository(context);
            var res = await rep.AddAsync(ev, CancellationToken.None);


            // Assert
            await using var ctx = await CreateContextAsync();
            var savedEvent = ctx.Events.SingleOrDefault(e => e.Id == id);
            savedEvent.Should().NotBeNull();
            savedEvent.Should().BeEquivalentTo(ev);
        }

        [Fact]
        public async Task DeleteEvent_CascadeDeletesBooking()
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
            await rep.DeleteAsync(id, CancellationToken.None);

            // Assert
            await using var ctx = await CreateContextAsync();
            var b = await ctx.Bookings.Where(o => o.EventId == id).ToListAsync();
            b.Should().BeEmpty();
            var e = ctx.Events.SingleOrDefaultAsync(e => e.Id == id);
            e.Should().BeNull();
        }

        [Fact]
        public async Task DeleteEvent_IncorrectId_ReturnsFalse()
        {
            // Arrange
            await ResetDatabaseAsync();
            
            // Act
            var rep = new EventRepository(await CreateContextAsync());
            var res = await rep.DeleteAsync(Guid.NewGuid(), CancellationToken.None);

            // Assert
            res.Should().BeFalse();
        }

        [Fact]
        public async Task GetAllEvents_ReturnsAllEvents()
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
            var res = await rep.GetAllAsync(CancellationToken.None);

            // Assert
            res.Should().BeEquivalentTo(events);
        }

        [Fact]
        public async Task GetEventsCount_ReturnsCount()
        {
            // Arrange
            await ResetDatabaseAsync();
            await using var context = await CreateContextAsync();
            var events = TestData.GetTestEvents();
            await context.Events.AddRangeAsync(events);
            await context.SaveChangesAsync();

            // Act
            var rep = new EventRepository(await CreateContextAsync());
            var res = await rep.GetCountAsync(CancellationToken.None);

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
            var e = await rep.GetByIdAsync(ev.Id, CancellationToken.None);
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
        public async Task BeginTransaction_ReturnsTransaction()
        {
            // Arrange
            await ResetDatabaseAsync();

            // Act
            var ctx = await CreateContextAsync();
            var rep = new EventRepository(ctx);
            await using var tr = await rep.BeginTransactionAsync(CancellationToken.None);            

            // Assert
            tr.Should().NotBeNull();
            tr.Should().BeOfType<IDbContextTransaction>();
            ctx.Database.CurrentTransaction.Should().Be(tr);
        }

        [Fact]
        public async Task GetEventWithBlocking_ReturnsBlockedEvent()
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
}
