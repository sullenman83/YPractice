using EventManagement.Common;
using EventManagement.Common.Exceptions;
using EventManagement.Interfaces;
using EventManagement.Models.Events;
using EventManagement.Models.Events.Extensions;
using EventManagement.Models.FilterModels;
using EventManagement.Services;
using FluentAssertions;
using Moq;
using System.Collections.Concurrent;

namespace EventServiceTest;

public class EventTest
{    
    private readonly Mock<IEventValidator> _validator;
    private readonly Mock<IEventRepository> _repository;    

    public EventTest()
    {
        _validator = new Mock<IEventValidator>();
        _repository = new Mock<IEventRepository>();
        _validator.Setup(v => v.ValidateAsync(It.IsAny<CreateEventDTO>(), CancellationToken.None));       
    }

    [Fact]
    public async Task CreateEvent_ReturnNewEvent()
    {
	    // Arrange
        var newEvent = TestData.GetTestEvent();
        var expectedResponse = new EventResponseDto()
        {
            Title = newEvent.Title,
            Description = newEvent.Description,
            EndAt = newEvent.EndAt.GetValueOrDefault(),
            StartAt = newEvent.StartAt.GetValueOrDefault(),
            TotalSeats = newEvent.TotalSeats.GetValueOrDefault(),
            AvailableSeats = newEvent.TotalSeats.GetValueOrDefault()
        };

        _repository.Setup(o => o.Add()).Returns(expectedResponse);
        var service = getService(TestData.GetTestData());

	    // Act
        var result = await service.CreateEventAsync(newEvent, CancellationToken.None);

	    // Assert
        _validator.Verify(s => s.ValidateAsync(It.IsAny<CreateEventDTO>(), CancellationToken.None), Times.Once);
        result.Title.Should().BeEquivalentTo(expectedResponse.Title);
        result.Description.Should().BeEquivalentTo(expectedResponse.Description);
        result.EndAt.Should().BeSameDateAs(expectedResponse.EndAt);
        result.StartAt.Should().BeSameDateAs(expectedResponse.StartAt);
        result.TotalSeats.Should().Be(expectedResponse.TotalSeats);
        result.AvailableSeats.Should().Be(expectedResponse.AvailableSeats);
    }

    [Fact]
    public async Task UpdateEvent_ReturnChangedEvent()
    {
        // Arrange
        var data = TestData.GetTestData();
        var ev = data.First().Value;
        var updateEvent = new UpdateEventDTO()
        {
            Title = ev.Title + "test",
            Description = ev.Description + "TestDescription",
            StartAt = ev.StartAt.AddDays(1),
            EndAt = ev.EndAt.AddDays(2)            
        };
        
        var id = ev.Id;
        var expectedResponse = ev.ToResponse();
        var service = getService(data);

	    // Act
        var result = await service.UpdateEventAsync(id, updateEvent, CancellationToken.None);
        
	    // Assert
        _validator.Verify(s => s.ValidateAsync(It.IsAny<CreateEventDTO>(), CancellationToken.None), Times.Once);
        result.Title.Should().BeEquivalentTo(expectedResponse.Title);
        result.Description.Should().BeEquivalentTo(expectedResponse.Description);
        result.EndAt.Should().BeSameDateAs(expectedResponse.EndAt);
        result.StartAt.Should().BeSameDateAs(expectedResponse.StartAt);
        result.TotalSeats.Should().Be(expectedResponse.TotalSeats);
        result.AvailableSeats.Should().Be(expectedResponse.AvailableSeats);
    }

    [Fact]
    public async Task DeleteEvent_ReturnOk()
    {
        // Arrange
        var eventRepository = new EventRepository();
        var events = eventRepository.GetAll().ToList();        
        var id = events.First().Id;        
        var eventCount = events.Count;
        var filter = new EventFilterRequestDTO();
        var service = new EventService(new EventValidator(), eventRepository);

        // Act
        await service.DeleteEventAsync(id, CancellationToken.None);
        var remains = await service.GetEventsAsync(filter, CancellationToken.None);

        // Assert        
        remains.Events.Should().NotContain(o => o.Id == id);
    }

