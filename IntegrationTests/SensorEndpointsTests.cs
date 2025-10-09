using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Domain.Entities.Util;
using IntegrationTests.Utils;
using TemperatureSystem.Dto;
using Xunit;

namespace IntegrationTests;

public class SensorEndpointsTests : IClassFixture<CustomWebApplicationFactory> {
  private readonly HttpClient _client;

  public SensorEndpointsTests(CustomWebApplicationFactory factory) {
    _client = factory.CreateClient();
  }

  [Fact]
  public async Task CreateSensor_AsAdmin_ShouldReturnCreated() {
    // Arrange
    HttpClient adminClient = await IntegrationTestAuthHelper.GetAuthenticatedClient(new HttpClient { BaseAddress = _client.BaseAddress }, Role.Admin);
    SensorRequest newSensorRequest = new("Living Room Sensor", "28-0000076d22e4");

    // Act
    HttpResponseMessage response = await adminClient.PostAsJsonAsync("/api/sensors", newSensorRequest);

    // Assert
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    SensorDto? createdSensor = await response.Content.ReadFromJsonAsync<SensorDto>();
    Assert.NotNull(createdSensor);
    Assert.Equal(newSensorRequest.DisplayName, createdSensor.DisplayName);
  }

  [Fact]
  public async Task CreateSensor_AsViewer_ShouldReturnForbidden() {
    // Arrange
    HttpClient viewerClient = await IntegrationTestAuthHelper.GetAuthenticatedClient(new HttpClient { BaseAddress = _client.BaseAddress }, Role.Viewer);
    SensorRequest newSensorRequest = new("Basement Sensor", "28-0000076d1111");

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
    SensorRequest createRequest = new("Initial Name", "addr-to-update");
    HttpResponseMessage createResponse = await adminClient.PostAsJsonAsync("/api/sensors", createRequest);
    SensorDto? createdSensor = await createResponse.Content.ReadFromJsonAsync<SensorDto>();

    SensorRequest updateRequest = new("Updated Name", "addr-updated");

    // Act
    HttpResponseMessage updateResponse = await adminClient.PutAsJsonAsync($"/api/sensors/{createdSensor!.Id}", updateRequest);

    // Assert
    Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
  }

  [Fact]
  public async Task DeleteSensor_AsAdmin_ShouldReturnNoContent() {
    // Arrange
    HttpClient adminClient = await IntegrationTestAuthHelper.GetAuthenticatedClient(new HttpClient { BaseAddress = _client.BaseAddress }, Role.Admin);
    SensorRequest createRequest = new("To Be Deleted", "addr-to-delete");
    HttpResponseMessage createResponse = await adminClient.PostAsJsonAsync("/api/sensors", createRequest);
    SensorDto? createdSensor = await createResponse.Content.ReadFromJsonAsync<SensorDto>();

    // Act
    HttpResponseMessage deleteResponse = await adminClient.DeleteAsync($"/api/sensors/{createdSensor!.Id}");

    // Assert
    Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
  }

  [Fact]
  public async Task DeleteSensor_AsViewer_ShouldReturnForbidden() {
    // Arrange
    // Create a sensor as Admin first
    HttpClient adminClient = await IntegrationTestAuthHelper.GetAuthenticatedClient(new HttpClient { BaseAddress = _client.BaseAddress }, Role.Admin);
    SensorRequest createRequest = new("Protected Sensor", "addr-protected");
    HttpResponseMessage createResponse = await adminClient.PostAsJsonAsync("/api/sensors", createRequest);
    SensorDto? createdSensor = await createResponse.Content.ReadFromJsonAsync<SensorDto>();

    // Now, attempt to delete it as Viewer
    HttpClient viewerClient = await IntegrationTestAuthHelper.GetAuthenticatedClient(new HttpClient { BaseAddress = _client.BaseAddress }, Role.Viewer);

    // Act
    HttpResponseMessage deleteResponse = await viewerClient.DeleteAsync($"/api/sensors/{createdSensor!.Id}");

    // Assert
    Assert.Equal(HttpStatusCode.Forbidden, deleteResponse.StatusCode);
  }
}