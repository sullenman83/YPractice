
using Contracts;
using Events.Application.Common;
using Events.Application.Exceptions;
using Events.Application.Interfaces;
using Events.Application.Interfaces.Repositories;
using Events.Application.Interfaces.Validators;
using Events.Application.Models.Messages;
using Events.Application.Services.MessageHandlers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TestData;
using TransactionManager.Abstractions;

namespace Events.UnitTests;

public class BookingConfirmedHandlerTest
{
    private readonly Mock<IEventRepository> _mockEventRepository = new Mock<IEventRepository>();
    private readonly Mock<IInboxMessageRepository> _mockInboxRepository = new Mock<IInboxMessageRepository>();
    private readonly Mock<ITransactionService> _mockTransactionService = new Mock<ITransactionService>();
    private readonly Mock<ITransaction> _mockTransaction = new Mock<ITransaction>();
    private readonly Mock<ILogger<BookingConfirmedHandler>> _mockLogger = new Mock<ILogger<BookingConfirmedHandler>>();
    private readonly Mock<ICacheService> _mockCache = new Mock<ICacheService>();
    private readonly Mock<IBookingConfirmedValidator> _mockValidator = new Mock<IBookingConfirmedValidator>();

    public BookingConfirmedHandlerTest()
    {
        _mockTransactionService.Setup(o => o.BeginTransactionAsync()).ReturnsAsync(_mockTransaction.Object);
    }

