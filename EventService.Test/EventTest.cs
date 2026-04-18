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
        _validator.Setup(v => v.ValidateAsync(It.IsAny<EventCreationDTO>(), CancellationToken.None));       
    }

    [Fact]
    public async Task CreateEvent_ReturnNewEvent()
    {
	    // Arrange
        var ev = TestData.GetTestEvent();
        var evCreationDTO = TestData.GetTestEventCreationDTO();
        var expectedResponse = ev.ToResponse();
        

        _repository.Setup(o => o.Add(It.IsAny<Event>())).Returns<Event>(e => e);
        _validator.Setup(o => o.ValidateAsync(It.IsAny<EventUpdateDTO>(), CancellationToken.None));
        var service = new EventService(_validator.Object, _repository.Object);

	    // Act
        var result = await service.CreateEventAsync(evCreationDTO, CancellationToken.None);

	    // Assert
        _validator.Verify(s => s.ValidateAsync(It.IsAny<EventCreationDTO>(), CancellationToken.None), Times.Once);        
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
        var ev = TestData.GetTestEvent();

        //var data = TestData.GetTestData();
        //var ev = data.First().Value;
        var eventUpdateDTO = new EventUpdateDTO()
        {
            Title = ev.Title + "test",
            Description = ev.Description + "TestDescription",
            StartAt = ev.StartAt.AddDays(1),
            EndAt = ev.EndAt.AddDays(2)
        };

        var id = ev.Id;
        var expectedResponse = ev.ToResponse();
        expectedResponse.Title = eventUpdateDTO.Title;
        expectedResponse.Description = eventUpdateDTO.Description;
        expectedResponse.EndAt = eventUpdateDTO.EndAt ?? throw new ArgumentNullException("поле не должно быть null");
        expectedResponse.StartAt = eventUpdateDTO.StartAt ?? throw new ArgumentNullException("поле не должно быть null");

        _repository.Setup(o => o.GetByID(It.IsAny<Guid>())).Returns(ev);
        _repository.Setup(o => o.Update(It.IsAny<Event>())).Returns<Event>(e => e);

        var service = new EventService(_validator.Object, _repository.Object);

        // Act
        var result = await service.UpdateEventAsync(id, eventUpdateDTO, CancellationToken.None);

        // Assert
        _validator.Verify(s => s.ValidateAsync(It.IsAny<EventUpdateDTO>(), CancellationToken.None), Times.Once);
        result.Id.Should().Be(expectedResponse.Id);
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
        var service = new EventService(_validator.Object, _repository.Object);
        var id = Guid.NewGuid();

        // Act
        await service.DeleteEventAsync(id, CancellationToken.None);

        // Assert        
        _repository.Verify(r => r.Delete(id), Times.Once);
    }

    [Fact]
    public async Task GetEvent_ById_ReturnEventByID()
    {
        // Arrange
        var ev = TestData.GetTestEvent();        
        var id = ev.Id;
        var expectedResponse = ev.ToResponse();
        _repository.Setup(o => o.GetByID(It.IsAny<Guid>())).Returns(ev);
        var service = new EventService(_validator.Object, _repository.Object);

        // Act
        var result = await service.GetEventByIdAsync(id, CancellationToken.None);

        // Assert
        _repository.Verify(o => o.GetByID(id), Times.Once);
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetEvents_ReturnAllEvent()
    {
        // Arrange
        var data = TestData.GetTestEvents();
        var eventCount = data.Count;
        var filter = new EventFilterRequestDTO();
        var response = data.Select(o => o.ToResponse());

        _repository.Setup(o => o.GetAll()).Returns(data);
        var service = new EventService(_validator.Object, _repository.Object);

        // Act
        var result = await service.GetEventsAsync(filter, CancellationToken.None);

        // Assert
        result.Events.Should().BeEquivalentTo(response);
    }

    [Fact]
    public void GetEvent_ByInvalidId_ThrowsArgumentException()
    {
        // Arrange
        var id = new Guid("BBA0E5B9-B2D4-4B54-A9D0-7442969CBBF2");

        _repository.Setup(o => o.GetByID(It.IsAny<Guid>())).Throws<ArgumentException>();
        var service = new EventService(_validator.Object, _repository.Object);

        // Act
        Func<Task<EventResponseDto>> act = async () => await service.GetEventByIdAsync(id, CancellationToken.None);

        // Assert        
        act.Should().ThrowAsync<ArgumentException>();
        _repository.Verify(o => o.GetByID(id), Times.Once);
    }

    [Fact]
    public void UpdateEvent_ByInvalidId_ThrowsArgumentException()
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
        _repository.Setup(o => o.GetByID(It.IsAny<Guid>())).Throws<ArgumentException>();
        var service = new EventService(_validator.Object, _repository.Object);

        // Act
        Func<Task<EventResponseDto>> act = async () => await service.UpdateEventAsync(id, ev, CancellationToken.None);

        // Assert
        act.Should().ThrowAsync<ArgumentException>();
        _repository.Verify(o => o.GetByID(id), Times.Once);
    }

    [Fact]
    public void DeleteEvent_ByInvalidId_ThrowsArgumentException()
    {

        // Arrange
        var id = new Guid("BBA0E5B9-B2D4-4B54-A9D0-7442969CBBF2");

        _repository.Setup(o => o.Delete(It.IsAny<Guid>())).Throws<ArgumentException>();
        var service = new EventService(_validator.Object, _repository.Object);

        // Act
        Func<Task> act = async () => await service.DeleteEventAsync(id, CancellationToken.None);

        // Assert        
        act.Should().ThrowAsync<ArgumentException>();
        _repository.Verify(o => o.Delete(id), Times.Once);
    }

    [Fact]
    public void Updatevents_InvalidDate_ThrowsArgumentException()
    {
        // Arrange
        var testEvent = TestData.GetTestEvent();
        var id = testEvent.Id;
        var ev = new EventUpdateDTO()
        {
            Title = testEvent. Title,
            Description = testEvent.Description,
            EndAt = testEvent.EndAt,
            StartAt = testEvent.StartAt,
        };
        ev.EndAt = ev.StartAt?.AddDays(-1);        
        var service = new EventService(new EventValidator(), new EventRepository());

        // Act
        Func<Task<EventResponseDto>> act = async () => await service.UpdateEventAsync(id, ev, CancellationToken.None);

        // Assert
        act.Should().ThrowAsync<EventValidationException>();
    }

    [Fact]
    public void CreateEvent_EventValidatorThrowsException()
    {
        // Arrange
        var newEvent = TestData.GetTestEventCreationDTO();
        var message = "Ошибка сервиса валидации";
                
        _validator.Setup(v => v.ValidateAsync(It.IsAny<EventCreationDTO>(), CancellationToken.None))
            .Throws(new InvalidOperationException(message));
        var service = new EventService(_validator.Object, new EventRepository());

        // Act
        Func<Task<EventResponseDto>> act = async () => await service.CreateEventAsync(newEvent, CancellationToken.None);

        // Assert
        act.Should().ThrowAsync<InvalidOperationException>();
        _validator.Verify(o => o.ValidateAsync(newEvent, CancellationToken.None), Times.Once);
    }

    //private EventService getService(List<KeyValuePair<Guid, Event>> data)
    //{
    //    _repository.Setup(v => v.Data).Returns(() => new ConcurrentDictionary<Guid, Event>(data));
    //    return new EventService(_validator.Object, _repository.Object);
    //}
}

