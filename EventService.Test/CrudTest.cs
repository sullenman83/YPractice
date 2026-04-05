using EventManagement.Common;
using EventManagement.Common.Exceptions;
using EventManagement.Interfaces;
using EventManagement.Models.Events;
using EventManagement.Models.FilterModels;
using EventManagement.Services;
using FluentAssertions;
using Moq;
using System.Collections.Concurrent;

namespace EventServiceTest;

public class CRUDTest
{    
    private readonly Mock<IEventValidator> _validator;
    private readonly Mock<IEventRepository> _repository;

    public CRUDTest()
    {
        _validator = new Mock<IEventValidator>();
        _repository = new Mock<IEventRepository>();        
        
        _validator.Setup(v => v.Validate(It.IsAny<EventRequestDto>()));
    }

    [Fact]
    public void CreateEvent_ReturnNewEvent()
    {
	    // Arrange
        var newEvent = TestData.GetTestEvent();
        var expectedResponse = new EventResponseDto()
        {
            Title = newEvent.Title,
            Description = newEvent.Description,
            EndAt = newEvent.EndAt,
            StartAt = newEvent.StartAt,
        };

        //Решил для проверок воспользоваться данными по умолчанию внутри сервиса.
        //Чтобы они были предсказуемыми и одинаковыми для всех тестов сервис решил создавать внутри каждого теста свой        
        var service = getService(TestData.GetTestData());

	    // Act
        var result = service.CreateEvent(newEvent);

	    // Assert
        _validator.Verify(s => s.Validate(It.IsAny<EventRequestDto>()), Times.Once);
        result.Title.Should().BeEquivalentTo(expectedResponse.Title);
        result.Description.Should().BeEquivalentTo(expectedResponse.Description);
        result.EndAt.Should().BeSameDateAs(expectedResponse.EndAt);
        result.StartAt.Should().BeSameDateAs(expectedResponse.StartAt);
    }

    [Fact]
    public void UpdateEvent_ReturnChangedEvent()
    {
        // Arrange
        var id = TestData.GetTestEvents().First().Id;
        var ev = TestData.GetTestEvent();
        var expectedResponse = new EventResponseDto()
        {            
            Title = ev.Title,
            Description = ev.Description,
            EndAt = ev.EndAt,
            StartAt = ev.StartAt,
        };
        var service = getService(TestData.GetTestData());

	    // Act
        var result = service.UpdateEvent(id, ev);
        
	    // Assert
        _validator.Verify(s => s.Validate(It.IsAny<EventRequestDto>()), Times.Once);
        result.Title.Should().BeEquivalentTo(expectedResponse.Title);
        result.Description.Should().BeEquivalentTo(expectedResponse.Description);
        result.EndAt.Should().BeSameDateAs(expectedResponse.EndAt);
        result.StartAt.Should().BeSameDateAs(expectedResponse.StartAt);
    }

    [Fact]
    public void DeleteEvent_ReturnOk()
    {
        // Arrange
        var id = TestData.GetTestEvents().First().Id;
        var testData = TestData.GetTestData();
        var eventCount = testData.Count;
        var filter = new EventFilterRequestDTO();
        var service = new EventService(new EventValidator(), new EventRepository());

        // Act
        service.DeleteEvent(id);
        var remains = service.GetEvents(filter);

        // Assert        
        remains.Events.Should().NotContain(o => o.Id == id);
    }

    [Fact]
    public void GetEvent_ById_ReturnEventByID()
    {
        // Arrange
        var id = TestData.GetTestEvents().First().Id;
        var ev = TestData.GetTestData().First(k => k.Key == id).Value;

        var expectedResponse = new EventResponseDto()
        {            
            Id = id,
            Title = ev.Title,
            Description = ev.Description,
            StartAt = ev.StartAt,
            EndAt = ev.EndAt
        };
        var service = getService(TestData.GetTestData());

	    // Act
        var result = service.GetEventById(id);        

	    // Assert
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public void GetEvents_ReturnAllEvent()
    {
	    // Arrange
        var data = TestData.GetTestData();
        var eventCount = data.Count;
        var filter = new EventFilterRequestDTO();        
        var service = getService(data);

	    // Act
        var result =  service.GetEvents(filter);

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
        Action act = () => service.GetEventById(id);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateEvent_ByInvalidId_ReturnError()
    {
	    // Arrange
        var id = new Guid("BBA0E5B9-B2D4-4B54-A9D0-7442969CBBF2");
        var ev = TestData.GetTestEvent();
        var service = getService(TestData.GetTestData());        

	    // Act
        Action act = () => service.UpdateEvent(id, ev);

	    // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void DeleteEvent_ByInvalidId_ReturnError()
    {
	    // Arrange
        var id = new Guid("BBA0E5B9-B2D4-4B54-A9D0-7442969CBBF2");        
        var service = getService(TestData.GetTestData());        

	    // Act
        Action act = () => service.DeleteEvent(id);

        // Assert
        act.Should().Throw<ArgumentException>();
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
        Action act = ()=> service.UpdateEvent(id, ev);

        // Assert
        act.Should().Throw<EventValidationException>();
    }

    [Fact]
    public void CreateEvent_EventValidatorThrowsException()
    {
	    // Arrange
        var newEvent = TestData.GetTestEvent();
        var message = "Ошибка сервиса валидации";
        
        var validator = new Mock<IEventValidator>();
        validator.Setup(v => v.Validate(It.IsAny<EventRequestDto>()))
            .Throws(new InvalidOperationException(message));        
        var service = new EventService(validator.Object, new EventRepository());

	    // Act
        Action act = () => service.CreateEvent(newEvent);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    private EventService getService(List<KeyValuePair<Guid, Event>> data)
    {
        _repository.Setup(v => v.Data).Returns(() => new ConcurrentDictionary<Guid, Event>(data));
        return new EventService(_validator.Object, _repository.Object);
    }
}
