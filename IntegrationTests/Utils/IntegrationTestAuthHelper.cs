using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Domain.Entities.Util;
using Domain.Records;
using TemperatureSystem.Dto;

namespace IntegrationTests.Utils {
  public static class IntegrationTestAuthHelper {
    public static async Task<HttpClient> GetAuthenticatedClient(HttpClient client, Role role = Role.Viewer) {
      // Generate a unique user for each authentication to ensure test isolation
      string email = $"testuser-{System.Guid.NewGuid()}@example.com";
      string password = "Password123!";

      // Register the new user
      // Note: The API doesn't let us set a role on creation, so the 'role' parameter is for future use
      // if the API is extended. All users will be created as Viewers initially.
      UserRequest registrationRequest = new(email, password);
      await client.PostAsJsonAsync("/api/users", registrationRequest);

      // Log in as the new user
      AuthRequest loginRequest = new(email, password);
      HttpResponseMessage loginResponse = await client.PostAsJsonAsync("/api/auth", loginRequest);
      loginResponse.EnsureSuccessStatusCode();

      AuthResultDto? authResult = await loginResponse.Content.ReadFromJsonAsync<AuthResultDto>();

      // Set the Authorization header for subsequent requests
      client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult!.AccessToken);

      return client;
    }
  }
}