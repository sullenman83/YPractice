using EventManagement.Application.Interfaces.Security;
using System.Security.Cryptography;
using System.Text;

namespace EventManagement.Infrastructure.Services.Securiry;

/// <summary>
/// Класс для работы с хэшем паролей
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    ///<inheritdoc/>
    public string GenerateHash(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }

    ///<inheritdoc/>
    public bool VerifyPassword(string password, string hashedPassword)
    {
        var hash = GenerateHash(password);

        return string.Equals(hashedPassword, hash);
    }
}
