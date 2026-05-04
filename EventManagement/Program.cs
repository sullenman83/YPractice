using EventManagement.Data;
using EventManagement.Extensions.Middleware;
using EventManagement.Interfaces;
using EventManagement.Services;
using EventManagement.Services.BookingServices;
using EventManagement.Services.EventServices;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Не задана строка подключения к базе даных");

if (builder.Environment.IsDevelopment())
{
    builder.Host.UseDefaultServiceProvider(options =>
    {
        options.ValidateOnBuild = true;
        options.ValidateScopes = true;
    });

    builder.Services.AddSwaggerGen(options =>
    {
        var file = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var path = Path.Combine(AppContext.BaseDirectory, file);
        options.IncludeXmlComments(path);
    });

    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseNpgsql(connectionString)
        .LogTo(Console.WriteLine)
        .EnableDetailedErrors()
        .EnableSensitiveDataLogging();
    });

    //builder.Services.AddDbContextFactory<AppDbContext>(options =>
    //{
    //    options.UseNpgsql(connectionString)
    //    .LogTo(Console.WriteLine)
    //    .EnableDetailedErrors()
    //    .EnableSensitiveDataLogging();
    //});
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseNpgsql(connectionString);
    });

    //builder.Services.AddDbContextFactory<AppDbContext>(options =>
    //{
    //    options.UseNpgsql(connectionString);
    //});
}

builder.Services.AddScoped<IEventValidator, EventValidator>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddHostedService<BookingHandlerService>();
builder.Services.AddControllers(options =>
{
    options.SuppressAsyncSuffixInActionNames = false;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseGlobalExceptionHandling();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();

app.Run();