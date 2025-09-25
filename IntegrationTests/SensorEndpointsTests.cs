using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TemperatureSystem.Dto;
using Xunit;

namespace IntegrationTests
{
    public class SensorEndpointsTests : BaseEndpointTests
    {
        [Fact]
        public async Task GetAll_ShouldReturnOk()
        {
            // Act
            var response = await Client.GetAsync("/api/sensors");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var sensors = await response.Content.ReadFromJsonAsync<List<SensorDto>>();
            Assert.NotNull(sensors);
            Assert.Empty(sensors);
        }
    }
}