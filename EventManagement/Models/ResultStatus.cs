namespace EventManagement.Common.Results;

/// <summary>
/// Коды результатов
/// </summary>
public enum ResultStatusCode
{
    /// <summary>
    /// Операция успешна
    /// </summary>
    Ok,

    /// <summary>
    /// Ошибка объект не найден
    /// </summary>
    NotFound,

    /// <summary>
    /// Ошибка валидации
    /// </summary>
    ValidationError,


    /// <summary>
    /// Неизвестная ошибка
    /// </summary>
    InternalError

}