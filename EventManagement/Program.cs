using EventManagement.Interfaces;
using EventManagement.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IEventValidator, EventValidator>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.Run();