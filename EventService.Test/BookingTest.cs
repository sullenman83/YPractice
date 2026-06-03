using EventManagement.Application.Interfaces.Reposirories;
using EventManagement.Application.Models.BookingModels;
using EventManagement.Application.Services.BookingServices;
using EventManagement.Common;
using EventManagement.Domain.Exceptions;
using EventManagement.Domain.Models;
using EventManagement.Domain.Services;
using FluentAssertions;
using Moq;

namespace EventServiceTest;

public class BookingTest
{
    private readonly Mock<IBookingRepository<Booking>> _bookingRepository;
    private readonly Mock<IEventRepository<Event>> _eventRepository;
    private readonly DateTimeProvider _dateTimeProvider;
    //private readonly Mock<ResiliencePipelineProvider<string>> _pipelineProvider;

    //private readonly Mock<AppDbContext> _context;    
    //private readonly Mock<IDbContextTransaction> _tr;
    //private readonly Mock<DatabaseFacade> _facade;

    public BookingTest()
    {
        _bookingRepository = new Mock<IBookingRepository<Booking>>();
        _eventRepository = new Mock<IEventRepository<Event>>();
        _dateTimeProvider = new DateTimeProvider();
        //_pipelineProvider = new Mock<ResiliencePipelineProvider<string>>();
        //_pipelineProvider.Setup(p => p.GetPipeline(Consts.CreateBookingRetry))
        //    .Returns(ResiliencePipeline.Empty);

        //var options = new DbContextOptionsBuilder<AppDbContext>()
        //    .Options;

        //_tr = new Mock<IDbContextTransaction>();
        //_context = new Mock<AppDbContext>(options);
        //_facade = new Mock<DatabaseFacade>(_context.Object);
        //_context.Setup(c => c.Database).Returns(_facade.Object);
        //_context.Setup(o => o.Database.BeginTransactionAsync())
        //    .ReturnsAsync(_tr.Object);
        //_eventRepository.Setup(o => o.Context).Returns(_context.Object);
        //_tr.Setup(o => o.CommitAsync());
        //_tr.Setup(o => o.RollbackAsync());

    }

