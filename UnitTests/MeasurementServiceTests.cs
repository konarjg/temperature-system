using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Records;
using Domain.Repositories;
using Domain.Services;
using Domain.Services.Util;
using Moq;
using Xunit;

namespace UnitTests;

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
    public async Task GetByIdAsync_ShouldCallRepositoryWithCorrectId()
    {
        // Arrange
        long measurementId = 1;
        var expectedMeasurement = new Measurement { Id = measurementId, TemperatureCelsius = 25, Timestamp = DateTime.UtcNow, SensorId = 123 };
        _measurementRepositoryMock.Setup(r => r.GetByIdAsync(measurementId)).ReturnsAsync(expectedMeasurement);

        // Act
        var result = await _measurementService.GetByIdAsync(measurementId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(measurementId, result.Id);
        _measurementRepositoryMock.Verify(r => r.GetByIdAsync(measurementId), Times.Once);
    }

    [Fact]
    public async Task GetLatestAsync_ShouldCallRepositoryWithCorrectSensorId() {
        // Arrange
        long sensorId = 123;
        var expectedMeasurement = new Measurement { Id = 1, TemperatureCelsius = 25, Timestamp = DateTime.UtcNow, SensorId = sensorId };
        _measurementRepositoryMock.Setup(r => r.GetLatestAsync(sensorId)).ReturnsAsync(expectedMeasurement);

        // Act
        var result = await _measurementService.GetLatestAsync(sensorId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(sensorId, result.SensorId);
        _measurementRepositoryMock.Verify(r => r.GetLatestAsync(sensorId), Times.Once);
    }

    [Fact]
    public async Task GetHistoryAsync_ShouldCallRepositoryWithCorrectParameters()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow;
        var expectedHistory = new List<Measurement> { new Measurement {
                SensorId = 123,
                Timestamp = default,
                TemperatureCelsius = 0
            }
        };
        _measurementRepositoryMock.Setup(r => r.GetHistoryAsync(startDate, endDate)).ReturnsAsync(expectedHistory);

        // Act
        var result = await _measurementService.GetHistoryAsync(startDate, endDate);

        // Assert
        Assert.Single(result);
        _measurementRepositoryMock.Verify(r => r.GetHistoryAsync(startDate, endDate), Times.Once);
    }

    [Fact]
    public async Task GetHistoryForSensorAsync_ShouldCallRepositoryWithCorrectParameters() {
        // Arrange
        long sensorId = 456;
        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow;
        var expectedHistory = new List<Measurement> { new Measurement {
                SensorId = sensorId,
                Timestamp = default,
                TemperatureCelsius = 0
            }
        };
        _measurementRepositoryMock.Setup(r => r.GetHistoryForSensorAsync(startDate, endDate, sensorId)).ReturnsAsync(expectedHistory);

        // Act
        var result = await _measurementService.GetHistoryForSensorAsync(startDate, endDate, sensorId);

        // Assert
        Assert.Single(result);
        Assert.Equal(sensorId, result[0].SensorId);
        _measurementRepositoryMock.Verify(r => r.GetHistoryForSensorAsync(startDate, endDate, sensorId), Times.Once);
    }

    [Fact]
    public async Task GetAggregatedHistoryForSensorAsync_ShouldCallRepositoryWithCorrectParameters() {
        // Arrange
        long sensorId = 789;
        var startDate = DateTime.UtcNow.AddMonths(-1);
        var endDate = DateTime.UtcNow;
        var granularity = MeasurementHistoryGranularity.Daily;
        var expectedAggregates = new List<AggregatedMeasurement> { new AggregatedMeasurement(DateTime.UtcNow, 22.5f) };
        _measurementRepositoryMock.Setup(r => r.GetAggregatedHistoryForSensorAsync(startDate, endDate, granularity, sensorId)).ReturnsAsync(expectedAggregates);

        // Act
        var result = await _measurementService.GetAggregatedHistoryForSensorAsync(startDate, endDate, granularity, sensorId);

        // Assert
        Assert.Single(result);
        _measurementRepositoryMock.Verify(r => r.GetAggregatedHistoryForSensorAsync(startDate, endDate, granularity, sensorId), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldCallRepositoryAndUnitOfWork()
    {
        // Arrange
        var measurement = new Measurement { TemperatureCelsius = 20, Timestamp = DateTime.UtcNow, SensorId = 1 };
        _measurementRepositoryMock.Setup(r => r.AddAsync(measurement)).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        // Act
        var result = await _measurementService.CreateAsync(measurement);

        // Assert
        Assert.True(result);
        _measurementRepositoryMock.Verify(r => r.AddAsync(measurement), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateRangeAsync_ShouldCallRepositoryAndUnitOfWork() {
        // Arrange
        var measurements = new List<Measurement> {
            new Measurement { TemperatureCelsius = 20, Timestamp = DateTime.UtcNow, SensorId = 1 },
            new Measurement { TemperatureCelsius = 21, Timestamp = DateTime.UtcNow, SensorId = 2 }
        };
        _measurementRepositoryMock.Setup(r => r.AddRangeAsync(measurements)).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(2);

        // Act
        var result = await _measurementService.CreateRangeAsync(measurements);

        // Assert
        Assert.True(result);
        _measurementRepositoryMock.Verify(r => r.AddRangeAsync(measurements), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }
    
    [Fact]
    public async Task DeleteByIdAsync_WhenMeasurementExists_ShouldRemoveAndSaveChangesAndReturnTrue()
    {
        // Arrange
        long measurementId = 1;
        var measurement = new Measurement {
            Id = measurementId,
            Timestamp = default,
            TemperatureCelsius = 0,
            SensorId = 0
        };
        _measurementRepositoryMock.Setup(r => r.GetByIdAsync(measurementId)).ReturnsAsync(measurement);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        // Act
        var result = await _measurementService.DeleteByIdAsync(measurementId);

        // Assert
        Assert.True(result);
        _measurementRepositoryMock.Verify(r => r.GetByIdAsync(measurementId), Times.Once);
        _measurementRepositoryMock.Verify(r => r.Remove(measurement), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteByIdAsync_WhenMeasurementDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        long measurementId = 1;
        _measurementRepositoryMock.Setup(r => r.GetByIdAsync(measurementId)).ReturnsAsync((Measurement)null);

        // Act
        var result = await _measurementService.DeleteByIdAsync(measurementId);

        // Assert
        Assert.False(result);
        _measurementRepositoryMock.Verify(r => r.GetByIdAsync(measurementId), Times.Once);
        _measurementRepositoryMock.Verify(r => r.Remove(It.IsAny<Measurement>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
    }
}
