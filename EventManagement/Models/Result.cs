using EventManagement.Common.Results;

namespace EventManagement.Models;

/// <summary>
/// Клас результата для сервисов
/// </summary>
public class Result<T> where T: class
{
    /// <summary>
    /// Объект результата
    /// </summary>
    public T? Value { get; set; }

    /// <summary>
    /// Флаг успешна операция или нет
    /// </summary>
    public bool IsSuccess { get; set; } = true;

    /// <summary>
    /// Статус операции
    /// </summary>
    public ResultStatusCode StatusCode { get; set; } = ResultStatusCode.Ok;

    /// <summary>
    /// Дополнительная информация или сообщение об ошибкее
    /// </summary>
    public string? Message { get; set; } = string.Empty;

}
