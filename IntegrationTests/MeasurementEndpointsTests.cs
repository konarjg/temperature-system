using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using DatabaseAdapters;
using DatabaseAdapters.Repositories.Test;
using Domain.Entities;
using Domain.Entities.Util;
using IntegrationTests.Utils;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IntegrationTests {
  public class MeasurementEndpointsTests : IClassFixture<CustomWebApplicationFactory> {
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public MeasurementEndpointsTests(CustomWebApplicationFactory factory) {
      _factory = factory;
      _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetHistory_WhenDataExists_ShouldReturnOkAndPaginatedData() {
      // Arrange
      HttpClient adminClient = await IntegrationTestAuthHelper.GetAuthenticatedClient(_factory.CreateClient(), Role.Admin);
      long createdSensorId = await CreateSensor(adminClient, "History Test Sensor");

      using (IServiceScope scope = _factory.Services.CreateScope()) {
        IDatabaseContext dbContext = scope.ServiceProvider.GetRequiredService<IDatabaseContext>();
        DateTime now = DateTime.UtcNow;
        List<Measurement> measurements = new() {
          new Measurement { SensorId = createdSensorId, TemperatureCelsius = 20.1f, Timestamp = now.AddMinutes(-10) },
          new Measurement { SensorId = createdSensorId, TemperatureCelsius = 20.2f, Timestamp = now.AddMinutes(-8) },
          new Measurement { SensorId = createdSensorId, TemperatureCelsius = 20.3f, Timestamp = now.AddMinutes(-6) }
        };
        await ((TestDatabaseContext)dbContext).Measurements.AddRangeAsync(measurements);
        await dbContext.SaveChangesAsync();
      }

      HttpClient viewerClient = await IntegrationTestAuthHelper.GetAuthenticatedClient(_factory.CreateClient(), Role.Viewer);
      string startDate = DateTime.UtcNow.AddHours(-1).ToString("o");
      string endDate = DateTime.UtcNow.AddHours(1).ToString("o");

      // Act
      HttpResponseMessage response = await viewerClient.GetAsync($"/api/measurements/history?startDate={startDate}&endDate={endDate}&sensorId={createdSensorId}");

      // Assert
      response.EnsureSuccessStatusCode();
      Assert.Equal(HttpStatusCode.OK, response.StatusCode);

      using JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
      JsonElement root = doc.RootElement;

      Assert.Equal(3, root.GetProperty("totalCount").GetInt32());
      Assert.Equal(3, root.GetProperty("items").GetArrayLength());
      Assert.Equal(20.3f, root.GetProperty("items")[0].GetProperty("temperatureCelsius").GetSingle());
    }

    [Fact]
    public async Task GetLatest_ShouldReturnCorrectNumberOfPoints() {
      // Arrange
      HttpClient adminClient = await IntegrationTestAuthHelper.GetAuthenticatedClient(_factory.CreateClient(), Role.Admin);
      long createdSensorId = await CreateSensor(adminClient, "Latest Test Sensor");
      await SeedMeasurements(createdSensorId, 5);

      HttpClient viewerClient = await IntegrationTestAuthHelper.GetAuthenticatedClient(_factory.CreateClient(), Role.Viewer);

      // Act
      HttpResponseMessage response = await viewerClient.GetAsync($"/api/measurements/latest?sensorId={createdSensorId}&points=3");

      // Assert
      response.EnsureSuccessStatusCode();
      using JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
      Assert.Equal(3, doc.RootElement.GetArrayLength());
    }

    [Fact]
    public async Task GetAggregatedHistory_ShouldReturnAggregatedData() {
      // Arrange
      HttpClient adminClient = await IntegrationTestAuthHelper.GetAuthenticatedClient(_factory.CreateClient(), Role.Admin);
      long createdSensorId = await CreateSensor(adminClient, "Aggregated Test Sensor");

      using (IServiceScope scope = _factory.Services.CreateScope()) {
        IDatabaseContext dbContext = scope.ServiceProvider.GetRequiredService<IDatabaseContext>();
        DateTime now = DateTime.UtcNow;
        List<Measurement> measurements = new() {
          new Measurement { SensorId = createdSensorId, TemperatureCelsius = 10f, Timestamp = now.AddMinutes(-50) },
          new Measurement { SensorId = createdSensorId, TemperatureCelsius = 20f, Timestamp = now.AddMinutes(-40) },
          new Measurement { SensorId = createdSensorId, TemperatureCelsius = 30f, Timestamp = now.AddHours(-1) },
        };
        await ((TestDatabaseContext)dbContext).Measurements.AddRangeAsync(measurements);
        await dbContext.SaveChangesAsync();
      }

      HttpClient viewerClient = await IntegrationTestAuthHelper.GetAuthenticatedClient(_factory.CreateClient(), Role.Viewer);
      string startDate = DateTime.UtcNow.AddHours(-2).ToString("o");
      string endDate = DateTime.UtcNow.AddHours(1).ToString("o");

      // Act
      HttpResponseMessage response = await viewerClient.GetAsync($"/api/measurements/aggregated-history?startDate={startDate}&endDate={endDate}&granularity=Hourly&sensorId={createdSensorId}");

      // Assert
      response.EnsureSuccessStatusCode();
      using JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
      Assert.Equal(2, doc.RootElement.GetArrayLength());
    }

    private async Task<long> CreateSensor(HttpClient client, string displayName) {
      var createRequest = new { DisplayName = displayName, DeviceAddress = $"addr-{Guid.NewGuid()}" };
      HttpResponseMessage createResponse = await client.PostAsJsonAsync("/api/sensors", createRequest);
      createResponse.EnsureSuccessStatusCode();
      using JsonDocument doc = await JsonDocument.ParseAsync(await createResponse.Content.ReadAsStreamAsync());
      return doc.RootElement.GetProperty("id").GetInt64();
    }

    private async Task SeedMeasurements(long sensorId, int count) {
      using IServiceScope scope = _factory.Services.CreateScope();
      IDatabaseContext dbContext = scope.ServiceProvider.GetRequiredService<IDatabaseContext>();
      List<Measurement> measurements = new();
      for (int i = 0; i < count; i++) {
        measurements.Add(new Measurement { SensorId = sensorId, TemperatureCelsius = 20f + i, Timestamp = DateTime.UtcNow.AddMinutes(-i * 2) });
      }
      await ((TestDatabaseContext)dbContext).Measurements.AddRangeAsync(measurements);
      await dbContext.SaveChangesAsync();
    }
  }
}