using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Repositories;
using Domain.Services;
using Domain.Services.Util;
using Moq;
using Xunit;

namespace UnitTests;

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
    public async Task GetAllAsync_ShouldReturnAllSensorsFromRepository()
    {
        // Arrange
            var expectedSensors = new List<Sensor> { new Sensor {
                DeviceAddress = "test",
                DisplayName = "Living Room"
            }, new Sensor {
                DeviceAddress = "test2",
                DisplayName = "Bedroom"
            }
        };
        _sensorRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(expectedSensors);

        // Act
        var result = await _sensorService.GetAllAsync();

        // Assert
        Assert.Equal(expectedSensors, result);
        _sensorRepositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task SyncSensorsAsync_ShouldAddNewSensors()
    {
        // Arrange
        var definitions = new List<SensorDefinition> { new SensorDefinition("Address1", "Sensor1") };
        var existingSensors = new List<Sensor>();
        _sensorRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(existingSensors);

        // Act
        await _sensorService.SyncSensorsAsync(definitions);

        // Assert
        _sensorRepositoryMock.Verify(r => r.AddRangeAsync(It.Is<List<Sensor>>(list => list.Count == 1 && list[0].DeviceAddress == "Address1")), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task SyncSensorsAsync_ShouldRemoveMissingSensors()
    {
        // Arrange
        var definitions = new List<SensorDefinition>();
        var existingSensors = new List<Sensor> { new Sensor {
                DeviceAddress = "Address1",
                DisplayName = "Living Room"
            }
        };
        _sensorRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(existingSensors);

        // Act
        await _sensorService.SyncSensorsAsync(definitions);

        // Assert
        _sensorRepositoryMock.Verify(r => r.RemoveRange(It.Is<List<Sensor>>(list => list.Count == 1 && list[0].DeviceAddress == "Address1")), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task SyncSensorsAsync_ShouldUpdateExistingSensors()
    {
        // Arrange
        var definitions = new List<SensorDefinition> { new SensorDefinition("Address1", "New Name") };
        var existingSensors = new List<Sensor> { new Sensor { DeviceAddress = "Address1", DisplayName = "Old Name" } };
        _sensorRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(existingSensors);

        // Act
        await _sensorService.SyncSensorsAsync(definitions);

        // Assert
        Assert.Equal("New Name", existingSensors.First().DisplayName);
        _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
    }
}