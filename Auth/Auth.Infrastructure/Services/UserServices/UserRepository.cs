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
            var message = "Ошибка добавления элемента в БД";
            _logger.LogDebug(message, ex);
            throw new DbOperationException(message);
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
            var message = $"Ошибка получения записи по логину = {login}";
            _logger.LogDebug(message, ex);
            throw new DbOperationException(message);
        }
    }    
}
