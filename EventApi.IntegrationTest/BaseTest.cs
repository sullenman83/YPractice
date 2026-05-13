using EventManagement.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace EventApi.IntegrationTest;

public class BaseTest : IAsyncLifetime
{
    protected readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("EventManagementTest")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    protected async Task<AppDbContext> CreateContextAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;
        var context = new AppDbContext(options);
        await context.Database.MigrateAsync();

        return context;
    }

    protected async Task ResetDatabaseAsync()
    {
        await using var context = await CreateContextAsync();
        await context.Database.ExecuteSqlRawAsync(
"TRUNCATE TABLE bookings, events CASCADE"
        );
    }
}
