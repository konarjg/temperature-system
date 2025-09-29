using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Records;
using Domain.Services.Interfaces;
using Domain.Services.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IntegrationTests;

public class MeasurementServiceTests : BaseServiceTests {
    private readonly IMeasurementService _measurementService;

    public MeasurementServiceTests() {
        _measurementService = ServiceProvider.GetRequiredService<IMeasurementService>();
    }

    [Fact]
    public async Task CreateAsync_ShouldAddMeasurementToDatabase() {
        // Arrange
        var measurement = new Measurement { TemperatureCelsius = 25.5f, Timestamp = DateTime.UtcNow, SensorId = 1 };

        // Act
        var result = await _measurementService.CreateAsync(measurement);

        // Assert
        Assert.True(result);
        var savedMeasurement = await DbContext.Measurements.AsNoTracking().SingleOrDefaultAsync(m => m.Id == measurement.Id);
        Assert.NotNull(savedMeasurement);
        Assert.Equal(1, savedMeasurement.SensorId);
    }

    [Fact]
    public async Task CreateRangeAsync_ShouldAddMultipleMeasurementsToDatabase() {
        // Arrange
        var measurements = new List<Measurement> {
            new Measurement { TemperatureCelsius = 20, Timestamp = DateTime.UtcNow, SensorId = 1 },
            new Measurement { TemperatureCelsius = 22, Timestamp = DateTime.UtcNow, SensorId = 2 }
        };

        // Act
        var result = await _measurementService.CreateRangeAsync(measurements);

        // Assert
        Assert.True(result);
        Assert.Equal(2, await DbContext.Measurements.CountAsync());
    }
    
    [Fact]
    public async Task GetByIdAsync_ShouldReturnCorrectMeasurement()
    {
        // Arrange
        var measurement = new Measurement { TemperatureCelsius = 25.5f, Timestamp = DateTime.UtcNow, SensorId = 1 };
        await _measurementService.CreateAsync(measurement);

        // Act
        var result = await _measurementService.GetByIdAsync(measurement.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(measurement.Id, result.Id);
        Assert.Equal(measurement.TemperatureCelsius, result.TemperatureCelsius);
    }

    [Fact]
    public async Task GetLatestAsync_ShouldReturnTheMostRecentMeasurementForASpecificSensor() {
        // Arrange
        long targetSensorId = 1;
        await _measurementService.CreateAsync(new Measurement { TemperatureCelsius = 20, Timestamp = DateTime.UtcNow.AddHours(-2), SensorId = targetSensorId });
        var latestForTarget = new Measurement { TemperatureCelsius = 22, Timestamp = DateTime.UtcNow.AddHours(-1), SensorId = targetSensorId };
        await _measurementService.CreateAsync(latestForTarget);
        await _measurementService.CreateAsync(new Measurement { TemperatureCelsius = 99, Timestamp = DateTime.UtcNow, SensorId = 2 }); // Measurement for another sensor

        // Act
        var result = await _measurementService.GetLatestAsync(targetSensorId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(latestForTarget.Id, result.Id);
        Assert.Equal(targetSensorId, result.SensorId);
    }
    
    [Fact]
    public async Task GetHistoryAsync_ShouldReturnAllMeasurementsWithinDateRange()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow;
        await _measurementService.CreateAsync(new Measurement { TemperatureCelsius = 18, Timestamp = startDate.AddHours(-1), SensorId = 1 }); // Before range
        await _measurementService.CreateAsync(new Measurement { TemperatureCelsius = 19, Timestamp = startDate.AddHours(1), SensorId = 1 }); // In range
        await _measurementService.CreateAsync(new Measurement { TemperatureCelsius = 20, Timestamp = startDate.AddHours(2), SensorId = 2 }); // In range
        await _measurementService.CreateAsync(new Measurement { TemperatureCelsius = 21, Timestamp = endDate.AddHours(1), SensorId = 1 });   // After range

        // Act
        var result = await _measurementService.GetHistoryAsync(startDate, endDate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetHistoryForSensorAsync_ShouldReturnOnlyMeasurementsForThatSensor() {
        // Arrange
        long targetSensorId = 1;
        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow;
        await _measurementService.CreateAsync(new Measurement { TemperatureCelsius = 18, Timestamp = startDate.AddHours(1), SensorId = targetSensorId });
        await _measurementService.CreateAsync(new Measurement { TemperatureCelsius = 19, Timestamp = startDate.AddHours(2), SensorId = targetSensorId });
        await _measurementService.CreateAsync(new Measurement { TemperatureCelsius = 99, Timestamp = startDate.AddHours(3), SensorId = 2 }); // Other sensor

        // Act
        var result = await _measurementService.GetHistoryForSensorAsync(startDate, endDate, targetSensorId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, m => Assert.Equal(targetSensorId, m.SensorId));
    }
    
    [Fact]
    public async Task GetAggregatedHistoryForSensorAsync_ShouldReturnCorrectlyAggregatedData()
    {
        // Arrange
        long sensorId = 1;
        var startDate = DateTime.UtcNow.Date.AddDays(-2);
        var endDate = DateTime.UtcNow.Date;
        
        // Day 1: 2 measurements
        await _measurementService.CreateAsync(new Measurement { TemperatureCelsius = 10, Timestamp = startDate.AddHours(1), SensorId = sensorId });
        await _measurementService.CreateAsync(new Measurement { TemperatureCelsius = 20, Timestamp = startDate.AddHours(2), SensorId = sensorId });
        // Day 2: 1 measurement
        await _measurementService.CreateAsync(new Measurement { TemperatureCelsius = 30, Timestamp = startDate.AddDays(1).AddHours(1), SensorId = sensorId });
        // Other sensor
        await _measurementService.CreateAsync(new Measurement { TemperatureCelsius = 99, Timestamp = startDate.AddHours(3), SensorId = 2 });

        // Act
        var result = await _measurementService.GetAggregatedHistoryForSensorAsync(startDate, endDate, MeasurementHistoryGranularity.Daily, sensorId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        
        var firstDayAggregate = result.Single(r => r.TimeStamp == startDate);
        Assert.Equal(15, firstDayAggregate.AverageTemperatureCelsius, 1); 

        var secondDayAggregate = result.Single(r => r.TimeStamp == startDate.AddDays(1));
        Assert.Equal(30, secondDayAggregate.AverageTemperatureCelsius, 1);
    }
    
    [Fact]
    public async Task DeleteByIdAsync_WhenMeasurementExists_ShouldRemoveFromDatabase()
    {
        // Arrange
        var measurement = new Measurement { TemperatureCelsius = 25.5f, Timestamp = DateTime.UtcNow, SensorId = 1 };
        await _measurementService.CreateAsync(measurement);
        Assert.Equal(1, await DbContext.Measurements.CountAsync());

        // Act
        var result = await _measurementService.DeleteByIdAsync(measurement.Id);

        // Assert
        Assert.True(result);
        Assert.Equal(0, await DbContext.Measurements.CountAsync());
    }

    [Fact]
    public async Task DeleteByIdAsync_WhenMeasurementDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        long nonExistentId = 999;

        // Act
        var result = await _measurementService.DeleteByIdAsync(nonExistentId);

        // Assert
        Assert.False(result);
    }
}
