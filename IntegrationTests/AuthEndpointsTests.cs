using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Domain.Records;
using TemperatureSystem.Dto;
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
      UserRequest registrationRequest = new("test.user@example.com", "Password123!");

      // Act
      HttpResponseMessage response = await _client.PostAsJsonAsync("/api/users", registrationRequest);

      // Assert
      Assert.Equal(HttpStatusCode.Created, response.StatusCode);

      UserDto? createdUser = await response.Content.ReadFromJsonAsync<UserDto>();
      Assert.NotNull(createdUser);
      Assert.Equal(registrationRequest.Email, createdUser.Email);
      Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturnConflict() {
      // Arrange
      UserRequest registrationRequest = new("duplicate@example.com", "Password123!");
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
      UserRequest registrationRequest = new("login.user@example.com", "Password123!");
      HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/users", registrationRequest);
      UserDto? createdUser = await createResponse.Content.ReadFromJsonAsync<UserDto>();
      Assert.NotNull(createdUser);

      AuthRequest loginRequest = new("login.user@example.com", "Password123!");

      // Act
      HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth", loginRequest);

      // Assert
      response.EnsureSuccessStatusCode();
      Assert.Equal(HttpStatusCode.OK, response.StatusCode);

      AuthResultDto? authResult = await response.Content.ReadFromJsonAsync<AuthResultDto>();
      Assert.NotNull(authResult);
      Assert.NotEmpty(authResult.AccessToken);
      Assert.Equal(createdUser.Id, authResult.UserId);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized() {
      // Arrange
      AuthRequest loginRequest = new("login.user@example.com", "WrongPassword!");

      // Act
      HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth", loginRequest);

      // Assert
      Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
  }
}