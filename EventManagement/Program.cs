using EventManagement.Common.AppSettings;
using EventManagement.Data;
using EventManagement.Extensions.Middleware;
using EventManagement.Interfaces;
using EventManagement.Interfaces.Reposirories;
using EventManagement.Interfaces.Services;
using EventManagement.Models.BookingModels;
using EventManagement.Models.Events;
using EventManagement.Services;
using EventManagement.Services.BookingServices;
using EventManagement.Services.EventServices;
using EventManagement.Services.TransactionService;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var retrySettings = new RetrySettings();
var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Не задана строка подключения к базе даных");

builder.Configuration.GetSection("RetrySettings").Bind(retrySettings);

builder.Services.Configure<BookingHandlerSettings>(builder.Configuration.GetSection("BookingHandlerSettings"));


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
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseNpgsql(connectionString);
    });
}

builder.Services.AddScoped<IEventValidator, EventValidator>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IEventRepository<Event>, EventRepository>();
builder.Services.AddScoped<IBookingRepository<Booking>, BookingRepository>();
builder.Services.AddScoped<IDateTimeProvider, DateTimeProvider>();
builder.Services.AddScoped<IBackgroundBookingService, BackgroundBookingService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddHostedService<BookingHandlerService>();
builder.Services.AddControllers(options =>
{
    options.SuppressAsyncSuffixInActionNames = false;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseGlobalExceptionHandling();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();

app.Run();