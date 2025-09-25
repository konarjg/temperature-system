using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TemperatureSystem.Dto;
using Xunit;

namespace IntegrationTests
{
    public class MeasurementEndpointsTests : BaseEndpointTests
    {
        [Fact]
        public async Task GetLatest_ShouldReturnNotFound_WhenNoMeasurementsExist()
        {
            // Act
            var response = await Client.GetAsync("/api/measurements/latest?sensorId=1");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task PostMeasurement_ShouldReturnCreatedAt()
        {
            // Arrange
            var measurementDto = new CreateMeasurementDto(System.DateTime.UtcNow.ToString("o"), 1, 25);

            // Act
            var response = await Client.PostAsJsonAsync("/api/measurements", measurementDto);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var createdMeasurement = await response.Content.ReadFromJsonAsync<MeasurementDto>();
            Assert.NotNull(createdMeasurement);
            Assert.Equal(measurementDto.TemperatureCelsius, createdMeasurement.TemperatureCelsius);
        }

        [Fact]
        public async Task GetHistory_ShouldReturnOk()
        {
            // Arrange
            var measurementDto = new CreateMeasurementDto(System.DateTime.UtcNow.ToString("o"), 1, 25);
            await Client.PostAsJsonAsync("/api/measurements", measurementDto);

            // Act
            var response = await Client.GetAsync($"/api/measurements/history?startDate={System.DateTime.UtcNow.AddDays(-1):O}&endDate={System.DateTime.UtcNow.AddDays(1):O}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var history = await response.Content.ReadFromJsonAsync<List<MeasurementDto>>();
            Assert.NotNull(history);
            Assert.Single(history);
        }
    }
}