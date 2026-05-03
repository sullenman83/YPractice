using EventManagement.Common;
using EventManagement.Data;
using EventManagement.Interfaces;
using EventManagement.Models.Events;
using EventManagement.Models.FilterModels;
using EventManagement.Services.EventServices;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventServiceTest;

public class EventFilterTest : IDisposable
{
    private readonly IEventValidator _eventValidator;
    private readonly ServiceProvider _serviceProvider;
    private readonly IEventService _eventService;
    private readonly IServiceScope _scope;

    private static readonly List<Event> _events = TestData.GetTestEvents();
 
    public EventFilterTest()
    {
        var services = new ServiceCollection();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IEventValidator, EventValidator>();
        var dbName = Guid.NewGuid().ToString();
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseInMemoryDatabase(dbName);
        });

        _serviceProvider = services.BuildServiceProvider();
        _scope = _serviceProvider.CreateScope();
        _eventService = _scope.ServiceProvider.GetRequiredService<IEventService>();
        _eventValidator = _scope.ServiceProvider.GetRequiredService<IEventValidator>();
    }
    public void Dispose()
    {
        _scope.Dispose();
        _serviceProvider.Dispose();
    }


    [Fact]
    public async Task Test_EmptyFilter_ReturnAllEvents()
    {
	    // Act
        var result = await _eventService.GetEventsAsync(new EventFilterRequestDTO(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Events.Should().BeEquivalentTo(_events);        
    }

    /// <summary>
    /// Тест для фильтрации данных (Тестируется три варианта вхождение строки по краям и в середине)
    /// </summary>
    /// <param name="filter">Фильтр для поиска событий</param>
    /// <param name="count">Количество найденных событий</param>
    /// <param name="id">id найденного события</param>
    [Theory]
    [MemberData(nameof(GetEventFilterByTitle))]    
    public async Task TestFilter_ByTitle_ReturnRelevantEvents(EventFilterRequestDTO filter, int count, Guid id)
    {
	    // Act
        var result = await _eventService.GetEventsAsync(filter, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Events.Count.Should().Be(count);
        result.Events.First().Id.Should().Be(id);
    }

    [Theory]
    [MemberData(nameof(GetEventFilterByDates))]
    public async Task TestFilter_ByDate_ReturnRelevantEvents(EventFilterRequestDTO filter, int count, Guid[] ids)
    {
	    // Act
        var result = await _eventService.GetEventsAsync(filter, CancellationToken.None);
        var resultIds = result.Events.Select(o => o.Id).ToArray() ?? new Guid[] { };

        // Assert
        result.Should().NotBeNull();
        result.Events.Count.Should().Be(count);
        if (resultIds.Length > 0)
            resultIds.Should().Contain(ids);
    }

    [Theory]
    [MemberData(nameof(GetPaginationFilterData))]
    public async Task TestPagination(EventFilterRequestDTO filter, int currentPage, int eventCount)
    {
	    // Act
        var result = await _eventService.GetEventsAsync(filter, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Page.Should().Be(currentPage);
        result.EventsCountOnCurrentPage.Should().Be(eventCount);
    }

    public static IEnumerable<object[]> GetEventFilterByTitle()
    {
        var titles = new string[]{ "Другое", "событие для", "теста 2"};
        var filters = new object[titles.Length][];        
        var id = _events.Last().Id;


        for(int i = 0; i < titles.Length; ++i)
        {
            var f = new EventFilterRequestDTO() { Title = titles[i] };
            filters[i] = new object[3] { f, 1, id };
        }

        return filters;
    }

    public static IEnumerable<object[]> GetEventFilterByDates()
    {
        var id1 = _events.First().Id;
        var id2 = _events.Last().Id;
        var dates = new[]
        {
            new Tuple<DateTimeOffset?, DateTimeOffset?, int, Guid[]>(new DateTimeOffset(2026,03,23, 1, 30, 30, TimeSpan.Zero), new DateTimeOffset(2023, 03, 21, 20, 30, 30, TimeSpan.Zero), 0, new Guid []{ }),
            new Tuple<DateTimeOffset?, DateTimeOffset?, int, Guid[]>(new DateTimeOffset(2026,03,22, 1, 30, 30, TimeSpan.Zero), null, 2, new Guid []{id1, id2 }),            
            new Tuple<DateTimeOffset?, DateTimeOffset?, int, Guid[]>(new DateTimeOffset(2026,03,24, 1, 30, 30, TimeSpan.Zero), null, 1, new Guid[]{id2}),
            new Tuple<DateTimeOffset?, DateTimeOffset?, int, Guid[]>(new DateTimeOffset(2026,03,25, 1, 30, 30, TimeSpan.Zero), null, 0 ,new Guid []{}),
                                   
            new Tuple<DateTimeOffset?, DateTimeOffset?, int, Guid[]>(null, new DateTimeOffset(2026,03,27, 20, 30, 30, TimeSpan.Zero), 2, new Guid []{ id1, id2}),
            new Tuple<DateTimeOffset?, DateTimeOffset?, int, Guid[]>(null, new DateTimeOffset(2026,03,22, 20, 30, 30, TimeSpan.Zero), 1, new Guid []{id1}),
            new Tuple<DateTimeOffset?, DateTimeOffset?, int, Guid[]>(null, new DateTimeOffset(2026,03,21, 20, 30, 30, TimeSpan.Zero), 0, new Guid []{}),

            
            new Tuple<DateTimeOffset?, DateTimeOffset?, int, Guid[]>(new DateTimeOffset(2026,03,22, 1, 30, 30, TimeSpan.Zero), new DateTimeOffset(2026,03,27, 20, 30, 30, TimeSpan.Zero), 2, new Guid[]{ id1, id2}),
            new Tuple<DateTimeOffset?, DateTimeOffset?, int, Guid[]>(new DateTimeOffset(2026,03,23, 1, 30, 30, TimeSpan.Zero), new DateTimeOffset(2026,03,27, 20, 30, 30, TimeSpan.Zero), 1, new Guid []{id2}),            
            new Tuple<DateTimeOffset?, DateTimeOffset?, int, Guid[]>(new DateTimeOffset(2026,03,22, 1, 30, 30, TimeSpan.Zero), new DateTimeOffset(2026,03,26, 20, 30, 30, TimeSpan.Zero), 1, new Guid []{ id1}),
        };
        var filters = new object[dates.Length][];

        for (int i =0; i< dates.Length; ++i)
        {
            var f = new EventFilterRequestDTO() 
            { 
                From = dates[i].Item1,
                To = dates[i].Item2,
            };

            filters[i] = new object[3] { f, dates[i].Item3, dates[i].Item4 };
        }

        return filters;
    }

    public static IEnumerable<object[]> GetPaginationFilterData()
    {
        var paginationData = new [] 
        { 
            new {Page = 1, PageSize = 10, CurrentPage = 1, EventCount = 2 },
            new {Page = 1, PageSize = 1, CurrentPage = 1, EventCount = 1},
            new {Page = 2, PageSize = 1, CurrentPage = 2, EventCount = 1},
            new {Page = 3, PageSize = 1, CurrentPage = 3, EventCount = 0},
        };
        var filters = new object[paginationData.Length][];

        for (int i = 0; i < paginationData.Length; ++i)
        {
            var f = new EventFilterRequestDTO() 
            {
                Page = paginationData[i].Page,
                PageSize = paginationData[i].PageSize,                
            };
            filters[i] = new object[] { f, paginationData[i].CurrentPage, paginationData[i].EventCount };
        }

        return filters;
    }
}
