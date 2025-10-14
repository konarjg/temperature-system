using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using DatabaseAdapters.Repositories.Test;
using Domain.Entities;
using Domain.Entities.Util;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IntegrationTests.Utils {
  public static class IntegrationTestAuthHelper {
    public static async Task<HttpClient> GetAuthenticatedClient(WebApplicationFactory<Program> factory, Role role = Role.Viewer) {
      HttpClient client = factory.CreateClient();
      string email = $"testuser-{System.Guid.NewGuid()}@example.com";
      string password = "Password123!";

      var registrationRequest = new { Email = email, Password = password };
      var createResponse = await client.PostAsJsonAsync("/api/users", registrationRequest);
      createResponse.EnsureSuccessStatusCode();

      var mockEmailService = factory.Services.GetRequiredService<Domain.Services.External.IEmailService>() as ExternalServiceAdapters.EmailService.MockEmailService;
      Assert.NotNull(mockEmailService?.LastVerificationToken);

      var verifyResponse = await client.GetAsync($"/api/auth/verify/{mockEmailService.LastVerificationToken}");
      verifyResponse.EnsureSuccessStatusCode();

      if (role == Role.Admin)
      {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDatabaseContext>();
        User? user = dbContext.Users.FirstOrDefault(u => u.Email == email);
        Assert.NotNull(user);
        user.Role = Role.Admin;
        await dbContext.SaveChangesAsync();
      }

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
