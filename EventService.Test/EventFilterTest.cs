using EventManagement.Common;
using EventManagement.Data;
using EventManagement.Interfaces;
using EventManagement.Models.Events;
using EventManagement.Models.FilterModels;
using EventManagement.Services.EventServices;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.InteropServices;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        // Arrange
        var events = await CreateEvents();
        
        // Act        
        var result = await _eventService.GetEventsAsync(new EventFilterRequestDTO(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Events.Should().BeEquivalentTo(events);        
    }

    /// <summary>
    /// Тест для фильтрации данных (Тестируется три варианта вхождение строки по краям и в середине)
    /// </summary>
    /// <param name="filter">Фильтр для поиска событий</param>
    /// <param name="count">Количество найденных событий</param>
    /// <param name="id">id найденного события</param>
    [Fact]    
    public async Task TestFilter_ByTitle_ReturnRelevantEvents()
    {
        // Arrange
        var events = await CreateEvents();
        var ev1 = events.First();
        var ev2 = events.Last();
        var tests = new Tuple<string, EventResponseDto[]>[]
        {
            new Tuple<string, EventResponseDto[]>("Другое", [ev2]),
            new Tuple<string, EventResponseDto[]>("событие", [ev1, ev2]),
            new Tuple<string, EventResponseDto[]>("событие для", [ev2]),
            new Tuple<string, EventResponseDto[]>("теста 2", [ev2]),
        };

        // Act
        var result = new List<Tuple<EventResponseDto[], EventResponseDto[]>>();
        foreach (var t in tests)
        {
            var filter = new EventFilterRequestDTO() { Title = t.Item1 };
            var r = await _eventService.GetEventsAsync(filter, CancellationToken.None);
            result.Add(new Tuple<EventResponseDto[], EventResponseDto[]>( t.Item2, r.Events.ToArray()));
        }

        // Assert
        foreach(var r in result)
        {
            r.Item2.Should().NotBeNull();
            r.Item1.Should().BeEquivalentTo(r.Item2);
        }
    }

    [Fact]
    public async Task TestFilter_ByDate_ReturnRelevantEvents()
    {
        // Arrange
        var data = await CreateEvents();
        var ev1 = data.First();
        var ev2 = data.Last();

        var filter1 = createFilter(ev1.StartAt, null);
        var filter2 = createFilter(null, ev2.EndAt);
        var filter3 = createFilter(ev1.StartAt, ev2.EndAt);
        
        var filter4 = createFilter(null, ev1.StartAt.AddDays(-1));
        var filter5 = createFilter(ev2.EndAt.AddDays(1), null);

        var filter6 = createFilter(ev2.StartAt.AddDays(-1), null);
        var filter7 = createFilter(null, ev1.EndAt.AddDays(1));

        // Act
        var result1 = await _eventService.GetEventsAsync(filter1, CancellationToken.None);
        var expectedResult1 = new[] { ev1, ev2 };

        var result2 = await _eventService.GetEventsAsync(filter2, CancellationToken.None);
        var expectedResult2 = new[] { ev1, ev2 };

        var result3 = await _eventService.GetEventsAsync(filter3, CancellationToken.None);
        var expectedResult3 = new[] { ev1, ev2 };

        var result4 = await _eventService.GetEventsAsync(filter4, CancellationToken.None);       
        var result5 = await _eventService.GetEventsAsync(filter5, CancellationToken.None);

        var result6 = await _eventService.GetEventsAsync(filter6, CancellationToken.None);
        var expectedResult6 = new[] { ev2 };
        var result7 = await _eventService.GetEventsAsync(filter7, CancellationToken.None);
        var expectedResult7 = new[] { ev1};



        // Assert        
        result1.Events.Should().BeEquivalentTo(expectedResult1);
        result2.Events.Should().BeEquivalentTo(expectedResult2);
        result3.Events.Should().BeEquivalentTo(expectedResult3);
        result4.Events.Should().HaveCount(0);
        result5.Events.Should().HaveCount(0);
        result6.Events.Should().BeEquivalentTo(expectedResult6);
        result7.Events.Should().BeEquivalentTo(expectedResult7);

    }

    [Fact]    
    public async Task TestPagination()
    {
        var data = await CreateEvents();
        var paginationData = new[]
        {
            new {Page = 1, PageSize = 10, CurrentPage = 1, EventCount = 2 },
            new {Page = 1, PageSize = 1, CurrentPage = 1, EventCount = 1},
            new {Page = 2, PageSize = 1, CurrentPage = 2, EventCount = 1},
            new {Page = 3, PageSize = 1, CurrentPage = 3, EventCount = 0},
        };
        var filters = new List<Tuple<EventFilterRequestDTO, int, int>>();

        for (int i = 0; i < paginationData.Length; ++i)
        {
            var f = new EventFilterRequestDTO()
            {
                Page = paginationData[i].Page,
                PageSize = paginationData[i].PageSize,
            };
            filters.Add(new Tuple<EventFilterRequestDTO, int, int>(f, paginationData[i].CurrentPage, paginationData[i].EventCount));
        }

        // Act
        var results = new List<Tuple<int, int, int, int>>();
        foreach(var f in filters)
        {
            var r = await _eventService.GetEventsAsync(f.Item1, CancellationToken.None);
            results.Add(new Tuple<int, int, int, int>(f.Item2, f.Item3, r.Page, r.EventsCountOnCurrentPage));
        }
        
        // Assert

        foreach(var r in results)
        {
            r.Item1.Should().Be(r.Item3);
            r.Item2.Should().Be(r.Item4);
        }
    }   

    private async Task<List<EventResponseDto>> CreateEvents()
    {
        var data = TestData.GetTestEvents();
        var events = new List<EventResponseDto>();
        foreach (var item in data)
        {
            var e = await _eventService.CreateEventAsync(item.ToCreationDTO(), CancellationToken.None);
            events.Add(e);
        }

        return events;
    }

    private EventFilterRequestDTO createFilter(DateTimeOffset? from, DateTimeOffset? to)
    {
        return new EventFilterRequestDTO()
        {
            From = from,
            To = to,
        };
    }
}
