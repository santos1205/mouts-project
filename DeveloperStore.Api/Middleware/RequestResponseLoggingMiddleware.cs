using System.Diagnostics;
using System.Text;

namespace DeveloperStore.Api.Middleware;

public class RequestResponseLoggingMiddleware
{
  private readonly RequestDelegate _next;
  private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

  public RequestResponseLoggingMiddleware(
      RequestDelegate next,
      ILogger<RequestResponseLoggingMiddleware> logger)
  {
    _next = next;
    _logger = logger;
  }

  public async Task InvokeAsync(HttpContext context)
  {
    var stopwatch = Stopwatch.StartNew();
    var requestId = context.TraceIdentifier;

    // Log request
    await LogRequestAsync(context, requestId);

    // Capture original response body stream
    var originalResponseBodyStream = context.Response.Body;

    using var responseBodyStream = new MemoryStream();
    context.Response.Body = responseBodyStream;

    try
    {
      await _next(context);
    }
    finally
    {
      stopwatch.Stop();

      // Log response
      await LogResponseAsync(context, requestId, stopwatch.ElapsedMilliseconds);

      // Copy response body back to original stream
      responseBodyStream.Seek(0, SeekOrigin.Begin);
      await responseBodyStream.CopyToAsync(originalResponseBodyStream);
      context.Response.Body = originalResponseBodyStream;
    }
  }

  private async Task LogRequestAsync(HttpContext context, string requestId)
  {
    var request = context.Request;

    var requestBody = string.Empty;
    if (request.Body.CanSeek && request.ContentLength > 0)
    {
      request.EnableBuffering();
      var buffer = new byte[request.ContentLength.Value];
      await request.Body.ReadAsync(buffer, 0, buffer.Length);
      requestBody = Encoding.UTF8.GetString(buffer);
      request.Body.Position = 0;
    }

    _logger.LogInformation(
        "HTTP Request - {RequestId} {Method} {Path}{QueryString} " +
        "Content-Type: {ContentType} Content-Length: {ContentLength}",
        requestId,
        request.Method,
        request.Path,
        request.QueryString,
        request.ContentType,
        request.ContentLength);

    if (!string.IsNullOrEmpty(requestBody) && ShouldLogBody(request.ContentType))
    {
      _logger.LogDebug("Request Body - {RequestId}: {RequestBody}", requestId, requestBody);
    }
  }

  private async Task LogResponseAsync(HttpContext context, string requestId, long elapsedMilliseconds)
  {
    var response = context.Response;

    var responseBody = string.Empty;
    if (response.Body.CanSeek)
    {
      response.Body.Seek(0, SeekOrigin.Begin);
      responseBody = await new StreamReader(response.Body).ReadToEndAsync();
      response.Body.Seek(0, SeekOrigin.Begin);
    }

    var logLevel = response.StatusCode >= 400 ? LogLevel.Warning : LogLevel.Information;

    _logger.Log(logLevel,
        "HTTP Response - {RequestId} Status: {StatusCode} " +
        "Content-Type: {ContentType} Duration: {Duration}ms",
        requestId,
        response.StatusCode,
        response.ContentType,
        elapsedMilliseconds);

    if (!string.IsNullOrEmpty(responseBody) && ShouldLogBody(response.ContentType))
    {
      _logger.LogDebug("Response Body - {RequestId}: {ResponseBody}", requestId, responseBody);
    }
  }

  private static bool ShouldLogBody(string? contentType)
  {
    if (string.IsNullOrEmpty(contentType))
      return false;

    return contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase) ||
           contentType.Contains("application/xml", StringComparison.OrdinalIgnoreCase) ||
           contentType.Contains("text/", StringComparison.OrdinalIgnoreCase);
  }
}
