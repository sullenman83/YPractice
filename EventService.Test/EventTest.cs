using EventManagement.Common;
using EventManagement.Common.Exceptions;
using EventManagement.Interfaces;
using EventManagement.Models.Events;
using EventManagement.Models.Events.Extensions;
using EventManagement.Models.FilterModels;
using EventManagement.Services;
using EventManagement.Services.EventServices;
using FluentAssertions;
using Moq;

namespace EventServiceTest;

public class EventTest
{
    private readonly Mock<IEventValidator> _validator;
    private readonly Mock<IEventRepository<Event>> _repository;

    public EventTest()
    {
        _validator = new Mock<IEventValidator>();
        _repository = new Mock<IEventRepository<Event>>();
        _validator.Setup(v => v.ValidateAsync(It.IsAny<EventCreationDTO>(), CancellationToken.None));
    }

    [Fact]
    public async Task CreateEvent_ReturnNewEvent()
    {
        // Arrange
        var ev = TestData.GetTestEvent();
        var evCreationDTO = TestData.GetTestEventCreationDTO();
        var expectedResponse = ev.ToResponse();


        _repository.Setup(o => o.AddAsync(It.IsAny<Event>())).ReturnsAsync((Event e, CancellationToken t) => e);
        _validator.Setup(o => o.ValidateAsync(It.IsAny<EventUpdateDTO>()));
        var service = new EventService(_validator.Object, _repository.Object);

        // Act
        var result = await service.CreateEventAsync(evCreationDTO, CancellationToken.None);

        // Assert
        _validator.Verify(s => s.ValidateAsync(It.IsAny<EventCreationDTO>()), Times.Once);
        _repository.Verify(v => v.AddAsync(It.IsAny<Event>()), Times.Once);
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

        _repository.Setup(o => o.GetByIdAsync(ev.Id)).ReturnsAsync(ev);
        _repository.Setup(o => o.SaveChangesAsync());

        var service = new EventService(_validator.Object, _repository.Object);

        // Act
        var result = await service.UpdateEventAsync(id, eventUpdateDTO, CancellationToken.None);

        // Assert
        _validator.Verify(s => s.ValidateAsync(It.IsAny<EventUpdateDTO>()), Times.Once);
        _repository.Verify(r => r.GetByIdAsync(ev.Id), Times.Once);
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
        var id = Guid.NewGuid();        
        _repository.Setup(r => r.DeleteAsync(id)).ReturnsAsync(true);
        var service = new EventService(_validator.Object, _repository.Object);        

        // Act
        await service.DeleteEventAsync(id, CancellationToken.None);

        // Assert
        _repository.Verify(r => r.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task GetEvent_ById_ReturnEventByID()
    {
        // Arrange
        var ev = TestData.GetTestEvent();
        var id = ev.Id;
        var expectedResponse = ev.ToResponse();
        _repository.Setup(o => o.GetByIdAsync(id)).ReturnsAsync(ev);
        var service = new EventService(_validator.Object, _repository.Object);

        // Act
        var result = await service.GetEventByIdAsync(id, CancellationToken.None);

        // Assert
        _repository.Verify(o => o.GetByIdAsync(id), Times.Once);
        result.Should().BeEquivalentTo(expectedResponse);
    }    

    [Fact]
    public async Task GetEvent_ByInvalidId_ThrowsNotFoundException()
    {
        // Arrange
        var id = new Guid("BBA0E5B9-B2D4-4B54-A9D0-7442969CBBF2");

        _repository.Setup(o => o.GetByIdAsync(id)).Throws<NotFoundException>();
        var service = new EventService(_validator.Object, _repository.Object);

        // Act
        Func<Task<EventResponseDto>> act = async () => await service.GetEventByIdAsync(id, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _repository.Verify(o => o.GetByIdAsync(id), Times.Once);
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
        _repository.Setup(o => o.GetByIdAsync(id)).Throws<NotFoundException>();
        var service = new EventService(_validator.Object, _repository.Object);

        // Act
        Func<Task<EventResponseDto>> act = async () => await service.UpdateEventAsync(id, ev, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _repository.Verify(o => o.GetByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task DeleteEvent_ByInvalidId_ThrowsNotFoundException()
    {

        // Arrange
        var id = new Guid("BBA0E5B9-B2D4-4B54-A9D0-7442969CBBF2");

        _repository.Setup(o => o.DeleteAsync(id)).Throws<NotFoundException>();
        var service = new EventService(_validator.Object, _repository.Object);

        // Act
        Func<Task> act = async () => await service.DeleteEventAsync(id, CancellationToken.None);

        // Assert        
        await act.Should().ThrowAsync<NotFoundException>();
        _repository.Verify(o => o.DeleteAsync(id), Times.Once);
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
        var service = new EventService(new EventValidator(), _repository.Object);

        // Act
        Func<Task<EventResponseDto>> act = async () => await service.UpdateEventAsync(id, ev, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<EventValidationException>();
    }

    [Fact]
    public async Task CreateEvent_EventValidatorThrowsException()
    {
        // Arrange
        var newEvent = TestData.GetTestEventCreationDTO();
        var message = "Ошибка сервиса валидации";

        _validator.Setup(v => v.ValidateAsync(It.IsAny<EventCreationDTO>(), CancellationToken.None))
            .Throws(new EventValidationException(message));
        var service = new EventService(_validator.Object, _repository.Object);

        // Act
        Func<Task<EventResponseDto>> act = async () => await service.CreateEventAsync(newEvent, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<EventValidationException>();
        _validator.Verify(o => o.ValidateAsync(newEvent, CancellationToken.None), Times.Once);
    }
}