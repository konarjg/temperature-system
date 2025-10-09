using Domain.Entities;
using Domain.Entities.Util;
using Domain.Services.Interfaces;
using ExternalServiceAdapters.TemperatureSensorReader;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.ExternalServiceAdapters {
  public class FakeTemperatureSensorReaderTests {
    private readonly Mock<ISensorService> _sensorServiceMock;
    private readonly FakeTemperatureSensorReader _fakeReader;

    public FakeTemperatureSensorReaderTests() {
      _sensorServiceMock = new Mock<ISensorService>();
      _fakeReader = new FakeTemperatureSensorReader(_sensorServiceMock.Object);
    }

    [Fact]
    public async Task ReadAsync_ShouldFetchSensorsFromService() {
      // Arrange
      List<Sensor> sensors = new() {
        new Sensor { Id = 1, DisplayName = "Sensor 1", DeviceAddress = "addr1", State = SensorState.Operational }
      };
      _sensorServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(sensors);

      // Act
      List<Measurement> result = await _fakeReader.ReadAsync();

      // Assert
      // Verify that the service was called to get the list of sensors
      _sensorServiceMock.Verify(s => s.GetAllAsync(), Times.Once);

      // We can also assert that a measurement was generated for the sensor
      Assert.Single(result);
      Assert.Equal(1, result[0].SensorId);
    }
  }
}