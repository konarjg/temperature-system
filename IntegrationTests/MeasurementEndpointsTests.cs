using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DatabaseAdapters;
using DatabaseAdapters.Repositories.Test;
using Domain.Entities;
using Domain.Entities.Util;
using Domain.Records;
using IntegrationTests.Utils;
using Microsoft.Extensions.DependencyInjection;
using TemperatureSystem.Dto;
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
      SensorDto? createdSensor = await CreateSensor(adminClient, "History Test Sensor");
      Assert.NotNull(createdSensor);

      using (IServiceScope scope = _factory.Services.CreateScope()) {
        TestDatabaseContext dbContext = (TestDatabaseContext)scope.ServiceProvider.GetRequiredService<IDatabaseContext>();
        DateTime now = DateTime.UtcNow;
        List<Measurement> measurements = new() {
          new Measurement { SensorId = createdSensor.Id, TemperatureCelsius = 20.1f, Timestamp = now.AddMinutes(-10) },
          new Measurement { SensorId = createdSensor.Id, TemperatureCelsius = 20.2f, Timestamp = now.AddMinutes(-8) },
          new Measurement { SensorId = createdSensor.Id, TemperatureCelsius = 20.3f, Timestamp = now.AddMinutes(-6) }
        };
        await dbContext.Measurements.AddRangeAsync(measurements);
        await dbContext.SaveChangesAsync();
      }

      HttpClient viewerClient = await IntegrationTestAuthHelper.GetAuthenticatedClient(_factory.CreateClient(), Role.Viewer);
      string startDate = DateTime.UtcNow.AddHours(-1).ToString("o");
      string endDate = DateTime.UtcNow.AddHours(1).ToString("o");

      // Act
      HttpResponseMessage response = await viewerClient.GetAsync($"/api/measurements/history?startDate={startDate}&endDate={endDate}&sensorId={createdSensor.Id}");

      // Assert
      response.EnsureSuccessStatusCode();
      Assert.Equal(HttpStatusCode.OK, response.StatusCode);

      PagedResultDto<MeasurementDto>? pagedResult = await response.Content.ReadFromJsonAsync<PagedResultDto<MeasurementDto>>();
      Assert.NotNull(pagedResult);
      Assert.Equal(3, pagedResult.TotalCount);
      Assert.Equal(3, pagedResult.Items.Count());
      Assert.Equal(20.3f, pagedResult.Items.First().TemperatureCelsius);
    }

    [Fact]
    public async Task GetLatest_ShouldReturnCorrectNumberOfPoints() {
      // Arrange
      HttpClient adminClient = await IntegrationTestAuthHelper.GetAuthenticatedClient(_factory.CreateClient(), Role.Admin);
      SensorDto? createdSensor = await CreateSensor(adminClient, "Latest Test Sensor");
      await SeedMeasurements(createdSensor!.Id, 5);

      HttpClient viewerClient = await IntegrationTestAuthHelper.GetAuthenticatedClient(_factory.CreateClient(), Role.Viewer);

      // Act
      HttpResponseMessage response = await viewerClient.GetAsync($"/api/measurements/latest?sensorId={createdSensor.Id}&points=3");

      // Assert
      response.EnsureSuccessStatusCode();
      List<MeasurementDto>? latestMeasurements = await response.Content.ReadFromJsonAsync<List<MeasurementDto>>();
      Assert.NotNull(latestMeasurements);
      Assert.Equal(3, latestMeasurements.Count);
    }

    [Fact]
    public async Task GetAggregatedHistory_ShouldReturnAggregatedData() {
      // Arrange
      HttpClient adminClient = await IntegrationTestAuthHelper.GetAuthenticatedClient(_factory.CreateClient(), Role.Admin);
      SensorDto? createdSensor = await CreateSensor(adminClient, "Aggregated Test Sensor");

      using (IServiceScope scope = _factory.Services.CreateScope()) {
        TestDatabaseContext dbContext = (TestDatabaseContext)scope.ServiceProvider.GetRequiredService<IDatabaseContext>();
        DateTime now = DateTime.UtcNow;
        List<Measurement> measurements = new() {
          new Measurement { SensorId = createdSensor!.Id, TemperatureCelsius = 10f, Timestamp = now.AddMinutes(-50) },
          new Measurement { SensorId = createdSensor!.Id, TemperatureCelsius = 20f, Timestamp = now.AddMinutes(-40) },
          new Measurement { SensorId = createdSensor!.Id, TemperatureCelsius = 30f, Timestamp = now.AddHours(-1) },
        };
        await dbContext.Measurements.AddRangeAsync(measurements);
        await dbContext.SaveChangesAsync();
      }

      HttpClient viewerClient = await IntegrationTestAuthHelper.GetAuthenticatedClient(_factory.CreateClient(), Role.Viewer);
      string startDate = DateTime.UtcNow.AddHours(-2).ToString("o");
      string endDate = DateTime.UtcNow.AddHours(1).ToString("o");

      // Act
      HttpResponseMessage response = await viewerClient.GetAsync($"/api/measurements/aggregated-history?startDate={startDate}&endDate={endDate}&granularity=Hourly&sensorId={createdSensor!.Id}");

      // Assert
      response.EnsureSuccessStatusCode();
      List<AggregatedMeasurement>? aggregatedResult = await response.Content.ReadFromJsonAsync<List<AggregatedMeasurement>>();
      Assert.NotNull(aggregatedResult);
      Assert.Equal(2, aggregatedResult.Count);
    }

    private async Task<SensorDto?> CreateSensor(HttpClient client, string displayName) {
      SensorRequest createRequest = new(displayName, $"addr-{Guid.NewGuid()}");
      HttpResponseMessage createResponse = await client.PostAsJsonAsync("/api/sensors", createRequest);
      createResponse.EnsureSuccessStatusCode();
      return await createResponse.Content.ReadFromJsonAsync<SensorDto>();
    }

    private async Task SeedMeasurements(long sensorId, int count) {
      using IServiceScope scope = _factory.Services.CreateScope();
      TestDatabaseContext dbContext = (TestDatabaseContext)scope.ServiceProvider.GetRequiredService<IDatabaseContext>();
      List<Measurement> measurements = new();
      for (int i = 0; i < count; i++) {
        measurements.Add(new Measurement { SensorId = sensorId, TemperatureCelsius = 20f + i, Timestamp = DateTime.UtcNow.AddMinutes(-i * 2) });
      }
      await dbContext.Measurements.AddRangeAsync(measurements);
      await dbContext.SaveChangesAsync();
    }
  }
}