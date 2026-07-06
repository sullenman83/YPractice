namespace Events.Application.Models.Messages;

/// <summary>
/// Тип для храенения outbox событий
/// </summary>
public record OutboxMessage
{
    /// <summary>
    /// Ключ
    /// </summary>
    public Guid Key { get; init; }

    /// <summary>
    /// тип сообщения
    /// </summary>
    public string MessageType { get; init; }

    /// <summary>
    /// когда создано
    /// </summary>
    public DateTimeOffset OccuredOn { get; init; }

    /// <summary>
    /// данные
    /// </summary>
    public string Payload { get; init; }

    /// <summary>
    /// количесство повторных отправлений
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Обработано или нет
    /// </summary>
    public bool Processed { get; set; }

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="key">ключ</param>
    /// <param name="messageType">тип сообщения</param>
    /// <param name="occuredOn">когда создано</param>
    /// <param name="payload">данные</param>
    /// <param name="retryCount">количесство повторных отправлений</param>
    /// <param name="processed">Обработано или нет</param>
    public OutboxMessage(Guid key, string messageType, DateTimeOffset occuredOn, string payload, int retryCount, bool processed)
    {
        Key = key; 
        MessageType = messageType; 
        OccuredOn = occuredOn; 
        Payload = payload; 
        RetryCount = retryCount;
        Processed = processed;
    }
};