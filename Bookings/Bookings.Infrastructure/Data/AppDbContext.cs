using Bookings.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Bookings.Infrastructure.Data;

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
    /// Коллекция бронирований
    /// </summary>
    public DbSet<Booking> Bookings => Set<Booking>();

    /// <summary>
    /// Создание модели
    /// </summary>
    /// <param name="modelBuilder">Конструктор модели</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
