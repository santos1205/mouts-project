using System.Net;
using System.Text.Json;

namespace DeveloperStore.Api.Middleware;

public class GlobalExceptionMiddleware
{
  private readonly RequestDelegate _next;
  private readonly ILogger<GlobalExceptionMiddleware> _logger;
  private readonly IHostEnvironment _environment;

  public GlobalExceptionMiddleware(
      RequestDelegate next,
      ILogger<GlobalExceptionMiddleware> logger,
      IHostEnvironment environment)
  {
    _next = next;
    _logger = logger;
    _environment = environment;
  }

  public async Task InvokeAsync(HttpContext context)
  {
    try
    {
      await _next(context);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "An unhandled exception occurred. RequestId: {RequestId}, Path: {Path}",
          context.TraceIdentifier, context.Request.Path);

      await HandleExceptionAsync(context, ex);
    }
  }

  private async Task HandleExceptionAsync(HttpContext context, Exception exception)
  {
    context.Response.ContentType = "application/json";

    var response = new ErrorResponse
    {
      RequestId = context.TraceIdentifier,
      Timestamp = DateTime.UtcNow
    };

    switch (exception)
    {
      case ArgumentException:
      case InvalidOperationException:
        response.StatusCode = (int)HttpStatusCode.BadRequest;
        response.Message = "Invalid request";
        response.Details = _environment.IsDevelopment() ? exception.Message : null;
        break;

      case KeyNotFoundException:
        response.StatusCode = (int)HttpStatusCode.NotFound;
        response.Message = "Resource not found";
        response.Details = _environment.IsDevelopment() ? exception.Message : null;
        break;

      case UnauthorizedAccessException:
        response.StatusCode = (int)HttpStatusCode.Unauthorized;
        response.Message = "Unauthorized access";
        response.Details = _environment.IsDevelopment() ? exception.Message : null;
        break;

      default:
        response.StatusCode = (int)HttpStatusCode.InternalServerError;
        response.Message = "An error occurred while processing your request";
        response.Details = _environment.IsDevelopment() ? exception.ToString() : null;
        break;
    }

    context.Response.StatusCode = response.StatusCode;

    var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    });

    await context.Response.WriteAsync(jsonResponse);
  }
}

public class ErrorResponse
{
  public int StatusCode { get; set; }
  public string Message { get; set; } = string.Empty;
  public string? Details { get; set; }
  public string RequestId { get; set; } = string.Empty;
  public DateTime Timestamp { get; set; }
}
