using System;
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
    private readonly HttpClient _client;

    public SensorEndpointsTests(CustomWebApplicationFactory factory) {
      _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateSensor_AsAdmin_ShouldReturnCreated() {
      // Arrange
      HttpClient adminClient = await IntegrationTestAuthHelper.GetAuthenticatedClient(new HttpClient { BaseAddress = _client.BaseAddress }, Role.Admin);
      var newSensorRequest = new { DisplayName = "Living Room Sensor", DeviceAddress = "28-0000076d22e4" };

      // Act
      HttpResponseMessage response = await adminClient.PostAsJsonAsync("/api/sensors", newSensorRequest);

      // Assert
      Assert.Equal(HttpStatusCode.Created, response.StatusCode);
      using JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
      Assert.Equal(newSensorRequest.DisplayName, doc.RootElement.GetProperty("displayName").GetString());
    }

    [Fact]
    public async Task CreateSensor_AsViewer_ShouldReturnForbidden() {
      // Arrange
      HttpClient viewerClient = await IntegrationTestAuthHelper.GetAuthenticatedClient(new HttpClient { BaseAddress = _client.BaseAddress }, Role.Viewer);
      var newSensorRequest = new { DisplayName = "Basement Sensor", DeviceAddress = "28-0000076d1111" };

      // Act
      HttpResponseMessage response = await viewerClient.PostAsJsonAsync("/api/sensors", newSensorRequest);

      // Assert
      Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetSensors_AsAuthenticatedUser_ShouldReturnOk() {
      // Arrange
      HttpClient userClient = await IntegrationTestAuthHelper.GetAuthenticatedClient(new HttpClient { BaseAddress = _client.BaseAddress }, Role.Viewer);

      // Act
      HttpResponseMessage response = await userClient.GetAsync("/api/sensors");

      // Assert
      response.EnsureSuccessStatusCode();
      Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateSensor_AsAdmin_ShouldReturnOk() {
      // Arrange
      HttpClient adminClient = await IntegrationTestAuthHelper.GetAuthenticatedClient(new HttpClient { BaseAddress = _client.BaseAddress }, Role.Admin);
      var createRequest = new { DisplayName = "Initial Name", DeviceAddress = "addr-to-update" };
      HttpResponseMessage createResponse = await adminClient.PostAsJsonAsync("/api/sensors", createRequest);
      using JsonDocument createdDoc = await JsonDocument.ParseAsync(await createResponse.Content.ReadAsStreamAsync());
      long createdSensorId = createdDoc.RootElement.GetProperty("id").GetInt64();

      var updateRequest = new { DisplayName = "Updated Name", DeviceAddress = "addr-updated" };

      // Act
      HttpResponseMessage updateResponse = await adminClient.PutAsJsonAsync($"/api/sensors/{createdSensorId}", updateRequest);

      // Assert
      Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteSensor_AsAdmin_ShouldReturnNoContent() {
      // Arrange
      HttpClient adminClient = await IntegrationTestAuthHelper.GetAuthenticatedClient(new HttpClient { BaseAddress = _client.BaseAddress }, Role.Admin);
      var createRequest = new { DisplayName = "To Be Deleted", DeviceAddress = "addr-to-delete" };
      HttpResponseMessage createResponse = await adminClient.PostAsJsonAsync("/api/sensors", createRequest);
      using JsonDocument createdDoc = await JsonDocument.ParseAsync(await createResponse.Content.ReadAsStreamAsync());
      long createdSensorId = createdDoc.RootElement.GetProperty("id").GetInt64();

      // Act
      HttpResponseMessage deleteResponse = await adminClient.DeleteAsync($"/api/sensors/{createdSensorId}");

      // Assert
      Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteSensor_AsViewer_ShouldReturnForbidden() {
      // Arrange
      HttpClient adminClient = await IntegrationTestAuthHelper.GetAuthenticatedClient(new HttpClient { BaseAddress = _client.BaseAddress }, Role.Admin);
      var createRequest = new { DisplayName = "Protected Sensor", DeviceAddress = "addr-protected" };
      HttpResponseMessage createResponse = await adminClient.PostAsJsonAsync("/api/sensors", createRequest);
      using JsonDocument createdDoc = await JsonDocument.ParseAsync(await createResponse.Content.ReadAsStreamAsync());
      long createdSensorId = createdDoc.RootElement.GetProperty("id").GetInt64();

      HttpClient viewerClient = await IntegrationTestAuthHelper.GetAuthenticatedClient(new HttpClient { BaseAddress = _client.BaseAddress }, Role.Viewer);

      // Act
      HttpResponseMessage deleteResponse = await viewerClient.DeleteAsync($"/api/sensors/{createdSensorId}");

      // Assert
      Assert.Equal(HttpStatusCode.Forbidden, deleteResponse.StatusCode);
    }
  }
}