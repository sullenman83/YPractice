namespace EventManagement.Application.Interfaces.Security;

/// <summary>
/// Интерфейс для работы с хэшем паролей
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Сгенерировать хэш
    /// </summary>
    /// <param name="password">Пароль</param>
    /// <returns>Хэш</returns>
    string GenerateHash(string password);

    /// <summary>
    /// Проверить пароль
    /// </summary>
    /// <param name="password">Пароль</param>
    /// <param name="hashedPassword">Хэш пароля</param>
    /// <returns>bool - совпадение, false - нет совпадения</returns>
    bool VerifyPassword(string password, string hashedPassword);
}
