using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace IntegrationTests {
  public class AuthEndpointsTests : IClassFixture<CustomWebApplicationFactory> {
    private readonly HttpClient _client;

    public AuthEndpointsTests(CustomWebApplicationFactory factory) {
      _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturnCreated() {
      // Arrange
      var registrationRequest = new { Email = "test.user@example.com", Password = "Password123!" };

      // Act
      HttpResponseMessage response = await _client.PostAsJsonAsync("/api/users", registrationRequest);

      // Assert
      Assert.Equal(HttpStatusCode.Created, response.StatusCode);

      using JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
      JsonElement root = doc.RootElement;

      Assert.Equal(registrationRequest.Email, root.GetProperty("email").GetString());
      Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturnConflict() {
      // Arrange
      var registrationRequest = new { Email = "duplicate@example.com", Password = "Password123!" };
      // First request should succeed
      await _client.PostAsJsonAsync("/api/users", registrationRequest);

      // Act
      // Second request with the same email should fail
      HttpResponseMessage response = await _client.PostAsJsonAsync("/api/users", registrationRequest);

      // Assert
      Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOkAndToken() {
      // Arrange
      // First, create a user to log in with
      var registrationRequest = new { Email = "login.user@example.com", Password = "Password123!" };
      HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/users", registrationRequest);
      using JsonDocument createdUserDoc = await JsonDocument.ParseAsync(await createResponse.Content.ReadAsStreamAsync());
      long createdUserId = createdUserDoc.RootElement.GetProperty("id").GetInt64();

      var loginRequest = new Dictionary<string, string> {
        { "email", "login.user@example.com" },
        { "password", "Password123!" }
      };

      // Act
      HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth", loginRequest);

      // Assert
      response.EnsureSuccessStatusCode();
      Assert.Equal(HttpStatusCode.OK, response.StatusCode);

      using JsonDocument authResultDoc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
      JsonElement authResultRoot = authResultDoc.RootElement;

      Assert.NotEmpty(authResultRoot.GetProperty("accessToken").GetString());
      Assert.Equal(createdUserId, authResultRoot.GetProperty("userId").GetInt64());
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized() {
      // Arrange
      var loginRequest = new Dictionary<string, string> {
        { "email", "login.user@example.com" },
        { "password", "WrongPassword!" }
      };

      // Act
      HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth", loginRequest);

      // Assert
      Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
  }
}