using EventManagement.Data;
using EventManagement.Interfaces.Reposirories;
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

    /// <inheritdoc/>>
    public AppDbContext Context => _context;

    /// <inheritdoc/>    
    public async Task<T> AddAsync(T entity, CancellationToken token = default)
    {
        return await addAsync(entity, _context, token);
    }

    /// <inheritdoc/>    
    public async Task<T> AddAsync(T entity, AppDbContext context, CancellationToken token = default)
    {
        return await addAsync(entity, context, token);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken token = default)
    {
        return await deleteAsync(id, _context, token);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(Guid id, AppDbContext context, CancellationToken token = default)
    {
        return await deleteAsync(id, context, token);
    }

    /// <inheritdoc/>
    public async Task<T?> GetByIdAsync(Guid id, CancellationToken token = default)
    {
        return await getByIdAsync(id, _context, token);
    }

    /// <inheritdoc/>
    public async Task<T?> GetByIdAsync(Guid id, AppDbContext context, CancellationToken token = default)
    {
        return await getByIdAsync(id, context, token);
    }
    
    /// <inheritdoc/>
    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken token = default)
    {
        return await getAllAsync(_context, token);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<T>> GetAllAsync(AppDbContext context, CancellationToken token = default)
    {
        return await getAllAsync(context, token);
    }

    /// <inheritdoc/>
    public async Task<int> GetCountAsync(CancellationToken token = default)
    {
        return await getCountAsync(_context, token);
    }

    /// <inheritdoc/>
    public async Task<int> GetCountAsync(AppDbContext context, CancellationToken token = default)
    {
        return await getCountAsync(context, token);
    }

    /// <inheritdoc/>
    public async Task SaveChangesAsync(CancellationToken token = default)
    {
        await saveChangesAsync(_context, token);
    }

    /// <inheritdoc/>
    public async Task SaveChangesAsync(AppDbContext context, CancellationToken token = default)
    {
        await saveChangesAsync(context, token);
    }

    private  async Task<T> addAsync(T entity, AppDbContext context, CancellationToken token = default)
    {
        await context.Set<T>().AddAsync(entity, token);
        await context.SaveChangesAsync(token);

        return entity;
    }

    private async Task<bool> deleteAsync(Guid id, AppDbContext context, CancellationToken token = default)
    {
        var entity = await context.Set<T>().FindAsync(id, token);
        if (entity == null)
            return false;
        context.Remove(entity);
        await context.SaveChangesAsync(token);

        return true;
    }

    private async Task<T?> getByIdAsync(Guid id, AppDbContext context, CancellationToken token = default)
    {
        return await context.Set<T>().FindAsync(id, token);
    }

    private async Task<IReadOnlyList<T>> getAllAsync(AppDbContext context, CancellationToken token = default)
    {
        return await context.Set<T>().ToListAsync(token);
    }

    private async Task<int> getCountAsync(AppDbContext context, CancellationToken token = default)
    {
        return await context.Set<T>().CountAsync(token);
    }

    private async Task saveChangesAsync(AppDbContext context, CancellationToken token = default)
    {
        await context.SaveChangesAsync(token);
    }
}
