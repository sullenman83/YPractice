using EventManagement.Common;
using EventManagement.Common.Results;
using EventManagement.Interfaces;
using EventManagement.Models;
using EventManagement.Models.Events;
using EventManagement.Models.FilterModels;
using EventManagement.Services;
using FluentAssertions;
using Moq;
using System.Collections.Concurrent;
using static System.Runtime.InteropServices.JavaScript.JSType;
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
        var newEvent = TestData.GetTestEvent();
        var expectedResponse = new Result<EventResponseDto>()
        {
            Value = new EventResponseDto()
            {
                Title = newEvent.Title,
                Description = newEvent.Description,
                EndAt = newEvent.EndAt,
                StartAt = newEvent.StartAt,
                Id = 3
            },
            IsSuccess = true,
            Message = "",
            StatusCode = ResultStatusCode.Ok
        };

        //Решил для проверок воспользоваться данными по умолчанию внутри сервиса.
        //Чтобы они были предсказуемыми и одинаковыми для всех тестов сервис решил создавать внутри каждого теста свой        
        var service = getService(TestData.GetTestData());

        var result = service.CreateEvent(newEvent);

        _validator.Verify(s => s.Validate(It.IsAny<EventRequestDto>()), Times.Once);
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public void UpdateEvent_ReturnChangedEvent()
    {
        var ev = TestData.GetTestEvent();
        var id = 2;
        var expectedResponse = new Result<EventResponseDto>()
        {
            Value = new EventResponseDto()
            {
                Title = ev.Title,
                Description = ev.Description,
                EndAt = ev.EndAt,
                StartAt = ev.StartAt,
                Id = id
            },
            IsSuccess = true,
            Message = "",
            StatusCode = ResultStatusCode.Ok
        };
        var service = getService(TestData.GetTestData());

        var result = service.UpdateEvent(id, ev);
        
        _validator.Verify(s => s.Validate(It.IsAny<EventRequestDto>()), Times.Once);
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public void DeleteEvent_ReturnOk()
    {     
        var id = 2;
        var expectedResponse = new Result<EventResponseDto>()
        {
            Value = null,
            IsSuccess = true,
            Message = "",
            StatusCode = ResultStatusCode.Ok
        };
        var service = getService(TestData.GetTestData());

        var result = service.DeleteEvent(id);

        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public void GetEvent_ById_ReturnEventByID()
    {
        var id = 2;
        var ev = TestData.GetTestData().First(k => k.Key == id).Value;

        var expectedResponse = new Result<EventResponseDto>()
        {
            Value = new EventResponseDto()
            {
                Id = 2,
                Title = ev.Title,
                Description = ev.Description,
                StartAt = ev.StartAt,
                EndAt = ev.EndAt
            },
            IsSuccess = true,
            Message = "",
            StatusCode = ResultStatusCode.Ok
        };
        var service = getService(TestData.GetTestData());

        var result = service.GetEventById(id);        

        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public void GetEvents_ReturnAllEvent()
    {
        var data = TestData.GetTestData();
        var eventCount = data.Count;
        var filter = new EventFilterRequestDTO();        
        var service = getService(data);

        var result =  service.GetEvents(filter);

        result.Value?.Events.Count.Should().Be(eventCount);
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(ResultStatusCode.Ok);
        result.Message.Should().Be("");
    }

    [Fact]
    public void GetEvent_ByInvalidId_ReturnError()
    {
        var id = 10;
        var service = getService(TestData.GetTestData());
        var expectedResponse = new Result<EventResponseDto>()
        {
            Value = null,
            IsSuccess = false,
            Message = $"Ошбика при получении события по {id}.",
            StatusCode = ResultStatusCode.NotFound
        };

        var result = service.GetEventById(id);

        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public void UpdateEvent_ByInvalidId_ReturnError()
    {
        var id = 10;
        var ev = TestData.GetTestEvent();
        var service = getService(TestData.GetTestData());
        var expectedResponse = new Result<EventResponseDto>()
        {
            Value = null,
            IsSuccess = false,
            Message = $"Не найдено событие с id = {id}",
            StatusCode = ResultStatusCode.NotFound
        };

        var result = service.UpdateEvent(id, ev);

        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public void DeleteEvent_ByInvalidId_ReturnError()
    {
        var id = 10;        
        var service = getService(TestData.GetTestData());
        var expectedResponse = new Result<EventResponseDto>()
        {
            Value = null,
            IsSuccess = false,
            Message = $"Ошбика при удалении события {id}.",
            StatusCode = ResultStatusCode.NotFound
        };

        var result = service.DeleteEvent(id);

        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public void Updatevents_InvalidDate_ReturnError()
    {
        var id = 2;
        var ev = TestData.GetTestEvent();
        ev.EndAt = ev.StartAt.AddDays(-1);
        _repository.Setup(v => v.Data).Returns(() => new ConcurrentDictionary<int, Event>(TestData.GetTestData()));
        var service = new EventService(new EventValidator(), _repository.Object);
        
        var expectedResponse = new Result<EventResponseDto>()
        {
            Value = null,
            IsSuccess = false,
            Message = "Событие содержит некорректные данные. Дата окончания меньше даты начала.",
            StatusCode = ResultStatusCode.ValidationError
        };

        var result = service.UpdateEvent(id, ev);

        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public void CreateEvent_EventValidatorThrowsException()
    {
        var newEvent = TestData.GetTestEvent();
        var message = "Ошибка сервиса валидации";
        var expectedResponse = new Result<EventResponseDto>()
        {
            Value = null,
            IsSuccess = false,
            Message = message,
            StatusCode = ResultStatusCode.InternalError
        };
        var validator = new Mock<IEventValidator>();
        validator.Setup(v => v.Validate(It.IsAny<EventRequestDto>()))
            .Throws(new InvalidOperationException(message));        
        var service = new EventService(validator.Object, new EventRepository());

        var result = service.CreateEvent(newEvent);

        result.Should().BeEquivalentTo(expectedResponse);
    }

    private EventService getService(List<KeyValuePair<int, Event>> data)
    {
        _repository.Setup(v => v.Data).Returns(() => new ConcurrentDictionary<int, Event>(data));
        return new EventService(_validator.Object, _repository.Object);
    }
}
