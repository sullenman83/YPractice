using EventManagement.Application.Common;
using EventManagement.Application.Common.AppSettings;
using EventManagement.Application.Interfaces;
using EventManagement.Application.Interfaces.Repositories;
using EventManagement.Application.Interfaces.Services;
using EventManagement.Application.Interfaces.Services.BookingServices;
using EventManagement.Application.Models.BookingModels;
using EventManagement.Application.Services.BookingServices;
using EventManagement.Common;
using EventManagement.Domain.Exceptions;
using EventManagement.Domain.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Polly;
using Polly.Registry;

namespace EventServiceTest;

public class BookingTest
{
    private readonly Mock<IBookingRepository<Booking>> _bookingRepository = new Mock<IBookingRepository<Booking>>();
    private readonly Mock<IEventRepository<Event>> _eventRepository = new Mock<IEventRepository<Event>>();
    private readonly Mock<ITransactionService> _mockTransactionService = new Mock<ITransactionService>();
    private readonly Mock<ITransaction> _mockTransaction = new Mock<ITransaction>();
    private readonly Mock<IDateTimeProvider> _dateTimeProvider = new Mock<IDateTimeProvider>();
    private readonly Mock<ResiliencePipelineProvider<string>> _pipelineProvider = new Mock<ResiliencePipelineProvider<string>>();
    private readonly Mock<IBookingValidator> _mockBookingValidator = new Mock<IBookingValidator>();
    private readonly Mock<ICurrentUserService> _mockCurrentUserService = new Mock<ICurrentUserService>();

    public BookingTest()
    {
        _dateTimeProvider.Setup(o => o.GetUtcNow()).Returns(DateTimeOffset.UtcNow.Date);
        _pipelineProvider.Setup(p => p.GetPipeline(Consts.BookingServiceRepeater))
            .Returns(ResiliencePipeline.Empty);
        _mockTransactionService.Setup(o => o.BeginTransactionAsync()).ReturnsAsync(_mockTransaction.Object);
        _mockBookingValidator.Setup(o => o.ValidateActiveBooking(It.IsAny<IReadOnlyCollection<Booking>>()));
        _mockBookingValidator.Setup(o => o.ValidateEventDate(It.IsAny<DateTimeOffset>()));
        _mockCurrentUserService.Setup(o => o.IsInRole(It.IsAny<string>())).Returns(true);
    }


