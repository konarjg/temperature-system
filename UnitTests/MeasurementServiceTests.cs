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
    public async Task GetHistoryForSensorAsync_ShouldCallRepositoryWithCorrectParameters() {
        // Arrange
        long sensorId = 456;
        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow;
        var expectedHistory = new List<Measurement> { new Measurement { TemperatureCelsius = 25f, Timestamp = DateTime.Now, SensorId = sensorId } };
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
    public async Task CreateRangeAsync_ShouldCallRepositoryAndUnitOfWork() {
        // Arrange
        var measurements = new List<Measurement> {
            new Measurement { TemperatureCelsius = 20, Timestamp = DateTime.UtcNow, SensorId = 1 },
            new Measurement { TemperatureCelsius = 21, Timestamp = DateTime.UtcNow, SensorId = 2 }
        };
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(2);

        // Act
        var result = await _measurementService.CreateRangeAsync(measurements);

        // Assert
        Assert.True(result);
        _measurementRepositoryMock.Verify(r => r.AddRangeAsync(measurements), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnMeasurement_WhenMeasurementExists()
    {
        // Arrange
        var measurement = new Measurement { Id = 1, TemperatureCelsius = 25, Timestamp = DateTime.UtcNow, SensorId = 1 };
        _measurementRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(measurement);

        // Act
        var result = await _measurementService.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        _measurementRepositoryMock.Verify(r => r.GetByIdAsync(1), Times.Once);
    }

    [Fact]
    public async Task GetHistoryAsync_ShouldReturnAllMeasurements()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow;
        var measurements = new List<Measurement>
        {
            new Measurement { Id = 1, TemperatureCelsius = 25, Timestamp = DateTime.UtcNow, SensorId = 1 },
            new Measurement { Id = 2, TemperatureCelsius = 26, Timestamp = DateTime.UtcNow, SensorId = 2 }
        };
        _measurementRepositoryMock.Setup(r => r.GetHistoryAsync(startDate, endDate)).ReturnsAsync(measurements);

        // Act
        var result = await _measurementService.GetHistoryAsync(startDate, endDate);

        // Assert
        Assert.Equal(2, result.Count);
        _measurementRepositoryMock.Verify(r => r.GetHistoryAsync(startDate, endDate), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnTrue_WhenMeasurementIsCreated()
    {
        // Arrange
        var measurement = new Measurement { Id = 1, TemperatureCelsius = 25, Timestamp = DateTime.UtcNow, SensorId = 1 };
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        // Act
        var result = await _measurementService.CreateAsync(measurement);

        // Assert
        Assert.True(result);
        _measurementRepositoryMock.Verify(r => r.AddAsync(measurement), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteByIdAsync_ShouldReturnTrue_WhenMeasurementIsDeleted()
    {
        // Arrange
        var measurement = new Measurement { Id = 1, TemperatureCelsius = 25, Timestamp = DateTime.UtcNow, SensorId = 1 };
        _measurementRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(measurement);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        // Act
        var result = await _measurementService.DeleteByIdAsync(1);

        // Assert
        Assert.True(result);
        _measurementRepositoryMock.Verify(r => r.GetByIdAsync(1), Times.Once);
        _measurementRepositoryMock.Verify(r => r.Remove(measurement), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }
}
