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
      var registrationRequest = new { Email = "test.user@example.com", Password = "Password123!" };

      HttpResponseMessage response = await _client.PostAsJsonAsync("/api/users", registrationRequest);

      Assert.Equal(HttpStatusCode.Created, response.StatusCode);

      string responseString = await response.Content.ReadAsStringAsync();
      using JsonDocument doc = JsonDocument.Parse(responseString);
      JsonElement root = doc.RootElement;

      Assert.Equal(registrationRequest.Email, root.GetProperty("email").GetString());
      Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturnConflict() {
      var registrationRequest = new { Email = "duplicate@example.com", Password = "Password123!" };
      await _client.PostAsJsonAsync("/api/users", registrationRequest);
      
      HttpResponseMessage response = await _client.PostAsJsonAsync("/api/users", registrationRequest);
      
      Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOkAndToken() {
      var registrationRequest = new { Email = "login.user@example.com", Password = "Password123!" };
      HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/users", registrationRequest);
      
      string createdUserString = await createResponse.Content.ReadAsStringAsync();
      using JsonDocument createdUserDoc = JsonDocument.Parse(createdUserString);
      long createdUserId = createdUserDoc.RootElement.GetProperty("id").GetInt64();

      var loginRequest = new Dictionary<string, string> {
        { "email", "login.user@example.com" },
        { "password", "Password123!" }
      };

      HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth", loginRequest);

      response.EnsureSuccessStatusCode();

      string authResultString = await response.Content.ReadAsStringAsync();
      using JsonDocument authResultDoc = JsonDocument.Parse(authResultString);
      JsonElement authResultRoot = authResultDoc.RootElement;

      Assert.NotEmpty(authResultRoot.GetProperty("accessToken").GetString());
      Assert.Equal(createdUserId, authResultRoot.GetProperty("userId").GetInt64());
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized() {
      var loginRequest = new Dictionary<string, string> {
        { "email", "login.user@example.com" },
        { "password", "WrongPassword!" }
      };
      
      HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth", loginRequest);
      
      Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
  }
}