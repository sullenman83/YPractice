using System.Net;

namespace EventManagement.Common.Results;

/// <summary>
/// Базовый класс ответа из API
/// </summary>
public class EventApiBaseResult
{
    /// <summary>
    /// Флаг показывает был ли вызов успешен (true - успех, false - ошибки)
    /// </summary>
    public required bool Success { get; set; }

    /// <summary>
    /// Статус-код отввета
    /// </summary>
    public required HttpStatusCode StatusCode { get; set; }

    /// <summary>
    /// Время ответа
    /// </summary>
    public required DateTime DateTime { get; set; }

    /// <summary>
    /// Дополнительнвая информация или сообщение об ошибке
    /// </summary>
    public required string Message { get; set; }
}

/// <summary>
/// Класс ответа без данных
/// </summary>
public class EventApiResult: EventApiBaseResult
{
}

/// <summary>
/// Класс ответа с данными
/// </summary>
/// <typeparam name="T"></typeparam>
public class EventApiResult<T>: EventApiBaseResult
{
    /// <summary>
    /// ДАнныые ответа (null если ответ с ошибкой)
    /// </summary>
    public T? Data { get; set; }
}
