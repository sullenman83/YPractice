using EventManagement.Common.Results;
using EventManagement.Common;
using EventManagement.Interfaces;
using EventManagement.Models;
using EventManagement.Models.Events;
using EventManagement.Services;
using FluentAssertions;
using Moq;
using EventManagement.Models.FilterModels;
namespace EventServiceTest;

public class CRUDTest
{    
    private readonly Mock<IEventValidator> _validator;

    public CRUDTest()
    {
        _validator = new Mock<IEventValidator>();
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
        var service = new EventService(_validator.Object);

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
        var service = new EventService(_validator.Object);
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

        var service = new EventService(_validator.Object);
        var result = service.DeleteEvent(id);

        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public void GetEvent_ById_ReturnEventByID()
    {
        var id = 2;
        var expectedResponse = new Result<EventResponseDto>()
        {
            Value = new EventResponseDto()
            {
                Id = 2,
                Title = "Событие 2",
                Description = "Описание 21",
                StartAt = DateTime.Parse("2026.03.24"),
                EndAt = DateTime.Parse("2026.03.27")
            },
            IsSuccess = true,
            Message = "",
            StatusCode = ResultStatusCode.Ok
        };
        var service = new EventService(_validator.Object);

        var result = service.GetEventById(id);        

        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public void GetEvents_ReturnAllEvent()
    {
        var eventCount = 2;
        var filter = new EventFilterRequestDTO();        
        var service = new EventService(_validator.Object);

        var result =  service.GetEvents(filter);

        result.Value?.Events.Count.Should().Be(eventCount);
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(ResultStatusCode.Ok);
        result.Message.Should().Be("");
    }    
}
