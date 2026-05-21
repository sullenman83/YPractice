using EventManagement.Common;
using EventManagement.Interfaces;
using EventManagement.Models.Events;
using EventManagement.Models.FilterModels;
using EventManagement.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using Org.BouncyCastle.Crypto.Signers;

namespace EventApi.IntegrationTest
{
    public class EventRepositoryTest : IClassFixture<DatabaseFixture>, IAsyncLifetime
    {
        private readonly DatabaseFixture _fixture ;
        private readonly IDateTimeProvider _dateTimeProvider = new DateTimeProvider();

        public EventRepositoryTest(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

//        [Fact]
        public async Task GetEventById_ReturnsEvent()
        {
            // Arrange
            await using var context = _fixture.Context;
            var ev = TestData.GetTestEvent();
            var id = ev.Id;
            await context.Events.AddAsync(ev);
            await context.SaveChangesAsync();

            // Act
            var rep = new EventRepository(_fixture.Context);
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
            var ev = TestData.GetTestEvent();
            var id = ev.Id;
            await context.Events.AddAsync(ev);
            await context.SaveChangesAsync();

            // Act
            var rep = new EventRepository(_fixture.Context);
            var res = await rep.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

            // Assert
            res.Should().BeNull();
        }

        [Fact]
        public async Task AddEvent_SavesEventToDataBase()
        {
            // Arrange
            await using var context = _fixture.Context;
            var ev = TestData.GetTestEvent();
            var id = ev.Id;
            
            // Act
            var rep = new EventRepository(context);
            var res = await rep.AddAsync(ev, CancellationToken.None);


            // Assert
            await using var ctx = _fixture.Context;
            var savedEvent = await ctx.Events.FirstOrDefaultAsync(e => e.Id == id);
            savedEvent.Should().NotBeNull();
            savedEvent.Should().BeEquivalentTo(ev);
        }

        [Fact]
        public async Task DeleteEvent_CascadeDeletesBooking()
        {
            // Arrange
            await using var context = _fixture.Context;
            var ev = TestData.GetTestEvent();
            var id = ev.Id;
            var booking = TestData.GetTestBooking(ev, _dateTimeProvider.UtcNow);
            await context.Events.AddAsync(ev);
            await context.SaveChangesAsync();
            await context.Bookings.AddAsync(booking);
            await context.SaveChangesAsync();

            // Act
            var rep = new EventRepository(_fixture.Context);
            await rep.DeleteAsync(id, CancellationToken.None);

            // Assert
            await using var ctx = _fixture.Context;
            var b = await ctx.Bookings.Where(o => o.EventId == id).ToListAsync();
            b.Should().BeEmpty();
            var e = await ctx.Events.FirstOrDefaultAsync(e => e.Id == id);
            e.Should().BeNull();
        }

        [Fact]
        public async Task DeleteEvent_IncorrectId_ReturnsFalse()
        {
            // Arrange            
            
            // Act
            var rep = new EventRepository(_fixture.Context);
            var res = await rep.DeleteAsync(Guid.NewGuid(), CancellationToken.None);

            // Assert
            res.Should().BeFalse();
        }

        [Fact]
        public async Task GetAllEvents_ReturnsAllEvents()
        {
            // Arrange
            await using var context = _fixture.Context;
            var events = TestData.GetTestEvents();
            await context.Events.AddRangeAsync(events);
            await context.SaveChangesAsync();
            var filter = new EventFilterRequestDTO();

            // Act
            var rep = new EventRepository(_fixture.Context);
            var res = await rep.GetAllAsync(CancellationToken.None);

            // Assert
            res.Should().BeEquivalentTo(events);
        }

        [Fact]
        public async Task GetEventsCount_ReturnsCount()
        {
            // Arrange
            await using var context = _fixture.Context;
            var events = TestData.GetTestEvents();
            await context.Events.AddRangeAsync(events);
            await context.SaveChangesAsync();

            // Act
            var rep = new EventRepository(_fixture.Context);
            var res = await rep.GetCountAsync(CancellationToken.None);

            // Assert
            res.Should().Be(events.Count);
        }


        [Fact]
        public async Task SaveChanges()
        {
            // Arrange
            await using var context = _fixture.Context;
            var ev = TestData.GetTestEvent();
            await context.Events.AddAsync(ev);
            await context.SaveChangesAsync();
            var title = ev.Title + "Changed";

            // Act
            var rep = new EventRepository(_fixture.Context);
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
        public async Task BeginTransaction_ReturnsTransaction()
        {
            // Arrange            

            // Act
            var ctx = _fixture.Context;
            var rep = new EventRepository(ctx);
            await using var tr = await rep.BeginTransactionAsync(CancellationToken.None);            

            // Assert
            tr.Should().NotBeNull();
            tr.Should().BeAssignableTo<IDbContextTransaction>();
            ctx.Database.CurrentTransaction.Should().Be(tr);
        }

        [Fact]
        public async Task GetEventWithBlocking_ReturnsBlockedEvent()
        {
            // Arrange
            var events = TestData.GetTestEvents();
            var id1 = events[0].Id;
            var id2 = events[1].Id;
            await using var ctx = _fixture.Context;
            await ctx.Events.AddRangeAsync(events);
            await ctx.SaveChangesAsync();

            var rep1 = new EventRepository(_fixture.Context);
            await using var tr1 = await rep1.BeginTransactionAsync(CancellationToken.None);
            var rep2 = new EventRepository(_fixture.Context);
            await using var tr2 = await rep2.BeginTransactionAsync(CancellationToken.None);
            var rep3 = new EventRepository(_fixture.Context);
            await using var tr3 = await rep3.BeginTransactionAsync(CancellationToken.None);

            // Act
            var res1 = await rep1.GetEventWithBlockingAsync(id1, CancellationToken.None);
            var res2 = await rep2.GetByIdAsync(id1);
            Func<Task<Event?>> act = async () => await rep2.GetEventWithBlockingAsync(id1, CancellationToken.None);
            var res3 = await rep1.GetEventWithBlockingAsync(id2, CancellationToken.None);

            // Assert
            res1.Should().BeEquivalentTo(events[0]);
            res2.Should().BeEquivalentTo(events[0]);
            res3.Should().BeEquivalentTo(events[1]);
            await act.Should().ThrowAsync<InvalidOperationException>();
            await tr1.RollbackAsync();
            await tr2.RollbackAsync();
            await using var tr4 = await rep2.BeginTransactionAsync();
            var res4 = await rep2.GetEventWithBlockingAsync(id1, CancellationToken.None);
            res4.Should().BeEquivalentTo(events[0]);
        }

        [Fact]
        public async Task SaveEvent_EndAtLessStartAt_ThrowsDbUpdateException()
        {
            // Arrange
            var ev = TestData.GetTestEvent();
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
            var ev = TestData.GetTestEvent();            
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
            var ev = TestData.GetTestEvent();
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
            var ev = TestData.GetTestEvent();
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
}
