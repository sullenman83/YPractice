using Events.Application.Extensions;
using Events.Infrastructure.Extensions;
using Events.Presentation.Extensions;
using Events.Presentation.Extensions.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddPresentation(builder.Environment, builder.Configuration);

if (builder.Environment.IsDevelopment())
{
    builder.Host.UseDefaultServiceProvider(options =>
    {
        options.ValidateOnBuild = true;
        options.ValidateScopes = true;
    });
}

var app = builder.Build();

app.UseGlobalExceptionHandling();
app.ApplyMigration();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapPrometheusScrapingEndpoint();

app.MapControllers();

app.Run();
