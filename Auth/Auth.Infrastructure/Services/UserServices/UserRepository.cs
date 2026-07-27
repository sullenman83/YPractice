using Auth.Application.Exceptions;
using Auth.Application.Interfaces.Repositories;
using Auth.Domain.Models;
using Auth.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Auth.Infrastructure.Services.UserServices;


/// <summary>
/// Репозиторий для пользователей
/// </summary>
/// <param name="context">Контекст БД</param>
/// <param name="logger">Логгер</param>
public class UserRepository(AppDbContext context, ILogger<UserRepository> logger): IUserRepository
{
    private readonly AppDbContext _context = context;
    private readonly ILogger<UserRepository> _logger = logger;

    ///<inheritdoc/>
    ///<exception cref="DbOperationException">Ошибка при работе с базой данных</exception>
    public async Task<User> AddUserAsync(User user, CancellationToken token = default)
    {
        try
        {
            await _context.Users.AddAsync(user, token);
            await _context.SaveChangesAsync(token);

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Ошибка добавления пользователя {userId} в БД", user.Id);
            throw new DbOperationException("Ошибка добавления элемента в БД");
        }
    }

    ///<inheritdoc/>
    ///<exception cref="DbOperationException">Ошибка при работе с базой данных</exception>
    public async Task<User?> GetUserByLoginAsync(string login, CancellationToken token= default)
    {
        try
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Login == login, token);
        }
        catch (Exception ex)
        {
            var message = "Ошибка получения записи по логину";
            _logger.LogWarning(ex, message);
            throw new DbOperationException(message);
        }
    }
}