    [Fact]
    public async Task HandleMessage_EventSeatsReserved()
    {
        // Arrange
        var totalSeats = 10;
        var seatsCount = 5;
        var ev = EventTestData.GetTestEvent(totalSeats);
        _mockEventRepository.Setup(o => o.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(ev);
        var message = new BookingConfirmed(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), seatsCount, DateTimeOffset.UtcNow);

        var service = new BookingConfirmedHandler(_mockEventRepository.Object, _mockInboxRepository.Object, _mockTransactionService.Object, 
            _mockValidator.Object, _mockLogger.Object, _mockCache.Object);

        // Act

        await service.HandleMessageAsync(message, CancellationToken.None);

        // Assert
        _mockEventRepository.Verify(o => o.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
        _mockTransactionService.Verify(o => o.BeginTransactionAsync(), Times.Once);
        _mockEventRepository.Verify(o => o.SaveChangesAsync(), Times.Once);
        _mockInboxRepository.Verify(o => o.AddAsync(It.IsAny<InboxMessage>()), Times.Once);
        _mockTransaction.Verify(o => o.CommitAsync(), Times.Once);
        ev.AvailableSeats.Should().Be(totalSeats - seatsCount);
    }

    [Fact]
    public async Task HandleMessage_IncorrectSeatsCount_ThrowsNoAvailableSeatsException()
    {
        // Arrange
        var totalSeats = 10;
        var seatsCount = 11;
        var ev = EventTestData.GetTestEvent(totalSeats);
        _mockEventRepository.Setup(o => o.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(ev);
        var message = new BookingConfirmed(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), seatsCount, DateTimeOffset.UtcNow);

        var service = new BookingConfirmedHandler(_mockEventRepository.Object, _mockInboxRepository.Object, _mockTransactionService.Object,
            _mockValidator.Object, _mockLogger.Object, _mockCache.Object);

        // Act

        await service.HandleMessageAsync(message, CancellationToken.None);

        // Assert
        _mockEventRepository.Verify(o => o.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
        _mockValidator.Verify(o => o.ValidateEventDate(It.IsAny<DateTimeOffset>()), Times.Once);
        _mockTransactionService.Verify(o => o.BeginTransactionAsync(), Times.Once);
        _mockEventRepository.Verify(o => o.SaveChangesAsync(), Times.Never);
        _mockInboxRepository.Verify(o => o.AddAsync(It.IsAny<InboxMessage>()), Times.Never);
        _mockTransaction.Verify(o => o.CommitAsync(), Times.Never);
        _mockCache.Verify(o => o.DeleteAsync(CacheKeys.EventKey(ev.Id)), Times.Never);
        ev.AvailableSeats.Should().Be(totalSeats);
    }

    [Fact]
    public async Task HandleMessage_DublicateInboxMessage_ThrowsDublicateInsertionException()
    {
        // Arrange
        var totalSeats = 10;
        var seatsCount = 5;
        var ev = EventTestData.GetTestEvent(totalSeats);
        _mockEventRepository.Setup(o => o.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(ev);
        var message = new BookingConfirmed(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), seatsCount, DateTimeOffset.UtcNow);
        _mockInboxRepository.Setup(o => o.AddAsync(It.IsAny<InboxMessage>())).Throws<DublicateInsertionException>();        

        var service = new BookingConfirmedHandler(_mockEventRepository.Object, _mockInboxRepository.Object, _mockTransactionService.Object,
            _mockValidator.Object, _mockLogger.Object, _mockCache.Object);

        // Act

        await service.HandleMessageAsync(message, CancellationToken.None);

        // Assert

        _mockEventRepository.Verify(o => o.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
        _mockValidator.Verify(o => o.ValidateEventDate(It.IsAny<DateTimeOffset>()), Times.Once);
        _mockTransactionService.Verify(o => o.BeginTransactionAsync(), Times.Once);
        _mockEventRepository.Verify(o => o.SaveChangesAsync(), Times.Once);
        _mockInboxRepository.Verify(o => o.AddAsync(It.IsAny<InboxMessage>()), Times.Once);
        _mockTransaction.Verify(o => o.CommitAsync(), Times.Never);
        _mockCache.Verify(o => o.DeleteAsync(CacheKeys.EventKey(ev.Id)), Times.Never);
        ev.AvailableSeats.Should().Be(totalSeats - seatsCount);
    }

    [Fact]
    public async Task HandleMessage_IncorrectEventId_ThrowsNotFoundException()
    {
        // Arrange
        var totalSeats = 10;
        var seatsCount = 5;
        var ev = EventTestData.GetTestEvent(totalSeats);
        _mockEventRepository.Setup(o => o.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Guid id, CancellationToken t) => null);
        var message = new BookingConfirmed(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), seatsCount, DateTimeOffset.UtcNow);
        var service = new BookingConfirmedHandler(_mockEventRepository.Object, _mockInboxRepository.Object, _mockTransactionService.Object,
            _mockValidator.Object, _mockLogger.Object, _mockCache.Object);

        // Act

        await service.HandleMessageAsync(message, CancellationToken.None);

        // Assert

        _mockEventRepository.Verify(o => o.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
        _mockValidator.Verify(o => o.ValidateEventDate(It.IsAny<DateTimeOffset>()), Times.Never);
        _mockTransactionService.Verify(o => o.BeginTransactionAsync(), Times.Never);
        _mockEventRepository.Verify(o => o.SaveChangesAsync(), Times.Never);
        _mockInboxRepository.Verify(o => o.AddAsync(It.IsAny<InboxMessage>()), Times.Never);
        _mockTransaction.Verify(o => o.CommitAsync(), Times.Never);
        _mockCache.Verify(o => o.DeleteAsync(CacheKeys.EventKey(ev.Id)), Times.Never);
        ev.AvailableSeats.Should().Be(totalSeats);
    }

    [Fact]
    public async Task HandleMessage_EventSeatsReserved_DeletesCacheKey()
    {
        // Arrange
        var totalSeats = 10;
        var seatsCount = 5;
        var ev = EventTestData.GetTestEvent(totalSeats);
        _mockEventRepository.Setup(o => o.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(ev);
        var message = new BookingConfirmed(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), seatsCount, DateTimeOffset.UtcNow);

        var service = new BookingConfirmedHandler(_mockEventRepository.Object, _mockInboxRepository.Object, _mockTransactionService.Object,
            _mockValidator.Object, _mockLogger.Object, _mockCache.Object);

        // Act

        await service.HandleMessageAsync(message, CancellationToken.None);

        // Assert        
        _mockTransaction.Verify(o => o.CommitAsync(), Times.Once);
        _mockCache.Verify(o => o.DeleteAsync(CacheKeys.EventKey(ev.Id)), Times.Once);
    }
}
