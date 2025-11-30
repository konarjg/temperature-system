using System;
using System.Collections.Generic;
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

    public MeasurementEndpointsTests(CustomWebApplicationFactory factory) {
      _factory = factory;
    }

    [Fact]
    public async Task GetHistory_WhenDataExists_ShouldReturnOkAndPaginatedData() {
      HttpClient adminClient = await IntegrationTestAuthHelper.GetAuthenticatedClient(_factory, Role.Admin);
      long createdSensorId = await CreateSensor(adminClient, "History Test Sensor");

      using (var scope = _factory.Services.CreateScope()) {
        var dbContext = scope.ServiceProvider.GetRequiredService<IDatabaseContext>();
        DateTime now = DateTime.UtcNow;
        var measurements = new List<Measurement> {
          new Measurement { SensorId = createdSensorId, TemperatureCelsius = 20.1f, Timestamp = now.AddMinutes(-10) },
          new Measurement { SensorId = createdSensorId, TemperatureCelsius = 20.2f, Timestamp = now.AddMinutes(-8) },
          new Measurement { SensorId = createdSensorId, TemperatureCelsius = 20.3f, Timestamp = now.AddMinutes(-6) }
        };
        await ((TestDatabaseContext)dbContext).Measurements.AddRangeAsync(measurements);
        await dbContext.SaveChangesAsync();
      }

      HttpClient viewerClient = await IntegrationTestAuthHelper.GetAuthenticatedClient(_factory, Role.Viewer);
      string startDate = DateTime.UtcNow.AddHours(-1).ToString("o");
      string endDate = DateTime.UtcNow.AddHours(1).ToString("o");
      
      HttpResponseMessage response = await viewerClient.GetAsync($"/api/measurements/history?startDate={startDate}&endDate={endDate}&sensorId={createdSensorId}");

      response.EnsureSuccessStatusCode();

      string responseString = await response.Content.ReadAsStringAsync();
      using JsonDocument doc = JsonDocument.Parse(responseString);
      JsonElement root = doc.RootElement;

      Assert.Equal(3, root.GetProperty("totalCount").GetInt32());
      Assert.Equal(3, root.GetProperty("items").GetArrayLength());
    }
    
    private async Task<long> CreateSensor(HttpClient client, string displayName) {
        var createRequest = new { DisplayName = displayName, DeviceAddress = $"addr-{Guid.NewGuid()}" };
        HttpResponseMessage createResponse = await client.PostAsJsonAsync("/api/sensors", createRequest);
        createResponse.EnsureSuccessStatusCode();

        string responseString = await createResponse.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(responseString);
        return doc.RootElement.GetProperty("id").GetInt64();
    }

    private async Task SeedMeasurements(long sensorId, int count) {
      using var scope = _factory.Services.CreateScope();
      var dbContext = scope.ServiceProvider.GetRequiredService<IDatabaseContext>();
      var measurements = new List<Measurement>();
      for (int i = 0; i < count; i++) {
        measurements.Add(new Measurement { SensorId = sensorId, TemperatureCelsius = 20f + i, Timestamp = DateTime.UtcNow.AddMinutes(-i * 2) });
      }
      await ((TestDatabaseContext)dbContext).Measurements.AddRangeAsync(measurements);
      await dbContext.SaveChangesAsync();
    }
  }
}