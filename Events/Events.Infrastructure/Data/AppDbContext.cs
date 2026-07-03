using Events.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Events.Infrastructure.Data;

/// <summary>
/// Контекст базы данных
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Конструктор
    /// </summary>
    public AppDbContext() { }

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="options">Параметры контекста</param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }

    /// <summary>
    /// Коллекция событий`
    /// </summary>
    public DbSet<Event> Events => Set<Event>();

    /// <summary>
    /// Создание модели
    /// </summary>
    /// <param name="modelBuilder">Конструктор модели</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
