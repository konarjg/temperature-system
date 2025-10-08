using Domain.Entities;
using Domain.Repositories;
using Domain.Services;
using Domain.Services.Util;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UnitTests {
  public class MeasurementServiceTests {
    private readonly Mock<IMeasurementRepository> _measurementRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly MeasurementService _measurementService;

    public MeasurementServiceTests() {
      _measurementRepositoryMock = new Mock<IMeasurementRepository>();
      _unitOfWorkMock = new Mock<IUnitOfWork>();
      _measurementService = new MeasurementService(_measurementRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task CreateRangeAsync_ShouldSucceed() {
      // Arrange
      List<Measurement> measurements = new() {
        new Measurement { Id = 1, TemperatureCelsius = 20.5f, Timestamp = System.DateTime.UtcNow, SensorId = 1 },
        new Measurement { Id = 2, TemperatureCelsius = 21.0f, Timestamp = System.DateTime.UtcNow, SensorId = 1 }
      };
      _unitOfWorkMock.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

      // Act
      bool result = await _measurementService.CreateRangeAsync(measurements);

      // Assert
      Assert.True(result);
      _measurementRepositoryMock.Verify(r => r.AddRangeAsync(measurements), Times.Once);
      _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteByIdAsync_WithExistingMeasurement_ShouldSucceed() {
      // Arrange
      long measurementId = 1L;
      Measurement measurement = new() { Id = measurementId, TemperatureCelsius = 25.0f, Timestamp = System.DateTime.UtcNow, SensorId = 1 };

      _measurementRepositoryMock.Setup(r => r.GetByIdAsync(measurementId)).ReturnsAsync(measurement);
      _unitOfWorkMock.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

      // Act
      OperationResult result = await _measurementService.DeleteByIdAsync(measurementId);

      // Assert
      Assert.Equal(OperationResult.Success, result);
      _measurementRepositoryMock.Verify(r => r.Remove(measurement), Times.Once);
      _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteByIdAsync_WithNonExistentMeasurement_ShouldReturnNotFound() {
      // Arrange
      long measurementId = 99L;
      _measurementRepositoryMock.Setup(r => r.GetByIdAsync(measurementId)).ReturnsAsync((Measurement?)null);

      // Act
      OperationResult result = await _measurementService.DeleteByIdAsync(measurementId);

      // Assert
      Assert.Equal(OperationResult.NotFound, result);
      _measurementRepositoryMock.Verify(r => r.Remove(It.IsAny<Measurement>()), Times.Never);
      _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Never);
    }
  }
}