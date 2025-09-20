using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Services.Interfaces;
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
    public async Task GetByIdAsync_ShouldReturnMeasurement_WhenMeasurementExists() {
        var measurement = new Measurement { Id = 1, TemperatureCelsius = 25, Timestamp = DateTime.UtcNow };
        _measurementRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(measurement);

        var result = await _measurementService.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(25, result.TemperatureCelsius);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenMeasurementDoesNotExist() {
        _measurementRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Measurement)null);

        var result = await _measurementService.GetByIdAsync(1);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetLatestAsync_ShouldReturnMeasurement_WhenMeasurementExists() {
        var measurement = new Measurement { Id = 1, TemperatureCelsius = 25, Timestamp = DateTime.UtcNow };
        _measurementRepositoryMock.Setup(r => r.GetLatestAsync()).ReturnsAsync(measurement);

        var result = await _measurementService.GetLatestAsync();

        Assert.NotNull(result);
        Assert.Equal(25, result.TemperatureCelsius);
    }

    [Fact]
    public async Task GetLatestAsync_ShouldReturnNull_WhenNoMeasurementsExist() {
        _measurementRepositoryMock.Setup(r => r.GetLatestAsync()).ReturnsAsync((Measurement)null);

        var result = await _measurementService.GetLatestAsync();

        Assert.Null(result);
    }

    [Fact]
    public async Task GetHistoryAsync_ShouldReturnListOfMeasurements() {
        var measurements = new List<Measurement> {
            new Measurement { Id = 1, TemperatureCelsius = 20, Timestamp = DateTime.UtcNow },
            new Measurement { Id = 2, TemperatureCelsius = 22, Timestamp = DateTime.UtcNow.AddHours(-1) }
        };
        _measurementRepositoryMock.Setup(r => r.GetHistoryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(measurements);

        var result = await _measurementService.GetHistoryAsync(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);

        Assert.Equal(2, result.Count);
    }

    [Theory]
    [InlineData(MeasurementHistoryGranularity.Hourly)]
    [InlineData(MeasurementHistoryGranularity.Daily)]
    [InlineData(MeasurementHistoryGranularity.Monthly)]
    public async Task GetAggregatedHistoryAsync_ShouldReturnAggregatedMeasurements(MeasurementHistoryGranularity granularity) {
        var aggregatedMeasurements = new List<AggregatedMeasurement> {
            new AggregatedMeasurement(DateTime.UtcNow, 25.5f),
            new AggregatedMeasurement(DateTime.UtcNow.AddHours(-1), 24.8f)
        };
        _measurementRepositoryMock.Setup(r => r.GetAggregatedHistoryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), granularity)).ReturnsAsync(aggregatedMeasurements);

        var result = await _measurementService.GetAggregatedHistoryAsync(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow, granularity);

        Assert.Equal(2, result.Count);
        Assert.Equal(25.5f, result[0].AverageTemperatureCelsius);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnTrue_WhenCreationIsSuccessful() {
        var measurement = new Measurement { TemperatureCelsius = 25, Timestamp = DateTime.UtcNow };
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1L);

        var result = await _measurementService.CreateAsync(measurement);

        _measurementRepositoryMock.Verify(r => r.AddAsync(measurement), Times.Once);
        Assert.True(result);
    }
    
    [Fact]
    public async Task CreateAsync_ShouldReturnFalse_WhenCreationFails() {
        var measurement = new Measurement { TemperatureCelsius = 25, Timestamp = DateTime.UtcNow };
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(0L);

        var result = await _measurementService.CreateAsync(measurement);

        _measurementRepositoryMock.Verify(r => r.AddAsync(measurement), Times.Once);
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteByIdAsync_ShouldReturnTrue_WhenDeletionIsSuccessful() {
        var measurement = new Measurement { Id = 1, TemperatureCelsius = 25, Timestamp = DateTime.UtcNow };
        _measurementRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(measurement);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1L);

        var result = await _measurementService.DeleteByIdAsync(1);

        _measurementRepositoryMock.Verify(r => r.Remove(measurement), Times.Once);
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteByIdAsync_ShouldReturnFalse_WhenMeasurementDoesNotExist() {
        _measurementRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Measurement)null);

        var result = await _measurementService.DeleteByIdAsync(1);

        Assert.False(result);
        _measurementRepositoryMock.Verify(r => r.Remove(It.IsAny<Measurement>()), Times.Never);
    }
}
