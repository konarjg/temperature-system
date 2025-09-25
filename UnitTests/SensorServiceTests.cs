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
        public async Task SyncSensorsAsync_ShouldAddNewSensors()
        {
            // Arrange
            var definitions = new List<SensorDefinition> { new("Sensor 3", "address-3") };
            var existingSensors = new List<Sensor> { new() { DeviceAddress = "address-1", DisplayName = "Sensor 1" } };
            var addedSensors = new List<Sensor>();
            _sensorRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(existingSensors);
            _sensorRepositoryMock.Setup(r => r.AddRangeAsync(It.IsAny<List<Sensor>>()))
                .Callback<List<Sensor>>(s => addedSensors.AddRange(s));

            // Act
            await _sensorService.SyncSensorsAsync(definitions);

            // Assert
            Assert.Single(addedSensors);
            Assert.Equal("address-3", addedSensors[0].DeviceAddress);
            Assert.Equal("Sensor 3", addedSensors[0].DisplayName);
        }

        [Fact]
        public async Task SyncSensorsAsync_ShouldUpdateExistingSensors()
        {
            // Arrange
            var definitions = new List<SensorDefinition> { new("New Name", "address-1") };
            var existingSensor = new Sensor { DeviceAddress = "address-1", DisplayName = "Old Name" };
            var existingSensors = new List<Sensor> { existingSensor };
            _sensorRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(existingSensors);

            // Act
            await _sensorService.SyncSensorsAsync(definitions);

            // Assert
            Assert.Equal("New Name", existingSensor.DisplayName);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task SyncSensorsAsync_ShouldRemoveOrphanedSensors()
        {
            // Arrange
            var definitions = new List<SensorDefinition> { new("Sensor 1", "address-1") };
            var orphanedSensor = new Sensor { DeviceAddress = "address-2", DisplayName = "Sensor 2" };
            var existingSensors = new List<Sensor> { new() { DeviceAddress = "address-1", DisplayName = "Sensor 1" }, orphanedSensor };
            var removedSensors = new List<Sensor>();
            _sensorRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(existingSensors);
            _sensorRepositoryMock.Setup(r => r.RemoveRange(It.IsAny<List<Sensor>>()))
                .Callback<List<Sensor>>(s => removedSensors.AddRange(s));

            // Act
            await _sensorService.SyncSensorsAsync(definitions);

            // Assert
            Assert.Single(removedSensors);
            Assert.Equal("address-2", removedSensors[0].DeviceAddress);
        }
    }
}