    [Fact]
    public async Task CreateBooking_ByEventId_ReturnsBookingWithPendingStatus()
    {
        // Arrange        
        var totalSeats = 10;
        var ev = TestData.GetTestEvent(totalSeats);
        var id = ev.Id;
        var seats = 5;

        _eventRepository.Setup(o => o.GetByIdAsync(id)).ReturnsAsync(ev);
        _eventRepository.Setup(o => o.SaveChangesAsync());
        _bookingRepository.Setup(o => o.AddAsync(It.IsAny<Booking>())).ReturnsAsync((Booking b, CancellationToken t) => b);
        var service = new BookingService(_bookingRepository.Object, _eventRepository.Object, _dateTimeProvider);

        // Act
        var result = await service.CreateBookingAsync(id, seats, CancellationToken.None);

        // Assert
        result.EventId.Should().Be(id);
        result.Status.Should().Be(BookingStatus.Pending);
        _eventRepository.Verify(o => o.SaveChangesAsync(), Times.Once);
        _bookingRepository.Verify(o => o.AddAsync(It.IsAny<Booking>()), Times.Once);
        _eventRepository.Verify(o => o.GetByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task CreateSeveralBookings_ByEventID_ReturnsUniqueBookingId()
    {
        // Arrange
        var ev = TestData.GetTestEvent();
        var id = ev.Id;
        var ids = new List<Guid>();
        var bookingCount = 3;

        _eventRepository.Setup(o => o.GetByIdAsync(id)).ReturnsAsync(ev);
        _eventRepository.Setup(o => o.SaveChangesAsync());
        _bookingRepository.Setup(o => o.AddAsync(It.IsAny<Booking>())).ReturnsAsync((Booking b, CancellationToken t) => b);
        var service = new BookingService(_bookingRepository.Object, _eventRepository.Object, _dateTimeProvider);

        // Act
        for (int i = 0; i < bookingCount; ++i)
        {
            var result = await service.CreateBookingAsync(id, 1, CancellationToken.None);
            ids.Add(result.Id);
        }

        // Assert
        ids.Should().HaveCount(bookingCount);
        ids.Should().OnlyHaveUniqueItems();
        _eventRepository.Verify(o => o.GetByIdAsync(id), Times.Exactly(bookingCount));
        _eventRepository.Verify(o => o.SaveChangesAsync(), Times.Exactly(bookingCount));
        _bookingRepository.Verify(o => o.AddAsync(It.IsAny<Booking>()), Times.Exactly(bookingCount));
    }

    [Fact]
    public async Task GetBooking_ById_ReturnsBookig()
    {
        // Arrange
        var eventID = Guid.NewGuid();
        var booking = new Booking(BookingStatus.Pending, eventID, 1, _dateTimeProvider.GetUtcNow());
        var id = booking.Id;
        _bookingRepository.Setup(o => o.GetByIdAsync(id)).ReturnsAsync(booking);
        var service = new BookingService(_bookingRepository.Object, _eventRepository.Object, _dateTimeProvider);

        // Act
        var result = await service.GetBookingByIdAsync(booking.Id, CancellationToken.None);

        // Assert
        result.EventId.Should().Be(booking.EventId);
        result.Id.Should().Be(booking.Id);
        result.Status.Should().Be(booking.Status);
    }

    [Fact]
    public async Task GetBooking_ChangeStatus_ReturnsChangedStatus()
    {
        // Arrange        
        var eventID = Guid.NewGuid();
        var booking = new Booking(BookingStatus.Pending, eventID, 1, _dateTimeProvider.GetUtcNow());
        var id = booking.Id;
        _bookingRepository.Setup(o => o.GetByIdAsync(id)).ReturnsAsync(booking);
        var service = new BookingService(_bookingRepository.Object, _eventRepository.Object, _dateTimeProvider);


        // Act
        var result = await service.GetBookingByIdAsync(id, CancellationToken.None);
        booking.Reject(_dateTimeProvider);
        var result1 = await service.GetBookingByIdAsync(id, CancellationToken.None);

        // Assert
        result1.Status.Should().NotBe(BookingStatus.Pending);
    }

    [Fact]
    public async Task CreateBooking_ByDeletedEventId_ThrowsNotFoundException()
    {
        // Arrange
        var ev = TestData.GetTestEvent();
        var eventId = ev.Id;
        _eventRepository.Setup(o => o.GetByIdAsync(eventId)).ReturnsAsync(ev);
        _eventRepository.Setup(o => o.SaveChangesAsync());
        _bookingRepository.Setup(o => o.AddAsync(It.IsAny<Booking>())).ReturnsAsync((Booking b, CancellationToken t) => b);
        var service = new BookingService(_bookingRepository.Object, _eventRepository.Object, _dateTimeProvider);

        // Act
        var result = await service.CreateBookingAsync(eventId, 2, CancellationToken.None);
        _eventRepository.Setup(o => o.GetByIdAsync(It.IsAny<Guid>())).Throws<NotFoundException>();
        Func<Task> act = async () => await service.CreateBookingAsync(eventId, 2, CancellationToken.None);

        // Assert
        result.EventId.Should().Be(eventId);
        await act.Should().ThrowAsync<NotFoundException>();
        _eventRepository.Verify(o => o.GetByIdAsync(eventId), Times.Exactly(2));
        _eventRepository.Verify(o => o.SaveChangesAsync(), Times.Exactly(1));
        _bookingRepository.Verify(o => o.AddAsync(It.IsAny<Booking>()), Times.Exactly(1));
    }

    [Fact]
    public async Task GetBooking_ByInvalidBookingId_ThrowsNotFoundException()
    {
        // Arrange        
        var bookingId = Guid.NewGuid();
        _bookingRepository.Setup(o => o.GetByIdAsync(It.IsAny<Guid>())).Throws<NotFoundException>();
        var service = new BookingService(_bookingRepository.Object, _eventRepository.Object, _dateTimeProvider);

        // Act
        Func<Task> act = async () => await service.GetBookingByIdAsync(bookingId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateSeveralBooking_ReturnsBookingWhitUniqueId()
    {
        // Arrange        
        var ev = TestData.GetTestEvent(3);
        var id = ev.Id;
        var seats = 1;
        var ids = new List<Guid>();
        var cnt = 3;

        _eventRepository.Setup(o => o.GetByIdAsync(id)).ReturnsAsync(ev);
        _eventRepository.Setup(o => o.SaveChangesAsync());
        _bookingRepository.Setup(o => o.AddAsync(It.IsAny<Booking>())).ReturnsAsync((Booking b, CancellationToken t) => b);
        var service = new BookingService(_bookingRepository.Object, _eventRepository.Object, _dateTimeProvider);

        // Act
        for (int i = 0; i < cnt; ++i)
        {
            var result = await service.CreateBookingAsync(id, seats, CancellationToken.None);
            ids.Add(result.Id);
        }

        // Assert
        ids.Should().HaveCount(cnt);
        ids.Should().OnlyHaveUniqueItems();
        _eventRepository.Verify(o => o.GetByIdAsync(id), Times.Exactly(cnt));
        _eventRepository.Verify(o => o.SaveChangesAsync(), Times.Exactly(cnt));
        _bookingRepository.Verify(o => o.AddAsync(It.IsAny<Booking>()), Times.Exactly(cnt));
    }

    [Fact]
    public async Task CreateBooking_NoAvailableSeats_ThrowsNoAvailableSeatsException()
    {
        //Arrange
        var ev = TestData.GetTestEvent(1);
        var id = ev.Id;
        var seatsCount = 2;

        _eventRepository.Setup(o => o.GetByIdAsync(id)).ReturnsAsync(ev);
        _eventRepository.Setup(o => o.SaveChangesAsync());
        _bookingRepository.Setup(o => o.AddAsync(It.IsAny<Booking>())).ReturnsAsync((Booking b, CancellationToken t) => b);
        var service = new BookingService(_bookingRepository.Object, _eventRepository.Object, _dateTimeProvider);

        //Act
        Func<Task<BookingResponseDTO>> act = async () => await service.CreateBookingAsync(id, seatsCount, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NoAvailableSeatsException>();
        _eventRepository.Verify(o => o.GetByIdAsync(id), Times.Once);
        _bookingRepository.Verify(o => o.AddAsync(It.IsAny<Booking>()), Times.Never);
        _eventRepository.Verify(o => o.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task ChangeBookingStatus_SetConfirm_ReturnChangedBooking()
    {
        // Arrange                
        var eventID = Guid.NewGuid();
        var booking = new Booking(BookingStatus.Pending, eventID, 1, _dateTimeProvider.GetUtcNow());
        var id = booking.Id;
        _bookingRepository.Setup(o => o.GetByIdAsync(id)).ReturnsAsync(booking);
        var service = new BookingService(_bookingRepository.Object, _eventRepository.Object, _dateTimeProvider);

        // Act
        var result = await service.GetBookingByIdAsync(id, CancellationToken.None);
        booking.Confirm(_dateTimeProvider);
        var result1 = await service.GetBookingByIdAsync(id, CancellationToken.None);

        // Assert
        result.Status.Should().Be(BookingStatus.Pending);
        result1.Status.Should().Be(BookingStatus.Confirmed);
        result1.ProcessedAt.Should().NotBeNull();
        _bookingRepository.Verify(o => o.GetByIdAsync(id), Times.Exactly(2));
    }

    [Fact]
    public async Task ChangeBookingStatus_SetReject_ReturnChangedBooking()
    {
        // Arrange                
        var eventID = Guid.NewGuid();
        var booking = new Booking(BookingStatus.Pending, eventID, 1, _dateTimeProvider.GetUtcNow());
        var id = booking.Id;
        _bookingRepository.Setup(o => o.GetByIdAsync(id)).ReturnsAsync(booking);
        var service = new BookingService(_bookingRepository.Object, _eventRepository.Object, _dateTimeProvider);


        // Act
        var result = await service.GetBookingByIdAsync(id, CancellationToken.None);
        booking.Reject(_dateTimeProvider);
        var result1 = await service.GetBookingByIdAsync(id, CancellationToken.None);

        // Assert
        result.Status.Should().Be(BookingStatus.Pending);
        result1.Status.Should().Be(BookingStatus.Rejected);
        result1.ProcessedAt.Should().NotBeNull();
        _bookingRepository.Verify(o => o.GetByIdAsync(id), Times.Exactly(2));
    }

    [Fact]
    public async Task ReleaseSeatsAfterReject_ReturnRightAvailableSeats()
    {
        // Arrange        
        int cnt = 5;
        var ev = TestData.GetTestEvent(10);
        var booking = new Booking(BookingStatus.Confirmed, Guid.NewGuid(), cnt, _dateTimeProvider.GetUtcNow());
        ev.TryReserveSeats(cnt);
        var availableSeats = ev.AvailableSeats;

        // Act        
        booking.Reject(_dateTimeProvider);
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
        var ev = TestData.GetTestEvent(10);
        var booking = new Booking(BookingStatus.Confirmed, Guid.NewGuid(), cnt, _dateTimeProvider.GetUtcNow());
        ev.TryReserveSeats(cnt);
        var availableSeats = ev.AvailableSeats;

        // Act        
        booking.Reject(_dateTimeProvider);
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
    public async Task CreateBooking_NegativeSeatsCount_ThrowsArgumentException()
    {
        // Arrange
        int totalSeats = 10;
        var ev = TestData.GetTestEvent(totalSeats);
        var id = ev.Id;
        var bookingCnt = -1;

        _eventRepository.Setup(o => o.GetByIdAsync(id)).ReturnsAsync(ev);
        _eventRepository.Setup(o => o.SaveChangesAsync());
        _bookingRepository.Setup(o => o.AddAsync(It.IsAny<Booking>())).ReturnsAsync((Booking b, CancellationToken t) => b);
        var service = new BookingService(_bookingRepository.Object, _eventRepository.Object, _dateTimeProvider);

        // Act
        Func<Task<BookingResponseDTO>> act = async () => await service.CreateBookingAsync(id, bookingCnt, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
        _eventRepository.Verify(o => o.GetByIdAsync(id), Times.Never);
        _bookingRepository.Verify(o => o.AddAsync(It.IsAny<Booking>()), Times.Never);
        _eventRepository.Verify(o => o.SaveChangesAsync(), Times.Never);
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
}