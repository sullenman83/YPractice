using Bookings.Application.Services.BookingServices;
using Bookings.Domain.Models;
using Moq;
using Bookings.Application.Interfaces.Repositories;
using DateTimeManager.Abstractions;
using Bookings.Application.Interfaces.BookingServices;
using TransactionManager.Abstractions;
using Bookings.Application.Interfaces;
using FluentAssertions;
using Bookings.Domain.Exceptions;
using Bookings.Application.Models;
using Bookings.Application.AppSettings;
using Microsoft.Extensions.Options;


namespace Bookings.UnitTests;

public class BookingServiceTest
{
    private readonly Mock<IBookingRepository> _mockBookingRepository = new Mock<IBookingRepository>();
    private readonly Mock<IOutboxMessageRepository> _mockOutboxRepository = new Mock<IOutboxMessageRepository>();
    private readonly Mock<ITransactionService> _mockTransactionService = new Mock<ITransactionService>();
    private readonly Mock<ITransaction> _mockTransaction = new Mock<ITransaction>();
    private readonly Mock<IDateTimeProvider> _mockDateTimeProvider = new Mock<IDateTimeProvider>();
    private readonly Mock<ICurrentUserService> _mockCurrentUserService = new Mock<ICurrentUserService>();
    private readonly Mock<IBookingValidator> _mockBookingValidator = new Mock<IBookingValidator>();    

    public BookingServiceTest()
    {
        _mockDateTimeProvider.Setup(o => o.GetUtcNow()).Returns(DateTimeOffset.UtcNow.Date);
        _mockTransactionService.Setup(o => o.BeginTransactionAsync()).ReturnsAsync(_mockTransaction.Object);
        _mockBookingValidator.Setup(o => o.ValidateActiveBooking(It.IsAny<IReadOnlyCollection<Booking>>()));
        _mockCurrentUserService.Setup(o => o.IsInRole(It.IsAny<string>())).Returns(true);
    }

    [Fact]
    public async Task CreateBooking_ByEventId_ReturnsBookingWithPendingStatus()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();        
        var seats = 5;        
        
        _mockBookingRepository.Setup(o => o.GetActiveUserBookingAsync(It.IsAny<Guid>())).ReturnsAsync(new List<Booking>());        
        var service = new BookingService(_mockBookingRepository.Object, _mockDateTimeProvider.Object, _mockBookingValidator.Object, _mockCurrentUserService.Object, 
            _mockTransactionService.Object, _mockOutboxRepository.Object);

        // Act
        var result = await service.CreateBookingAsync(eventId, userId, seats, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.EventId.Should().Be(eventId);
        result.UserId.Should().Be(userId);
        result.Status.Should().Be(BookingStatus.Pending);        
        _mockBookingRepository.Verify(o => o.AddAsync(It.IsAny<Booking>()), Times.Once);
        _mockBookingRepository.Verify(o => o.GetActiveUserBookingAsync(It.IsAny<Guid>()), Times.Once);        
        _mockBookingValidator.Verify(o => o.ValidateActiveBooking(It.IsAny<IReadOnlyCollection<Booking>>()), Times.Once);
    }

    [Fact]
    public async Task CreateSeveralBookings_ReturnsUniqueBookingId()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var bookingCount = 3;
        var ids = new List<Guid>();

        _mockBookingRepository.Setup(o => o.GetActiveUserBookingAsync(It.IsAny<Guid>())).ReturnsAsync(new List<Booking>());
        var service = new BookingService(_mockBookingRepository.Object, _mockDateTimeProvider.Object, _mockBookingValidator.Object, _mockCurrentUserService.Object,
            _mockTransactionService.Object, _mockOutboxRepository.Object);

        // Act
        for (int i = 0; i < bookingCount; ++i)
        {
            var result = await service.CreateBookingAsync(eventId, userId, 1, CancellationToken.None);
            ids.Add(result.Id);
        }

