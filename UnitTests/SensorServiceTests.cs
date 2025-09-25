using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Repositories;
using Domain.Services;
using Domain.Services.Util;
using Moq;
using Xunit;

namespace UnitTests
{
    public class SensorServiceTests
    {
        private readonly Mock<ISensorRepository> _sensorRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly SensorService _sensorService;

        public SensorServiceTests()
        {
            _sensorRepositoryMock = new Mock<ISensorRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _sensorService = new SensorService(_sensorRepositoryMock.Object, _unitOfWorkMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllSensors()
        {
            // Arrange
            var sensors = new List<Sensor>
            {
                new Sensor { Id = 1, DisplayName = "Sensor 1", DeviceAddress = "Address 1" },
                new Sensor { Id = 2, DisplayName = "Sensor 2", DeviceAddress = "Address 2" }
            };
            _sensorRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(sensors);

            // Act
            var result = await _sensorService.GetAllAsync();

            // Assert
            Assert.Equal(2, result.Count);
            _sensorRepositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task SyncSensorsAsync_ShouldAddUpdateAndRemoveSensors()
        {
            // Arrange
            var definitions = new List<SensorDefinition>
            {
                new("Address 1", "New Sensor 1"),
                new("Address 3", "Sensor 3")
            };
            var existingSensors = new List<Sensor>
            {
                new() { Id = 1, DisplayName = "Sensor 1", DeviceAddress = "Address 1" },
                new() { Id = 2, DisplayName = "Sensor 2", DeviceAddress = "Address 2" }
            };

            List<Sensor> addedSensors = new();
            List<Sensor> removedSensors = new();

            _sensorRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(existingSensors);
            _sensorRepositoryMock.Setup(r => r.AddRangeAsync(It.IsAny<List<Sensor>>()))
                .Callback<List<Sensor>>(list => addedSensors.AddRange(list));
            _sensorRepositoryMock.Setup(r => r.RemoveRange(It.IsAny<List<Sensor>>()))
                .Callback<List<Sensor>>(list => removedSensors.AddRange(list));

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // Act
            await _sensorService.SyncSensorsAsync(definitions);

            // Assert
            _sensorRepositoryMock.Verify(r => r.GetAllAsync(), Times.Once);

            Assert.Single(addedSensors);
            Assert.Equal("Address 3", addedSensors[0].DeviceAddress);

            Assert.Single(removedSensors);
            Assert.Equal("Address 2", removedSensors[0].DeviceAddress);

            Assert.Equal("New Sensor 1", existingSensors[0].DisplayName);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }
    }
}