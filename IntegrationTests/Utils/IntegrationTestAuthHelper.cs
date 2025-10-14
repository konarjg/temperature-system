using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Domain.Entities.Util;
using Microsoft.AspNetCore.Mvc.Testing;

namespace IntegrationTests.Utils {
  public static class IntegrationTestAuthHelper {
    public static async Task<HttpClient> GetAuthenticatedClient(WebApplicationFactory<Program> factory, Role role = Role.Viewer) {
      HttpClient client = factory.CreateClient();
      string email = $"testuser-{System.Guid.NewGuid()}@example.com";
      string password = "Password123!";

      var registrationRequest = new { Email = email, Password = password };
      await client.PostAsJsonAsync("/api/users", registrationRequest);

      var loginRequest = new Dictionary<string, string> {
        { "email", email },
        { "password", password }
      };
      HttpResponseMessage loginResponse = await client.PostAsJsonAsync("/api/auth", loginRequest);
      loginResponse.EnsureSuccessStatusCode();
      
      string responseString = await loginResponse.Content.ReadAsStringAsync();
      using JsonDocument doc = JsonDocument.Parse(responseString);
      string accessToken = doc.RootElement.GetProperty("accessToken").GetString()!;

      client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

      return client;
    }
  }
}
