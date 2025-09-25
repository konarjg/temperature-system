using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TemperatureSystem.Dto;
using Xunit;

namespace EndToEndTests;

public class SensorEndpointsTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public SensorEndpointsTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetSensors_ShouldReturnListOfSensors()
    {
        // Act
        var response = await _client.GetAsync("/sensors");

        // Assert
        response.EnsureSuccessStatusCode();
        var sensors = await response.Content.ReadFromJsonAsync<List<SensorDto>>();

        Assert.NotNull(sensors);
        Assert.Equal(2, sensors.Count);
        Assert.Contains(sensors, s => s.DeviceAddress == "test-sensor-1");
        Assert.Contains(sensors, s => s.DeviceAddress == "test-sensor-2");
    }
}