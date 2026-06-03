using EventManagement.Application.Common;
using EventManagement.Application.Common.AppSettings;
using EventManagement.Application.Common.Exceptions;
using EventManagement.Application.Interfaces;
using EventManagement.Application.Interfaces.Reposirories;
using EventManagement.Application.Interfaces.Services;
using EventManagement.Application.Services;
using EventManagement.Application.Services.BookingServices;
using EventManagement.Application.Services.EventServices;
using EventManagement.Domain.Interfaces;
using EventManagement.Domain.Models;
using EventManagement.Domain.Services;
using EventManagement.Infrastructure.Data;
using EventManagement.Infrastructure.Services.BookingServices;
using EventManagement.Infrastructure.Services.EventServices;
using EventManagement.Infrastructure.Services.TransactionService;
using EventManagement.Web.Extensions.Middleware;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;
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
builder.Services.AddResiliencePipeline(Consts.CreateBookingRetry, builder =>
{
    builder.AddRetry(new RetryStrategyOptions()
    {
        ShouldHandle = new PredicateBuilder().Handle<DbOperationWithBlockingRowException>(),
        MaxRetryAttempts = retrySettings.MaxRetryAttempts,
        Delay = TimeSpan.FromMilliseconds(retrySettings.Delay),
        BackoffType = DelayBackoffType.Constant
    });
});

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