using EventManagement.Data;
using EventManagement.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EventManagement.Services;

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

    /// <summary>
    /// Коллекция сущностей
    /// </summary>
    protected readonly DbSet<T> _entities = context.Set<T>();

    /// <inheritdoc/>    
    public async Task<T> AddAsync(T ev, CancellationToken token = default)
    {
        await _entities.AddAsync(ev, token);
        await _context.SaveChangesAsync(token);

        return ev;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken token = default)
    {
        var ev = await _entities.FindAsync(id, token);
        if (ev == null)
            return false;
        _context.Remove(ev);
        await _context.SaveChangesAsync(token);

        return true;
    }

    /// <inheritdoc/>
    public async Task<T?> GetByIdAsync(Guid id, CancellationToken token = default)
    {
        return await _entities.FindAsync(id, token);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken token = default)
    {
        return await _entities.ToListAsync(token);
    }

    /// <inheritdoc/>
    public async Task<int> GetCountAsync(CancellationToken token = default)
    {
        return await _entities.CountAsync(token);
    }

    /// <inheritdoc/>
    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken token = default)
    {
        return _context.Database.BeginTransactionAsync(token);
    }

    /// <inheritdoc/>
    public async Task SaveChangesAsync(CancellationToken token = default)
    {
        await _context.SaveChangesAsync(token);
    }
}
