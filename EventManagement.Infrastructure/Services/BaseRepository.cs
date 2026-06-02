using EventManagement.Application.Interfaces.Reposirories;
using EventManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EventManagement.Infrastructure.Services;

/// <summary>
/// Обобщенный бвзовый репозиторий
/// </summary>
/// <typeparam name="T">Тип сущности</typeparam>
/// <param name="context">Контекст базы данных</param>
public class BaseRepository<T>(AppDbContext context) : IBaseRepository<T> where T : class
{
    /// <summary>
    /// Контекст базы данных
    /// </summary>
    protected readonly AppDbContext _context = context;

    /// <inheritdoc/>    
    public async Task<T> AddAsync(T entity, CancellationToken token = default)
    {
        await _context.Set<T>().AddAsync(entity, token);
        await _context.SaveChangesAsync(token);

        return entity;
    }    

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken token = default)
    {
        var entity = await _context.Set<T>().FindAsync(id, token);
        if (entity == null)
            return false;
        _context.Remove(entity);
        await _context.SaveChangesAsync(token);

        return true;
    }

    /// <inheritdoc/>
    public async Task<T?> GetByIdAsync(Guid id, CancellationToken token = default)
    {
        return await _context.Set<T>().FindAsync(id, token);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken token = default)
    {
        return await _context.Set<T>().ToListAsync(token);
    }

    /// <inheritdoc/>
    public async Task<int> GetCountAsync(CancellationToken token = default)
    {
        return await _context.Set<T>().CountAsync(token);
    }

    /// <inheritdoc/>
    public async Task SaveChangesAsync(CancellationToken token = default)
    {
        await _context.SaveChangesAsync(token);
    }
}
