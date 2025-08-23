using DeveloperStore.Api.Middleware;
using DeveloperStore.Application;
using DeveloperStore.Infrastructure;
using Microsoft.AspNetCore.HttpLogging;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog only for non-testing environments
if (!builder.Environment.IsEnvironment("Testing"))
{
    Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .WriteTo.File("logs/developerstore-.txt", rollingInterval: RollingInterval.Day)
        .CreateBootstrapLogger();

    builder.Host.UseSerilog((context, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .WriteTo.Console()
            .WriteTo.File("logs/developerstore-.txt", rollingInterval: RollingInterval.Day)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "DeveloperStore.Api");
    });
}

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add HTTP logging for production monitoring
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddHttpLogging(options =>
    {
        options.LoggingFields = HttpLoggingFields.RequestPath |
                               HttpLoggingFields.RequestMethod |
                               HttpLoggingFields.ResponseStatusCode |
                               HttpLoggingFields.Duration;
    });
}

// Add health checks
builder.Services.AddHealthChecks();

// Infrastructure services
if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddInfrastructureDevelopment(builder.Configuration);
}
else
{
    builder.Services.AddInfrastructure(builder.Configuration);
}

// Application services (MediatR, validators, etc.)
builder.Services.AddApplication();

// Add CORS for development and testing
if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("DevelopmentPolicy", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });
}

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseSerilogRequestLogging();
}

// Global exception handling (must be early in pipeline)
app.UseMiddleware<GlobalExceptionMiddleware>();

// Request/Response logging for detailed monitoring
if (app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing"))
{
    app.UseMiddleware<RequestResponseLoggingMiddleware>();
}

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("DevelopmentPolicy");
}

app.UseHttpsRedirection();

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpLogging();
}

// Health checks
app.MapHealthChecks("/health");

app.MapControllers();

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
