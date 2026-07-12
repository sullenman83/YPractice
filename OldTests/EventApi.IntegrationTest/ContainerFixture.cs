using EventManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace EventApi.IntegrationTest;

public class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;
    public AppDbContext Context 
    {
        get
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .UseSnakeCaseNamingConvention()
            .Options;
            return new AppDbContext(options);
        }
    }

    public DatabaseFixture()
    {
         _container = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("EventManagementTest")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .UseSnakeCaseNamingConvention()
            .Options;
        using var context = new AppDbContext(options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        await using var context = Context;
        await context.Database.ExecuteSqlRawAsync(
"TRUNCATE TABLE bookings, users, events CASCADE"
        );
    }
}
