using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Domain.Entities.Util;
using IntegrationTests.Utils;
using Xunit;

namespace IntegrationTests {
  public class SensorEndpointsTests : IClassFixture<CustomWebApplicationFactory> {
    private readonly CustomWebApplicationFactory _factory;

    public SensorEndpointsTests(CustomWebApplicationFactory factory) {
      _factory = factory;
    }

    [Fact]
    public async Task CreateSensor_AsAdmin_ShouldReturnCreated() {
      HttpClient adminClient = await IntegrationTestAuthHelper.GetAuthenticatedClient(_factory, Role.Admin);
      var newSensorRequest = new { DisplayName = "Living Room Sensor", DeviceAddress = "28-0000076d22e4" };

      HttpResponseMessage response = await adminClient.PostAsJsonAsync("/api/sensors", newSensorRequest);

      Assert.Equal(HttpStatusCode.Created, response.StatusCode);
      
      string responseString = await response.Content.ReadAsStringAsync();
      using JsonDocument doc = JsonDocument.Parse(responseString);
      Assert.Equal(newSensorRequest.DisplayName, doc.RootElement.GetProperty("displayName").GetString());
    }

    [Fact]
    public async Task CreateSensor_AsViewer_ShouldReturnForbidden() {
      HttpClient viewerClient = await IntegrationTestAuthHelper.GetAuthenticatedClient(_factory, Role.Viewer);
      var newSensorRequest = new { DisplayName = "Basement Sensor", DeviceAddress = "28-0000076d1111" };

      HttpResponseMessage response = await viewerClient.PostAsJsonAsync("/api/sensors", newSensorRequest);

      Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateSensor_AsAdmin_ShouldReturnOk() {
      HttpClient adminClient = await IntegrationTestAuthHelper.GetAuthenticatedClient(_factory, Role.Admin);
      var createRequest = new { DisplayName = "Initial Name", DeviceAddress = "addr-to-update" };
      HttpResponseMessage createResponse = await adminClient.PostAsJsonAsync("/api/sensors", createRequest);
      
      string createString = await createResponse.Content.ReadAsStringAsync();
      using JsonDocument createdDoc = JsonDocument.Parse(createString);
      long createdSensorId = createdDoc.RootElement.GetProperty("id").GetInt64();

      var updateRequest = new { DisplayName = "Updated Name", DeviceAddress = "addr-updated" };

      HttpResponseMessage updateResponse = await adminClient.PutAsJsonAsync($"/api/sensors/{createdSensorId}", updateRequest);

      Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
    }
  }
}