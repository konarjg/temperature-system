using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Domain.Entities.Util;

namespace IntegrationTests.Utils {
  public static class IntegrationTestAuthHelper {
    public static async Task<HttpClient> GetAuthenticatedClient(HttpClient client, Role role = Role.Viewer) {
      string email = $"testuser-{System.Guid.NewGuid()}@example.com";
      string password = "Password123!";

      // Register the new user using an anonymous object
      var registrationRequest = new { Email = email, Password = password };
      await client.PostAsJsonAsync("/api/users", registrationRequest);

      // Log in as the new user using a dictionary
      var loginRequest = new Dictionary<string, string> {
        { "email", email },
        { "password", password }
      };
      HttpResponseMessage loginResponse = await client.PostAsJsonAsync("/api/auth", loginRequest);
      loginResponse.EnsureSuccessStatusCode();

      // Deserialize to a JsonElement to extract the token
      using JsonDocument doc = await JsonDocument.ParseAsync(await loginResponse.Content.ReadAsStreamAsync());
      string accessToken = doc.RootElement.GetProperty("accessToken").GetString()!;

      client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

      return client;
    }
  }
}