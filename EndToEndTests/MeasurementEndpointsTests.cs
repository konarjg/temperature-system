using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Domain.Records;
using TemperatureSystem.Dto;
using Xunit;

namespace EndToEndTests;

public class MeasurementEndpointsTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public MeasurementEndpointsTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostMeasurement_ShouldCreateNewMeasurement()
    {
        // Arrange
        var newMeasurement = new CreateMeasurementDto(DateTime.UtcNow.ToString("o"), 1, 25.5f);

        // Act
        var response = await _client.PostAsJsonAsync("/api/measurements/", newMeasurement);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetLatestMeasurement_ShouldReturnLatestMeasurementForSensor()
    {
        // Arrange
        var newMeasurement = new CreateMeasurementDto(DateTime.UtcNow.ToString("o"), 1, 30.0f);
        await _client.PostAsJsonAsync("/api/measurements/", newMeasurement);

        // Act
        var response = await _client.GetAsync("/api/measurements/latest?sensorId=1");

        // Assert
        response.EnsureSuccessStatusCode();
        var measurement = await response.Content.ReadFromJsonAsync<MeasurementDto>();

        Assert.NotNull(measurement);
        Assert.Equal(30.0f, measurement.TemperatureCelsius);
        Assert.Equal(1, measurement.SensorId);
    }

    [Fact]
    public async Task GetAggregatedHistory_ShouldReturnAggregatedData()
    {
        // Arrange
        // Note: In a real scenario, we'd seed this data more robustly.
        // For this test, we'll assume sensor 1 exists from our factory setup.
        await _client.PostAsJsonAsync("/api/measurements/", new CreateMeasurementDto(DateTime.UtcNow.AddHours(-1).ToString("o"), 1, 10));
        await _client.PostAsJsonAsync("/api/measurements/", new CreateMeasurementDto(DateTime.UtcNow.ToString("o"), 1, 20));

        var startDate = DateTime.UtcNow.AddDays(-1).ToString("o");
        var endDate = DateTime.UtcNow.AddDays(1).ToString("o");

        // Act
        var response = await _client.GetAsync($"/api/measurements/aggregated-history?StartDate={startDate}&EndDate={endDate}&Granularity=Daily&SensorId=1");

        // Assert
        response.EnsureSuccessStatusCode();
        var aggregatedData = await response.Content.ReadFromJsonAsync<List<AggregatedMeasurement>>();

        Assert.NotNull(aggregatedData);
        Assert.Single(aggregatedData);
        Assert.Equal(15, aggregatedData[0].AverageTemperatureCelsius, 1); // Allow for minor floating point differences
    }
}