    [Fact]
    public async Task CreateBooking_NoAvailableSeats_ThrowsNoAvailableSeatsException()
    {
        //Arrange
        var ev = TestData.GetTestEvent(1);
        var user = TestData.GetTestUser();
        var id = ev.Id;
        var seatsCount = 2;

        _eventRepository.Setup(o => o.GetByIdAsync(id)).ReturnsAsync(ev);
        _eventRepository.Setup(o => o.GetEventWithBlockingAsync(id)).ReturnsAsync(ev);
        _eventRepository.Setup(o => o.SaveChangesAsync());
        _bookingRepository.Setup(o => o.GetActiveUserBookingAsync(It.IsAny<Guid>())).ReturnsAsync(new List<Booking>());
        _bookingRepository.Setup(o => o.AddAsync(It.IsAny<Booking>())).ReturnsAsync((Booking b, CancellationToken t) => b);
        var service = new BookingService(_bookingRepository.Object, _eventRepository.Object, _mockTransactionService.Object, _dateTimeProvider.Object,
            _mockBookingValidator.Object, _mockCurrentUserService.Object, _pipelineProvider.Object);

        //Act
        Func<Task<BookingResponseDTO>> act = async () => await service.CreateBookingAsync(id, user.Id, seatsCount, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NoAvailableSeatsException>();
        _eventRepository.Verify(o => o.GetEventWithBlockingAsync(id), Times.Once);
        _bookingRepository.Verify(o => o.AddAsync(It.IsAny<Booking>()), Times.Never);
        _eventRepository.Verify(o => o.SaveChangesAsync(), Times.Never);
    }

   

    [Fact]
    public async Task ReleaseSeatsAfterReject_ReturnRightAvailableSeats()
    {
        // Arrange        
        int cnt = 5;
        var ev = TestData.GetTestEvent(10);
        var user = TestData.GetTestUser();
        var booking = new Booking(BookingStatus.Confirmed, Guid.NewGuid(), user.Id, cnt, _dateTimeProvider.Object.GetUtcNow());
        ev.TryReserveSeats(cnt);
        var availableSeats = ev.AvailableSeats;

        // Act        
        booking.Reject(_dateTimeProvider.Object.GetUtcNow());
        ev.ReleaseSeats(cnt);

        // Assert
        booking.Status.Should().Be(BookingStatus.Rejected);
        availableSeats.Should().Be(ev.TotalSeats - cnt);
        ev.AvailableSeats.Should().Be(ev.TotalSeats);
    }

    [Fact]
    public async Task BookingSeatsAfterRelease()
    {
        // Arrange        
        int cnt = 5;
        var user = TestData.GetTestUser();
        var ev = TestData.GetTestEvent(10);
        var booking = new Booking(BookingStatus.Confirmed, Guid.NewGuid(), user.Id, cnt, _dateTimeProvider.Object.GetUtcNow());
        ev.TryReserveSeats(cnt);
        var availableSeats = ev.AvailableSeats;

        // Act        
        booking.Reject(_dateTimeProvider.Object.GetUtcNow());
        ev.ReleaseSeats(cnt);
        var availableSeats1 = ev.AvailableSeats;
        ev.TryReserveSeats(cnt);

        // Assert
        booking.Status.Should().Be(BookingStatus.Rejected);
        availableSeats.Should().Be(ev.TotalSeats - cnt);
        availableSeats1.Should().Be(ev.TotalSeats);
        ev.AvailableSeats.Should().Be(ev.TotalSeats - cnt);
    }

   

    [Fact]
    public async Task ReleaseSeats_MoreThenTotalSeats_ThrowsInvalidOperationException()
    {
        // Arrange
        int totalSeats = 10;
        var ev = TestData.GetTestEvent(totalSeats);
        var seatsCount = 1;

        // Act
        ev.TryReserveSeats(seatsCount);
        var result1 = ev.ReleaseSeats(seatsCount);
        var result2 = ev.ReleaseSeats(seatsCount);

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeFalse();
    }

    [Fact]
    public async Task CreateBooking_StartDateLessCurrentDate_ThrowsPastEventBookingException()
    {
        //Arrange
        var ev = TestData.GetTestEvent();
        var user = TestData.GetTestUser();
        var id = ev.Id;
        ev.StartAt = ev.StartAt.AddDays(-1);
        var seatsCount = 1;
        var bookingSettings = new BookingSettings()
        {
            MaxActiveBookingCount = 1
        };
        var options = Options.Create(bookingSettings);
        var bookingValidator = new BookingValidator(_dateTimeProvider.Object, options);

        _eventRepository.Setup(o => o.GetByIdAsync(id)).ReturnsAsync(ev);
        _bookingRepository.Setup(o => o.GetActiveUserBookingAsync(It.IsAny<Guid>())).ReturnsAsync(new List<Booking>());
        var service = new BookingService(_bookingRepository.Object, _eventRepository.Object, _mockTransactionService.Object, _dateTimeProvider.Object,
            bookingValidator, _mockCurrentUserService.Object, _pipelineProvider.Object);

        //Act
        Func<Task<BookingResponseDTO>> act = async () => await service.CreateBookingAsync(id, user.Id, seatsCount, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<PastEventBookingException>();
        _eventRepository.Verify(o => o.GetByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task CreateBooking_CurrentDateLessStartDate_ReturnsBooking()
    {
        //Arrange
        var ev = TestData.GetTestEvent();
        var user = TestData.GetTestUser();
        var id = ev.Id;
        ev.StartAt = DateTimeOffset.UtcNow.AddDays(2);
        var seatsCount = 1;
        var bookingSettings = new BookingSettings()
        {
            MaxActiveBookingCount = 1
        };
        var options = Options.Create(bookingSettings);
        var bookingValidator = new BookingValidator(_dateTimeProvider.Object, options);

        _eventRepository.Setup(o => o.GetByIdAsync(id)).ReturnsAsync(ev);
        _eventRepository.Setup(o => o.GetEventWithBlockingAsync(id)).ReturnsAsync(ev);
        _eventRepository.Setup(o => o.SaveChangesAsync());
        _bookingRepository.Setup(o => o.GetActiveUserBookingAsync(It.IsAny<Guid>())).ReturnsAsync(new List<Booking>());
        _bookingRepository.Setup(o => o.AddAsync(It.IsAny<Booking>())).ReturnsAsync((Booking b, CancellationToken t) => b);
        var service = new BookingService(_bookingRepository.Object, _eventRepository.Object, _mockTransactionService.Object, _dateTimeProvider.Object,
            bookingValidator, _mockCurrentUserService.Object, _pipelineProvider.Object);

        //Act
        var result = await service.CreateBookingAsync(id, user.Id, seatsCount, CancellationToken.None);

        // Assert
        result.EventId.Should().Be(id);
        result.Status.Should().Be(BookingStatus.Pending);
        _eventRepository.Verify(o => o.GetByIdAsync(id), Times.Once);
        _eventRepository.Verify(o => o.SaveChangesAsync(), Times.Once);
        _bookingRepository.Verify(o => o.AddAsync(It.IsAny<Booking>()), Times.Once);
        _eventRepository.Verify(o => o.GetEventWithBlockingAsync(id), Times.Once);
    }



}