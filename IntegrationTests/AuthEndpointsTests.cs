using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IntegrationTests {
  public class AuthEndpointsTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory> {
    private readonly HttpClient _client = factory.CreateClient();
    private readonly CustomWebApplicationFactory _factory = factory;

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
    public async Task Login_WithValidCredentials_ShouldReturnOkAndToken()
    {
        var registrationRequest = new { Email = "login.user@example.com", Password = "Password123!" };
        var createResponse = await _client.PostAsJsonAsync("/api/users", registrationRequest);
        createResponse.EnsureSuccessStatusCode();

        string createdUserString = await createResponse.Content.ReadAsStringAsync();
        using JsonDocument createdUserDoc = JsonDocument.Parse(createdUserString);
        long createdUserId = createdUserDoc.RootElement.GetProperty("id").GetInt64();

        var mockEmailService = _factory.Services.GetRequiredService<Domain.Services.External.IEmailService>() as ExternalServiceAdapters.EmailService.MockEmailService;
        Assert.NotNull(mockEmailService?.LastVerificationToken);

        var verifyResponse = await _client.GetAsync($"/api/auth/verify/{mockEmailService.LastVerificationToken}");
        verifyResponse.EnsureSuccessStatusCode();

        var loginRequest = new { Email = "login.user@example.com", Password = "Password123!" };
        var response = await _client.PostAsJsonAsync("/api/auth", loginRequest);
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