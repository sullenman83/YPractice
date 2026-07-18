using Events.Application.Common;
using Events.Application.Interfaces;
using Events.Application.Interfaces.Repositories;
using Events.Application.Interfaces.Validators;
using Events.Application.Models;
using Events.Application.Models.Extensions;
using Events.Application.Services;
using Events.Application.Services.Validators;
using Events.Application.Settings;
using Events.Domain.Exceptions;
using Events.Domain.Models;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using TestData;

namespace Events.UnitTests
{
    public class EventServiceTest
    {
        private readonly Mock<IEventValidator> _mockValidator;
        private readonly Mock<IEventRepository> _mockEventRepository;
        private readonly Mock<ICacheService> _mockCache;
        private readonly IOptions<TTLSettings> _settings;

        public EventServiceTest()
        {
            _mockValidator = new Mock<IEventValidator>();
            _mockEventRepository = new Mock<IEventRepository>();
            _mockValidator.Setup(v => v.Validate(It.IsAny<EventCreationDTO>()));
            _mockCache = new Mock<ICacheService>();
            var settings = new TTLSettings()
            {
                EventTTL = 60,
                Top10TTL = 900
            };

            _settings = Options.Create(settings);
        }

        [Fact]
        public async Task CreateEvent_ReturnNewEvent()
        {
            // Arrange
            var ev = EventTestData.GetTestEvent();
            var evCreationDTO = EventTestData.GetTestEventCreationDTO();
            var expectedResponse = ev.ToResponse();

            _mockEventRepository.Setup(o => o.AddAsync(It.IsAny<Event>())).ReturnsAsync((Event e, CancellationToken t) => e);            
            var service = new EventService(_mockValidator.Object, _mockEventRepository.Object, _mockCache.Object, _settings);

            // Act
            var result = await service.CreateEventAsync(evCreationDTO, CancellationToken.None);

            // Assert
            _mockValidator.Verify(s => s.Validate(It.IsAny<EventCreationDTO>()), Times.Once);
            _mockEventRepository.Verify(v => v.AddAsync(It.IsAny<Event>()), Times.Once);
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
            var ev = EventTestData.GetTestEvent();

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

            _mockEventRepository.Setup(o => o.GetByIdAsync(ev.Id)).ReturnsAsync(ev);
            _mockEventRepository.Setup(o => o.SaveChangesAsync());

            var service = new EventService(_mockValidator.Object, _mockEventRepository.Object, _mockCache.Object, _settings);

            // Act
            var result = await service.UpdateEventAsync(id, eventUpdateDTO, CancellationToken.None);

            // Assert
            _mockValidator.Verify(s => s.Validate(It.IsAny<EventUpdateDTO>()), Times.Once);
            _mockEventRepository.Verify(r => r.GetByIdAsync(ev.Id), Times.Once);
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
            _mockEventRepository.Setup(r => r.DeleteAsync(id)).ReturnsAsync(true);
            var service = new EventService(_mockValidator.Object, _mockEventRepository.Object, _mockCache.Object, _settings);

            // Act
            await service.DeleteEventAsync(id, CancellationToken.None);

            // Assert
            _mockEventRepository.Verify(r => r.DeleteAsync(id), Times.Once);
        }

