using EventManagement.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace EventApi.IntegrationTest;

public class BaseTest : IClassFixture<DatabaseFixture>
{
//    private readonly DatabaseFixture _fixture;

//    public BaseTest(DatabaseFixture fixture)
//    {
//        _fixture = fixture;
//    }

//    protected async Task<AppDbContext> CreateContextAsync()
//    {
//        var options = new DbContextOptionsBuilder<AppDbContext>()
//            .UseNpgsql(_fixture.ConnectionString)
//            .Options;
//        var context = new AppDbContext(options);
//        await context.Database.MigrateAsync();

//        return context;
//    }

//    protected async Task ResetDatabaseAsync()
//    {
//        await using var context = await CreateContextAsync();
//        await context.Database.ExecuteSqlRawAsync(
//"TRUNCATE TABLE bookings, events CASCADE"
//        );
//    }
}
