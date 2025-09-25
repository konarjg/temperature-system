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
    public async Task SyncSensorsAsync_ShouldAddNewSensors_WhenNewDefinitionsAreProvided()
    {
        // Arrange
        var definitions = new List<SensorDefinition> { new("New Sensor", "new-address") };
        _sensorRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Sensor>());
        _unitOfWorkMock.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

        // Act
        await _sensorService.SyncSensorsAsync(definitions);

        // Assert
        _sensorRepositoryMock.Verify(r => r.AddRangeAsync(It.Is<List<Sensor>>(list => list.Count == 1 && list[0].DeviceAddress == "new-address")), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task SyncSensorsAsync_ShouldRemoveOrphanedSensors_WhenDefinitionsAreRemoved()
    {
        // Arrange
        var existingSensors = new List<Sensor> { new() { DisplayName = "Old Sensor", DeviceAddress = "old-address" } };
        _sensorRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(existingSensors);
        _unitOfWorkMock.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

        // Act
        await _sensorService.SyncSensorsAsync(new List<SensorDefinition>());

        // Assert
        _sensorRepositoryMock.Verify(r => r.RemoveRange(It.Is<List<Sensor>>(list => list.Count == 1 && list[0].DeviceAddress == "old-address")), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task SyncSensorsAsync_ShouldUpdateExistingSensors_WhenDisplayNameChanges()
    {
        // Arrange
        var definitions = new List<SensorDefinition> { new("Updated Sensor", "existing-address") };
        var existingSensors = new List<Sensor> { new() { DisplayName = "Original Sensor", DeviceAddress = "existing-address" } };
        _sensorRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(existingSensors);
        _unitOfWorkMock.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

        // Act
        await _sensorService.SyncSensorsAsync(definitions);

        // Assert
        Assert.Equal("Updated Sensor", existingSensors.First().DisplayName);
        _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllSensorsFromRepository()
    {
        // Arrange
        var expectedSensors = new List<Sensor> { new() { DisplayName = "Sensor 1", DeviceAddress = "address1" } };
        _sensorRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(expectedSensors);

        // Act
        var result = await _sensorService.GetAllAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("Sensor 1", result.First().DisplayName);
        _sensorRepositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
    }
}