        // Assert
        ids.Should().HaveCount(bookingCount);
        ids.Should().OnlyHaveUniqueItems();
        _mockBookingRepository.Verify(o => o.AddAsync(It.IsAny<Booking>()), Times.Exactly(bookingCount));
        _mockBookingRepository.Verify(o => o.GetActiveUserBookingAsync(It.IsAny<Guid>()), Times.Exactly(bookingCount));
        _mockBookingValidator.Verify(o => o.ValidateActiveBooking(It.IsAny<IReadOnlyCollection<Booking>>()), Times.Exactly(bookingCount));
    }

    [Fact]
    public async Task GetBooking_ById_ReturnsBooking()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var booking = new Booking(BookingStatus.Pending, eventId, userId, 1, _mockDateTimeProvider.Object.GetUtcNow());
        var id = booking.Id;
        _mockBookingRepository.Setup(o => o.GetByIdAsync(id)).ReturnsAsync(booking);
        var service = new BookingService(_mockBookingRepository.Object, _mockDateTimeProvider.Object, _mockBookingValidator.Object, _mockCurrentUserService.Object,
            _mockTransactionService.Object, _mockOutboxRepository.Object);

        // Act
        var result = await service.GetBookingByIdAsync(booking.Id, CancellationToken.None);

        // Assert
        result.EventId.Should().Be(booking.EventId);
        result.Id.Should().Be(booking.Id);
        result.Status.Should().Be(booking.Status);
    }

    [Fact]
    public async Task GetBooking_ByInvalidBookingId_ThrowsNotFoundException()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        _mockBookingRepository.Setup(o => o.GetByIdAsync(bookingId)).Throws<NotFoundException>();
        var service = new BookingService(_mockBookingRepository.Object, _mockDateTimeProvider.Object, _mockBookingValidator.Object, _mockCurrentUserService.Object,
            _mockTransactionService.Object, _mockOutboxRepository.Object);

        // Act
        Func<Task> act = async () => await service.GetBookingByIdAsync(bookingId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }


    [Fact]
    public async Task CreateBooking_NegativeSeatsCount_ThrowsArgumentException()
    {
        // Arrange        
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var bookingCnt = -1;

        var options = new BookingSettings() { MaxActiveBookingCount = 10};
        var mockOptions = new Mock<IOptions<BookingSettings>>();
        mockOptions.Setup(o => o.Value).Returns(options);

        _mockBookingRepository.Setup(o => o.AddAsync(It.IsAny<Booking>())).ReturnsAsync((Booking b, CancellationToken token) => b);
        _mockBookingRepository.Setup(o => o.GetActiveUserBookingAsync(It.IsAny<Guid>())).ReturnsAsync(new List<Booking>());
        var bookingValidator = new BookingValidator(_mockDateTimeProvider.Object, mockOptions.Object);
        var service = new BookingService(_mockBookingRepository.Object, _mockDateTimeProvider.Object, bookingValidator, _mockCurrentUserService.Object,
            _mockTransactionService.Object, _mockOutboxRepository.Object);

        // Act
        Func<Task<BookingResponseDTO>> act = async () => await service.CreateBookingAsync(eventId, userId, bookingCnt, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }


    [Fact]
    public async Task CreateBooking_BookingCountLessMaxActiveBooking_ReturnsBooking()
    {
        //Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var seatsCount = 1;
        var bookingSettings = new BookingSettings()
        {
            MaxActiveBookingCount = 2
        };
        var options = Options.Create(bookingSettings);
        var bookingValidator = new BookingValidator(_mockDateTimeProvider.Object, options);

        var bookinglist = new List<Booking>()
        {
            new Booking(BookingStatus.Pending, eventId, userId, 1, DateTimeOffset.UtcNow),
        };
        
        _mockBookingRepository.Setup(o => o.GetActiveUserBookingAsync(It.IsAny<Guid>())).ReturnsAsync(bookinglist);
        _mockBookingRepository.Setup(o => o.AddAsync(It.IsAny<Booking>())).ReturnsAsync((Booking b, CancellationToken t) => b);
        var service = new BookingService(_mockBookingRepository.Object, _mockDateTimeProvider.Object, bookingValidator, _mockCurrentUserService.Object,
            _mockTransactionService.Object, _mockOutboxRepository.Object);

        //Act
        var result = await service.CreateBookingAsync(eventId, userId, seatsCount, CancellationToken.None);

        // Assert
        result.EventId.Should().Be(eventId);
        result.UserId.Should().Be(userId);
        result.Status.Should().Be(BookingStatus.Pending);
        _mockBookingRepository.Verify(o => o.AddAsync(It.IsAny<Booking>()), Times.Once);
        
    }

    [Fact]
    public async Task CreateBooking_BookingCountMoreMaxActiveBooking_ThrowsActiveBookingLimitException()
    {
        //Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var seatsCount = 1;
        var bookingSettings = new BookingSettings()
        {
            MaxActiveBookingCount = 1
        };
        var options = Options.Create(bookingSettings);
        var bookingValidator = new BookingValidator(_mockDateTimeProvider.Object, options);

        var bookinglist = new List<Booking>()
        {
            new Booking(BookingStatus.Pending, eventId, userId, 1, DateTimeOffset.UtcNow),
            new Booking(BookingStatus.Pending, eventId, userId, 1, DateTimeOffset.UtcNow),
        };
                
        _mockBookingRepository.Setup(o => o.GetActiveUserBookingAsync(It.IsAny<Guid>())).ReturnsAsync(bookinglist);
        _mockBookingRepository.Setup(o => o.AddAsync(It.IsAny<Booking>())).ReturnsAsync((Booking b, CancellationToken t) => b);
        var service = new BookingService(_mockBookingRepository.Object, _mockDateTimeProvider.Object, bookingValidator, _mockCurrentUserService.Object,
            _mockTransactionService.Object, _mockOutboxRepository.Object);

        //Act
        Func<Task<BookingResponseDTO>> act = async () => await service.CreateBookingAsync(eventId, userId, seatsCount, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ActiveBookingLimitException>();        
        _mockBookingRepository.Verify(o => o.AddAsync(It.IsAny<Booking>()), Times.Never);
    }


    [Fact]
    public async Task CancelBooking_OwnBooking_ChangesBookingStatus()
    {
        //Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var seatsCount = 1;
        var status = BookingStatus.Pending;
        
        var booking = new Booking(status, eventId, userId, seatsCount, DateTimeOffset.UtcNow);

        _mockBookingRepository.Setup(o => o.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(booking);
        _mockCurrentUserService.Setup(o => o.IsInRole(It.IsAny<string>())).Returns(false);

        var service = new BookingService(_mockBookingRepository.Object, _mockDateTimeProvider.Object, _mockBookingValidator.Object, _mockCurrentUserService.Object,
            _mockTransactionService.Object, _mockOutboxRepository.Object);

        //Act
        await service.CancelBookingAsync(booking.Id, userId);

        // Assert
        booking.Status.Should().Be(BookingStatus.Cancelled);
        _mockBookingRepository.Verify(o => o.GetByIdAsync(booking.Id), Times.Once);
        _mockCurrentUserService.Verify(o => o.IsInRole(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CancelBooking_NotOwnBooking_ThrowsNoRightsException()
    {
        //Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var userId1 = Guid.NewGuid();
        var seatsCount = 1;
        var status = BookingStatus.Pending;
        var booking = new Booking(status, eventId, userId, seatsCount, DateTimeOffset.UtcNow);        
        
        _mockBookingRepository.Setup(o => o.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(booking);
        _mockCurrentUserService.Setup(o => o.IsInRole(It.IsAny<string>())).Returns(false);

        var service = new BookingService(_mockBookingRepository.Object, _mockDateTimeProvider.Object, _mockBookingValidator.Object, _mockCurrentUserService.Object,
            _mockTransactionService.Object, _mockOutboxRepository.Object);

        //Act
        Func<Task> act = async () => await service.CancelBookingAsync(booking.Id, userId1);

        // Assert
        await act.Should().ThrowAsync<NoRightsException>();
        booking.Status.Should().Be(BookingStatus.Pending);
        _mockBookingRepository.Verify(o => o.GetByIdAsync(booking.Id), Times.Once);
        _mockCurrentUserService.Verify(o => o.IsInRole(It.IsAny<string>()), Times.Once);
        _mockBookingRepository.Verify(o => o.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CancelBooking_NotOwnBooking_AdminRole_ChangesBookingStatus()
    {
        //Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var userId1 = Guid.NewGuid();
        var seatsCount = 1;
        var status = BookingStatus.Pending;
        var booking = new Booking(status, eventId, userId, seatsCount, DateTimeOffset.UtcNow);

        _mockBookingRepository.Setup(o => o.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(booking);
        _mockCurrentUserService.Setup(o => o.IsInRole(It.IsAny<string>())).Returns(true);

        var service = new BookingService(_mockBookingRepository.Object, _mockDateTimeProvider.Object, _mockBookingValidator.Object, _mockCurrentUserService.Object,
            _mockTransactionService.Object, _mockOutboxRepository.Object);

        //Act
        await service.CancelBookingAsync(booking.Id, userId1);

        // Assert        
        booking.Status.Should().Be(BookingStatus.Cancelled);
        _mockBookingRepository.Verify(o => o.GetByIdAsync(booking.Id), Times.Once);
        _mockCurrentUserService.Verify(o => o.IsInRole(It.IsAny<string>()), Times.Once);
        _mockBookingRepository.Verify(o => o.SaveChangesAsync(), Times.Once);
    }

}