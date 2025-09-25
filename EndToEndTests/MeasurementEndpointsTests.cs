using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
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
        var newMeasurement = new CreateMeasurementDto(25.5f, 1);

        // Act
        var response = await _client.PostAsJsonAsync("/measurements", newMeasurement);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetLatestMeasurement_ShouldReturnLatestMeasurementForSensor()
    {
        // Arrange
        var newMeasurement = new CreateMeasurementDto(30.0f, 1);
        await _client.PostAsJsonAsync("/measurements", newMeasurement);

        // Act
        var response = await _client.GetAsync("/measurements/latest/1");

        // Assert
        response.EnsureSuccessStatusCode();
        var measurement = await response.Content.ReadFromJsonAsync<MeasurementDto>();

        Assert.NotNull(measurement);
        Assert.Equal(30.0f, measurement.TemperatureCelsius);
        Assert.Equal(1, measurement.SensorId);
    }
}