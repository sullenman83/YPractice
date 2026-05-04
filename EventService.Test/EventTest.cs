using EventManagement.Common;
using EventManagement.Common.Exceptions;
using EventManagement.Data;
using EventManagement.Interfaces;
using EventManagement.Models.Events;
using EventManagement.Models.Events.Extensions;
using EventManagement.Models.FilterModels;
using EventManagement.Services.EventServices;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventServiceTest;

public class EventTest : IDisposable
{    
    private readonly IEventValidator _eventValidator;
    private readonly ServiceProvider _serviceProvider;
    private readonly IEventService _eventService;
    private readonly IServiceScope _scope;


    public EventTest()
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
    public async Task CreateEvent_ReturnNewEvent()
    {
	    // Arrange
        var ev = TestData.GetTestEvent();
        var evCreationDTO = ev.ToCreationDTO();
        var expectedResponse = ev.ToResponse();

	    // Act
        var result = await _eventService.CreateEventAsync(evCreationDTO, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().BeEquivalentTo(expectedResponse.Title);
        result.Description.Should().BeEquivalentTo(expectedResponse.Description);
        result.EndAt.Should().BeSameDateAs(expectedResponse.EndAt);
        result.StartAt.Should().BeSameDateAs(expectedResponse.StartAt);
        result.TotalSeats.Should().Be(expectedResponse.TotalSeats);
        result.AvailableSeats.Should().Be(expectedResponse.AvailableSeats);
    }

    [Fact]
    public async Task CreateEvent_EventValidatorThrowsException()
    {
        // Arrange
        var ev = TestData.GetTestEvent();
        var evCreationDTO = ev.ToCreationDTO();
        evCreationDTO.EndAt = evCreationDTO.StartAt.HasValue ? evCreationDTO.StartAt.Value.AddDays(-1) : throw new ArgumentException("Не задана дата");
        
        // Act
        Func<Task<EventResponseDto>> act = async () => await _eventService.CreateEventAsync(evCreationDTO, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<EventValidationException>();        
    }


    [Fact]
    public async Task UpdateEvent_ReturnChangedEvent()
    {
        // Arrange
        var ev = TestData.GetTestEvent();
        var newEvent = await _eventService.CreateEventAsync(ev.ToCreationDTO(), CancellationToken.None);

        var eventUpdateDTO = new EventUpdateDTO()
        {
            Title = newEvent.Title + "test",
            Description = newEvent.Description + "TestDescription",
            StartAt = newEvent.StartAt.AddDays(1),
            EndAt = newEvent.EndAt.AddDays(2),
        };

        var id = newEvent.Id;
        //var expectedResponse = ev.ToResponse();
        //expectedResponse.Title = eventUpdateDTO.Title;
        //expectedResponse.Description = eventUpdateDTO.Description;
        //expectedResponse.EndAt = eventUpdateDTO.EndAt ?? throw new ArgumentNullException("поле не должно быть null");
        //expectedResponse.StartAt = eventUpdateDTO.StartAt ?? throw new ArgumentNullException("поле не должно быть null");
        
        // Act
        var result = await _eventService.UpdateEventAsync(id, eventUpdateDTO, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(id);
        result.Title.Should().BeEquivalentTo(eventUpdateDTO.Title);
        result.Description.Should().BeEquivalentTo(eventUpdateDTO.Description);
        result.EndAt.Should().BeSameDateAs(eventUpdateDTO.EndAt ?? throw new ArgumentNullException("Не задана дата"));
        result.StartAt.Should().BeSameDateAs(eventUpdateDTO.StartAt ?? throw new ArgumentNullException("Не задана дата"));
    }

    [Fact]
    public async Task UpdateEvent_ByInvalidId_ThrowsNotFoundException()
    {
        // Arrange
        var id = new Guid("BBA0E5B9-B2D4-4B54-A9D0-7442969CBBF2");
        var testEvent = TestData.GetTestEvent();
        var ev = new EventUpdateDTO()
        {
            Title = testEvent.Title,
            Description = testEvent.Description,
            EndAt = testEvent.EndAt,
            StartAt = testEvent.StartAt,
        };
        
        // Act
        Func<Task<EventResponseDto>> act = async () => await _eventService.UpdateEventAsync(id, ev, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Updatevents_InvalidDate_ThrowsEventValidationException()
    {
        // Arrange
        var testEvent = TestData.GetTestEvent();
        var id = testEvent.Id;
        var ev = new EventUpdateDTO()
        {
            Title = testEvent.Title,
            Description = testEvent.Description,
            EndAt = testEvent.EndAt,
            StartAt = testEvent.StartAt,
        };
        ev.EndAt = ev.StartAt?.AddDays(-1);        

        // Act
        Func<Task<EventResponseDto>> act = async () => await _eventService.UpdateEventAsync(id, ev, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<EventValidationException>();
    }
       

    [Fact]
    public async Task GetEvent_ById_ReturnEventByID()
    {
        // Arrange
        var ev = TestData.GetTestEvent();
        var newEvent = await _eventService.CreateEventAsync(ev.ToCreationDTO(), CancellationToken.None);        
        var id = newEvent.Id;        
        
        // Act
        var result = await _eventService.GetEventByIdAsync(id, CancellationToken.None);

        // Assert        
        result.Should().NotBeNull();
        result.Id.Should().Be(newEvent.Id);
        result.Title.Should().Be(newEvent.Title);
        result.Description.Should().Be(newEvent.Description);
        result.AvailableSeats.Should().Be(newEvent.AvailableSeats);
        result.TotalSeats.Should().Be(newEvent.TotalSeats);
        result.StartAt.Should().Be(newEvent.StartAt);
        result.EndAt.Should().Be(newEvent.EndAt);


    }

    [Fact]
    public async Task GetEvents_ReturnAllEvent()
    {
        // Arrange
        var data = TestData.GetTestEvents();
        var cratedEvents = new List<EventResponseDto>();
        foreach (var e in data)
        {
            var ev = await _eventService.CreateEventAsync(e.ToCreationDTO(), CancellationToken.None);
            cratedEvents.Add(ev);
        }

        var eventCount = data.Count;
        var filter = new EventFilterRequestDTO();        
        
        // Act
        var result = await _eventService.GetEventsAsync(filter, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Events.Should().BeEquivalentTo(cratedEvents);
    }

    [Fact]
    public async Task GetEvent_ByInvalidId_ThrowsNotFoundException()
    {
        // Arrange
        var id = new Guid("BBA0E5B9-B2D4-4B54-A9D0-7442969CBBF2");
        
        // Act
        Func<Task<EventResponseDto>> act = async () => await _eventService.GetEventByIdAsync(id, CancellationToken.None);

        // Assert        
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteEvent_ReturnOk()
    {
        // Arrange
        var ev = TestData.GetTestEvent();
        var newEvent = await _eventService.CreateEventAsync(ev.ToCreationDTO(), CancellationToken.None);
        var result = await _eventService.GetEventsAsync(new EventFilterRequestDTO(), CancellationToken.None);
        var cnt = result.EventsCount;

        // Act
        await _eventService.DeleteEventAsync(newEvent.Id, CancellationToken.None);
        result = await _eventService.GetEventsAsync(new EventFilterRequestDTO(), CancellationToken.None);

        // Assert        
        result.EventsCount.Should().Be(cnt - 1);
        result.Events.Should().NotContain(e => e.Id == ev.Id);
    }

    [Fact]
    public async Task DeleteEvent_ByInvalidId_ThrowsNotFoundException()
    {
        // Arrange
        var id = new Guid("BBA0E5B9-B2D4-4B54-A9D0-7442969CBBF2");
        
        // Act
        Func<Task> act = async () => await _eventService.DeleteEventAsync(id, CancellationToken.None);

        // Assert        
        await act.Should().ThrowAsync<NotFoundException>();
    } 
}

