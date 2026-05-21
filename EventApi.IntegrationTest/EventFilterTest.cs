using EventApi.IntegrationTest;
using EventManagement.Common;
using EventManagement.Models.Events.Extensions;
using EventManagement.Models.FilterModels;
using EventManagement.Services;
using FluentAssertions;

namespace EventApi.IntegrationTest;

public class EventFilterTest(DatabaseFixture fixture) : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly DatabaseFixture _fixture = fixture;

    [Fact]
    public async Task Test_EmptyFilter_ReturnAllEvents()
    {
        // Arrange
        var events = TestData.GetTestEvents();
        await using var context = _fixture.Context;
        await context.Events.AddRangeAsync(events);
        await context.SaveChangesAsync();
        var filter = new EventFilterRequestDTO();
        var response = events.Select(o => o.ToResponse());

        // Act
        var rep = new EventRepository(_fixture.Context);
        var res = await rep.GetEventsByFilterAsync(filter, CancellationToken.None);        

        // Assert
        res.Events.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task TestFilter_ByTitle_ReturnRelevantEvents()
    {
        // Arrange
        var events = TestData.GetTestEvents();
        await using var context = _fixture.Context;
        await context.Events.AddRangeAsync(events);
        await context.SaveChangesAsync();
        var title = "событие для";
        var filter = new EventFilterRequestDTO() { Title = title };

        // Act
        var rep = new EventRepository(_fixture.Context);
        var result = await rep.GetEventsByFilterAsync(filter, CancellationToken.None);

        // Assert
        result.Events.Should().HaveCount(1);
        result.Events.First().Title.Should().Contain(title);
    }

    [Fact]
    public async Task TestFilter_ByDate_ReturnRelevantEvents()
    {
        // Arrange
        var events = TestData.GetTestEvents();
        await using var context = _fixture.Context;
        await context.Events.AddRangeAsync(events);
        var ev = TestData.GetTestEvent();
        ev.StartAt = events.First().StartAt.AddDays(-1);
        await context.Events.AddAsync(ev);
        ev = TestData.GetTestEvent();
        ev.EndAt = events.Last().EndAt.AddDays(1);
        await context.Events.AddAsync(ev);
        await context.SaveChangesAsync();
        var filter = new EventFilterRequestDTO() { From = events.First().StartAt, To = events.Last().EndAt };
        var response = events.Select(o => o.ToResponse()).ToList();
        
        // Act
        var rep = new EventRepository(_fixture.Context);
        var result = await rep.GetEventsByFilterAsync(filter, CancellationToken.None);

        // Assert
        result.Events.Should().HaveCount(2);
        result.Events.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task PaginationTest_ReturnsRightEvents()
    {
        // Arrange        
        await using var context = _fixture.Context;
        for (var i = 0; i < 3; i++)
        {
            await context.Events.AddRangeAsync(TestData.GetTestEvents());
        }        
        await context.SaveChangesAsync();
        var eventsCount = context.Events.Count();
        var pageSize = 2;
        var currentPage = 2;
        var filter = new EventFilterRequestDTO() { PageSize = pageSize, Page = currentPage};

        // Act
        var rep = new EventRepository(_fixture.Context);
        var result = await rep.GetEventsByFilterAsync(filter, CancellationToken.None);

        // Assert
        result.Events.Should().HaveCount(2);
        result.EventsCountOnCurrentPage.Should().Be(2);
        result.EventsCount.Should().Be(eventsCount);
    }

    [Fact]
    public async Task PaginationTest_FilterEvent_ReturnsRigthEventCount()
    {
        await using var context = _fixture.Context;
        for (var i = 0; i < 3; i++)
        {
            await context.Events.AddRangeAsync(TestData.GetTestEvents());
        }
        await context.SaveChangesAsync();
        var eventsCount = context.Events.Count();
        var pageSize = 1;
        var currentPage = 1;
        var title = "событие для";
        var filter = new EventFilterRequestDTO() { Title = title, PageSize = pageSize, Page = currentPage };

        // Act
        var rep = new EventRepository(_fixture.Context);
        var result = await rep.GetEventsByFilterAsync(filter, CancellationToken.None);

        // Assert
        result.Events.Should().HaveCount(1);
        result.EventsCount.Should().Be(3);
        result.EventsCount.Should().BeLessThan(eventsCount);
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