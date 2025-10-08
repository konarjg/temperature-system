using Domain.Entities;
using Domain.Records;
using Domain.Repositories;
using Domain.Services;
using Domain.Services.Util;
using Moq;

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
    public async Task DeleteByIdAsync_WithExistingSensor_ShouldSucceed()
    {
        // Arrange
        var sensorId = 1L;
        var sensor = new Sensor { Id = sensorId, DisplayName = "Test Sensor", DeviceAddress = "test_address", State = Domain.Entities.Util.SensorState.Operational };

        _sensorRepositoryMock.Setup(r => r.GetByIdAsync(sensorId)).ReturnsAsync(sensor);
        _unitOfWorkMock.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

        // Act
        var result = await _sensorService.DeleteByIdAsync(sensorId);

        // Assert
        Assert.Equal(OperationResult.Success, result);
        _sensorRepositoryMock.Verify(r => r.Remove(sensor), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteByIdAsync_WithNonExistentSensor_ShouldReturnNotFound()
    {
        // Arrange
        var sensorId = 99L;
        _sensorRepositoryMock.Setup(r => r.GetByIdAsync(sensorId)).ReturnsAsync((Sensor)null);

        // Act
        var result = await _sensorService.DeleteByIdAsync(sensorId);

        // Assert
        Assert.Equal(OperationResult.NotFound, result);
        _sensorRepositoryMock.Verify(r => r.Remove(It.IsAny<Sensor>()), Times.Never);
        _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldSucceed()
    {
        // Arrange
        var sensor = new Sensor { Id = 1L, DisplayName = "New Sensor", DeviceAddress = "new_address", State = Domain.Entities.Util.SensorState.Operational };
        _unitOfWorkMock.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

        // Act
        var result = await _sensorService.CreateAsync(sensor);

        // Assert
        Assert.True(result);
        _sensorRepositoryMock.Verify(r => r.AddAsync(sensor), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateDefinitionByIdAsync_WithExistingSensor_ShouldSucceed()
    {
        // Arrange
        var sensorId = 1L;
        var sensor = new Sensor { Id = sensorId, DisplayName = "Old Name", DeviceAddress = "old_address", State = Domain.Entities.Util.SensorState.Operational };
        var updateData = new SensorDefinitionUpdateData("New Name", "new_address");

        _sensorRepositoryMock.Setup(r => r.GetByIdAsync(sensorId)).ReturnsAsync(sensor);
        _unitOfWorkMock.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

        // Act
        var result = await _sensorService.UpdateDefinitionByIdAsync(sensorId, updateData);

        // Assert
        Assert.Equal(OperationResult.Success, result);
        Assert.Equal("New Name", sensor.DisplayName);
        Assert.Equal("new_address", sensor.DeviceAddress);
        _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
    }
}