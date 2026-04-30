using EventManagement.Models.BookingModels;
using EventManagement.Models.Events;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace EventManagement.Data;

/// <summary>
/// Контекст базы данных
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="options">Параметры контекста</param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }

    /// <summary>
    /// Коллекция событий
    /// </summary>
    public DbSet<Event> Events => Set<Event>();

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
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
