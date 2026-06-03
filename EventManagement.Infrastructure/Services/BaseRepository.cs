using EventManagement.Application.Common.Exceptions;
using EventManagement.Application.Interfaces.Reposirories;
using EventManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EventManagement.Infrastructure.Services;

/// <summary>
/// Обобщенный бвзовый репозиторий
/// </summary>
/// <typeparam name="T">Тип сущности</typeparam>
/// <param name="context">Контекст базы данных</param>
public class BaseRepository<T>(AppDbContext context, ILogger<BaseRepository<T>> logger) : IBaseRepository<T> where T : class
{
    /// <summary>
    /// Контекст базы данных
    /// </summary>
    protected readonly AppDbContext _context = context;

    protected readonly ILogger<BaseRepository<T>> _logger = logger;

    /// <inheritdoc/>    
    public async Task<T> AddAsync(T entity, CancellationToken token = default)
    {
        try
        {
            await _context.Set<T>().AddAsync(entity, token);
            await _context.SaveChangesAsync(token);

            return entity;
        }
        catch (Exception ex)
        {
            var message = "Ошибка добавления элемента в БД";
            _logger.LogDebug(message, ex);
            throw new DbOperationException(message);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken token = default)
    {
        try
        {
            var entity = await _context.Set<T>().FindAsync(id, token);
            if (entity == null)
                return false;
            _context.Remove(entity);
            await _context.SaveChangesAsync(token);

            return true;
        }
        catch (Exception ex)
        {
            var message = $"Ошибка добавления элемента {id} в БД";
            _logger.LogDebug(message, ex);
            throw new DbOperationException(message);
        }
    }

    /// <inheritdoc/>
    public async Task<T?> GetByIdAsync(Guid id, CancellationToken token = default)
    {
        try
        {
            return await _context.Set<T>().FindAsync(id, token);
        }
        catch (Exception ex)
        {
            var message = $"Ошибка получения записи по Id = {id}";
            _logger.LogDebug(message, ex);
            throw new DbOperationException(message);
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken token = default)
    {
        try
        {
            return await _context.Set<T>().ToListAsync(token);
        }
        catch (Exception ex)
        {
            var message = "Ошибка чтения записей.";
            _logger.LogDebug(message, ex);
            throw new DbOperationException(message);
        }
    }

    /// <inheritdoc/>
    public async Task<int> GetCountAsync(CancellationToken token = default)
    {
        try
        {
            return await _context.Set<T>().CountAsync(token);
        }
        catch(Exception ex)
        {
            var message = "Ошибка получения количества записей.";
            _logger.LogDebug(message, ex);
            throw new DbOperationException(message);
        }
    }

    /// <inheritdoc/>
    public async Task SaveChangesAsync(CancellationToken token = default)
    {
        try
        {
            await _context.SaveChangesAsync(token);
        }
        catch (Exception ex)
        {
            var message = "Ошибка сохранения.";
            _logger.LogDebug(message, ex);
            throw new DbOperationException(message);
        }
    }
}
