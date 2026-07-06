namespace Events.Application.Interfaces.Consumers;

public interface IBookingConfirmedConsumer: IDisposable
{
    void Consume();
}
