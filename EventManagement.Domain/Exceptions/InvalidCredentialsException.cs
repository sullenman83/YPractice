namespace EventManagement.Domain.Exceptions;

/// <summary>
/// Класс исключение для ошибок ввода логина/пароля
/// </summary>
public class InvalidCredentialsException: Exception
{
    /// <summary>
    /// Конструктор
    /// </summary>
    public InvalidCredentialsException() : base() { }

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    public InvalidCredentialsException(string message) : base(message) { }

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    /// <param name="inner">Обеъкт исключения</param>
    public InvalidCredentialsException(string message, Exception inner) : base(message, inner) { }
}
