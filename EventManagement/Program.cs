using EventManagement.Interfaces;
using EventManagement.Services;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

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
}

builder.Services.AddScoped<IEventValidator, EventValidator>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();

app.Run();