        [Fact]
        public async Task GetEvent_ById_ReturnEventByID()
        {
            // Arrange
            var ev = EventTestData.GetTestEvent();
            var id = ev.Id;
            var expectedResponse = ev.ToResponse();
            _mockEventRepository.Setup(o => o.GetByIdAsync(id)).ReturnsAsync(ev);
            var service = new EventService(_mockValidator.Object, _mockEventRepository.Object, _mockCache.Object, _settings);

            // Act
            var result = await service.GetEventByIdAsync(id, CancellationToken.None);

            // Assert
            _mockEventRepository.Verify(o => o.GetByIdAsync(id), Times.Once);
            result.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task GetEvent_ByInvalidId_ThrowsNotFoundException()
        {
            // Arrange
            var id = new Guid("BBA0E5B9-B2D4-4B54-A9D0-7442969CBBF2");

            _mockEventRepository.Setup(o => o.GetByIdAsync(id)).Throws<NotFoundException>();
            var service = new EventService(_mockValidator.Object, _mockEventRepository.Object, _mockCache.Object, _settings);

            // Act
            Func<Task<EventResponseDto>> act = async () => await service.GetEventByIdAsync(id, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
            _mockEventRepository.Verify(o => o.GetByIdAsync(id), Times.Once);
        }

        [Fact]
        public async Task UpdateEvent_ByInvalidId_ThrowsNotFoundException()
        {
            // Arrange
            var id = new Guid("BBA0E5B9-B2D4-4B54-A9D0-7442969CBBF2");
            var testEvent = EventTestData.GetTestEvent();
            var ev = new EventUpdateDTO()
            {
                Title = testEvent.Title,
                Description = testEvent.Description,
                EndAt = testEvent.EndAt,
                StartAt = testEvent.StartAt,
            };
            _mockEventRepository.Setup(o => o.GetByIdAsync(id)).Throws<NotFoundException>();
            var service = new EventService(_mockValidator.Object, _mockEventRepository.Object, _mockCache.Object, _settings);

            // Act
            Func<Task<EventResponseDto>> act = async () => await service.UpdateEventAsync(id, ev, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
            _mockEventRepository.Verify(o => o.GetByIdAsync(id), Times.Once);
        }

        [Fact]
        public async Task DeleteEvent_ByInvalidId_ThrowsNotFoundException()
        {

            // Arrange
            var id = new Guid("BBA0E5B9-B2D4-4B54-A9D0-7442969CBBF2");

            _mockEventRepository.Setup(o => o.DeleteAsync(id)).Throws<NotFoundException>();
            var service = new EventService(_mockValidator.Object, _mockEventRepository.Object, _mockCache.Object, _settings);

            // Act
            Func<Task> act = async () => await service.DeleteEventAsync(id, CancellationToken.None);

            // Assert        
            await act.Should().ThrowAsync<NotFoundException>();
            _mockEventRepository.Verify(o => o.DeleteAsync(id), Times.Once);
        }

        [Fact]
        public async Task Updatevents_InvalidDate_ThrowsEventValidationException()
        {
            // Arrange
            var testEvent = EventTestData.GetTestEvent();
            var id = testEvent.Id;
            var ev = new EventUpdateDTO()
            {
                Title = testEvent.Title,
                Description = testEvent.Description,
                EndAt = testEvent.EndAt,
                StartAt = testEvent.StartAt,
            };
            ev.EndAt = ev.StartAt?.AddDays(-1);
            var service = new EventService(new EventValidator(), _mockEventRepository.Object, _mockCache.Object, _settings);

            // Act
            Func<Task<EventResponseDto>> act = async () => await service.UpdateEventAsync(id, ev, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<EventValidationException>();
        }

        [Fact]
        public async Task CreateEvent_EventValidatorThrowsException()
        {
            // Arrange
            var newEvent = EventTestData.GetTestEventCreationDTO();
            var message = "Ошибка сервиса валидации";

            _mockValidator.Setup(v => v.Validate(It.IsAny<EventCreationDTO>()))
                .Throws(new EventValidationException(message));
            var service = new EventService(_mockValidator.Object, _mockEventRepository.Object, _mockCache.Object, _settings);

            // Act
            Func<Task<EventResponseDto>> act = async () => await service.CreateEventAsync(newEvent, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<EventValidationException>();
            _mockValidator.Verify(o => o.Validate(newEvent), Times.Once);
        }

        [Fact]
        public async Task GetEventById_CacheHit_ReadFromCache_ReturnsEvent()
        {
            // Arrange
            var ev = EventTestData.GetTestEvent();
            var id = ev.Id;
            var expectedResponse = ev.ToResponse();            
            _mockCache.Setup(o => o.GetAsync<EventResponseDto>(CacheKeys.EventKey(ev.Id))).ReturnsAsync(ev.ToResponse());
            var service = new EventService(_mockValidator.Object, _mockEventRepository.Object, _mockCache.Object, _settings);

            // Act
            var result = await service.GetEventByIdAsync(id, CancellationToken.None);

            // Assert
            _mockEventRepository.Verify(o => o.GetByIdAsync(id), Times.Never);
            _mockCache.Verify(o => o.GetAsync<EventResponseDto>(CacheKeys.EventKey(ev.Id)), Times.Once);
            _mockCache.Verify(o => o.SetAsync(CacheKeys.EventKey(ev.Id), It.IsAny<EventResponseDto>(), It.IsAny<TimeSpan>()), Times.Never);
            result.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task GetEventById_CacheMiss_ReadFromDB_ReturnsEvent()
        {
            // Arrange
            var ev = EventTestData.GetTestEvent();
            var id = ev.Id;
            var expectedResponse = ev.ToResponse();
            _mockCache.Setup(o => o.GetAsync<EventResponseDto>(CacheKeys.EventKey(ev.Id))).ReturnsAsync(() => null);
            _mockEventRepository.Setup(o => o.GetByIdAsync(id)).ReturnsAsync(ev);
            var service = new EventService(_mockValidator.Object, _mockEventRepository.Object, _mockCache.Object, _settings);

            // Act
            var result = await service.GetEventByIdAsync(id, CancellationToken.None);

            // Assert
            _mockEventRepository.Verify(o => o.GetByIdAsync(id), Times.Once);
            _mockCache.Verify(o => o.GetAsync<EventResponseDto>(CacheKeys.EventKey(ev.Id)), Times.Once);
            _mockCache.Verify(o => o.SetAsync(CacheKeys.EventKey(ev.Id), It.IsAny<EventResponseDto>(), It.IsAny<TimeSpan>()), Times.Once);
            result.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task UpdateEvent_DeletesCacheKey()
        {
            // Arrange
            var ev = EventTestData.GetTestEvent();            
            var id = ev.Id;
            var eventUpdateDTO = new EventUpdateDTO()
            {
                EndAt = DateTimeOffset.UtcNow,
                StartAt = DateTimeOffset.UtcNow,            
                Title = ""
            };
                        
            _mockEventRepository.Setup(o => o.GetByIdAsync(ev.Id)).ReturnsAsync(ev);
            _mockEventRepository.Setup(o => o.SaveChangesAsync());

            var service = new EventService(_mockValidator.Object, _mockEventRepository.Object, _mockCache.Object, _settings);

            // Act
            var result = await service.UpdateEventAsync(id, eventUpdateDTO, CancellationToken.None);

            // Assert
            _mockCache.Verify(o => o.DeleteAsync(CacheKeys.EventKey(id)), Times.Once);
        }

        [Fact]
        public async Task DeleteEvent_DeletesCacheKEy()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockEventRepository.Setup(r => r.DeleteAsync(id)).ReturnsAsync(true);
            var service = new EventService(_mockValidator.Object, _mockEventRepository.Object, _mockCache.Object, _settings);

            // Act
            await service.DeleteEventAsync(id, CancellationToken.None);

            // Assert
            _mockCache.Verify(o => o.DeleteAsync(CacheKeys.EventKey(id)), Times.Once);
        }

    }
}