    [Fact]
    public async Task GetEvent_ById_ReturnEventByID()
    {
        // Arrange
        var data = TestData.GetTestData();
        var ev = data.First().Value;
        var id = ev.Id;

        var expectedResponse = ev.ToResponse();
        
        var service = getService(TestData.GetTestData());

	    // Act
        var result = await service.GetEventByIdAsync(id, CancellationToken.None);        

	    // Assert
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetEvents_ReturnAllEvent()
    {
	    // Arrange
        var data = TestData.GetTestData();
        var eventCount = data.Count;
        var filter = new EventFilterRequestDTO();        
        var service = getService(data);

	    // Act
        var result =  await service.GetEventsAsync(filter, CancellationToken.None);

	    // Assert
        result.Events.Count.Should().Be(eventCount);
    }

    [Fact]
    public void GetEvent_ByInvalidId_ReturnError()
    {        
	    // Arrange
        var id = new Guid("BBA0E5B9-B2D4-4B54-A9D0-7442969CBBF2");
        var service = getService(TestData.GetTestData());        

	    // Act
        Func<Task<EventResponseDto>> act = async () => await service.GetEventByIdAsync(id, CancellationToken.None);

        // Assert
        act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public void UpdateEvent_ByInvalidId_ReturnError()
    {
	    // Arrange
        var id = new Guid("BBA0E5B9-B2D4-4B54-A9D0-7442969CBBF2");
        var testEvent = TestData.GetTestEvent();
        var ev = new UpdateEventDTO()
        {
            Title = testEvent.Title,
            Description = testEvent.Description,
            EndAt = testEvent.EndAt,
            StartAt = testEvent.StartAt,
        };
        var service = getService(TestData.GetTestData());        

	    // Act
        Func<Task<EventResponseDto>> act = async () => await service.UpdateEventAsync(id, ev, CancellationToken.None);

	    // Assert
        act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public void DeleteEvent_ByInvalidId_ReturnError()
    {
	    // Arrange
        var id = new Guid("BBA0E5B9-B2D4-4B54-A9D0-7442969CBBF2");        
        var service = getService(TestData.GetTestData());        

	    // Act
        Func<Task> act = async () => await service.DeleteEventAsync(id, CancellationToken.None);

        // Assert
        act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public void Updatevents_InvalidDate_ReturnError()
    {
        // Arrange
        var id = TestData.GetTestEvents().First().Id;
        var ev = TestData.GetTestEvent();
        ev.EndAt = ev.StartAt.AddDays(-1);
        _repository.Setup(v => v.Data).Returns(() => new ConcurrentDictionary<Guid, Event>(TestData.GetTestData()));
        var service = new EventService(new EventValidator(), _repository.Object);

	    // Act
        Func<Task<EventResponseDto>> act = async ()=> await service.UpdateEventAsync(id, ev, CancellationToken.None);

        // Assert
        act.Should().ThrowAsync<EventValidationException>();
    }

    [Fact]
    public void CreateEvent_EventValidatorThrowsException()
    {
	    // Arrange
        var newEvent = TestData.GetTestEvent();
        var message = "Ошибка сервиса валидации";
        
        var validator = new Mock<IEventValidator>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<CreateEventDTO>(), CancellationToken.None))
            .Throws(new InvalidOperationException(message));        
        var service = new EventService(validator.Object, new EventRepository());

	    // Act
        Func<Task<EventResponseDto>> act = async () => await service.CreateEventAsync(newEvent, CancellationToken.None);

        // Assert
        act.Should().ThrowAsync<InvalidOperationException>();
    }

    //private EventService getService(List<KeyValuePair<Guid, Event>> data)
    //{
    //    _repository.Setup(v => v.Data).Returns(() => new ConcurrentDictionary<Guid, Event>(data));
    //    return new EventService(_validator.Object, _repository.Object);
    //}
}

