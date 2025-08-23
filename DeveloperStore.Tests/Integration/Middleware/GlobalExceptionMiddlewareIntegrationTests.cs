using System.Net;
using System.Text.Json;
using DeveloperStore.Api.Middleware;
using FluentAssertions;

namespace DeveloperStore.Tests.Integration.Middleware;

public class GlobalExceptionMiddlewareIntegrationTests : IClassFixture<DeveloperStoreWebApplicationFactory>
{
  private readonly HttpClient _client;

  public GlobalExceptionMiddlewareIntegrationTests(DeveloperStoreWebApplicationFactory factory)
  {
    _client = factory.CreateClient();
  }

  [Fact]
  public async Task GlobalExceptionMiddleware_HandlesNotFound_ReturnsStructuredError()
  {
    // Act - Request non-existent sale
    var response = await _client.GetAsync($"/api/sales/{Guid.NewGuid()}");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.NotFound);

    var responseContent = await response.Content.ReadAsStringAsync();
    var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseContent, new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true
    });

    errorResponse.Should().NotBeNull();
    errorResponse!.StatusCode.Should().Be(404);
    errorResponse.Message.Should().Be("Resource not found");
    errorResponse.RequestId.Should().NotBeNullOrEmpty();
    errorResponse.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
  }

  [Fact]
  public async Task GlobalExceptionMiddleware_HandlesBadRequest_ReturnsStructuredError()
  {
    // Act - Send invalid JSON
    var invalidJson = "{ invalid json }";
    var content = new StringContent(invalidJson, System.Text.Encoding.UTF8, "application/json");
    var response = await _client.PostAsync("/api/sales", content);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

    var responseContent = await response.Content.ReadAsStringAsync();
    var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseContent, new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true
    });

    errorResponse.Should().NotBeNull();
    errorResponse!.StatusCode.Should().Be(400);
    errorResponse.RequestId.Should().NotBeNullOrEmpty();
  }
}
