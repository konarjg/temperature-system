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
        Measurement measurement = new() { TemperatureCelsius = 25.5f, Timestamp = DateTime.UtcNow, SensorId = 1 };

        // Act
        bool result = await _measurementService.CreateAsync(measurement);

        // Assert
        Assert.True(result);
        Measurement? savedMeasurement = await DbContext.Measurements.AsNoTracking().SingleOrDefaultAsync(m => m.Id == measurement.Id);
        Assert.NotNull(savedMeasurement);
        Assert.Equal(1, savedMeasurement.SensorId);
    }

    [Fact]
    public async Task CreateRangeAsync_ShouldAddMultipleMeasurementsToDatabase() {
        // Arrange
        List<Measurement> measurements = new() {
            new() { TemperatureCelsius = 20, Timestamp = DateTime.UtcNow, SensorId = 1 },
            new() { TemperatureCelsius = 22, Timestamp = DateTime.UtcNow, SensorId = 2 }
        };

        // Act
        bool result = await _measurementService.CreateRangeAsync(measurements);

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
        Measurement latestForTarget = new() { TemperatureCelsius = 22, Timestamp = DateTime.UtcNow.AddHours(-1), SensorId = targetSensorId };
        await _measurementService.CreateAsync(latestForTarget);
        await _measurementService.CreateAsync(new Measurement { TemperatureCelsius = 99, Timestamp = DateTime.UtcNow, SensorId = 2 }); // Measurement for another sensor

        // Act
        Measurement? result = await _measurementService.GetLatestAsync(targetSensorId);

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
        DateTime startDate = DateTime.UtcNow.AddDays(-1);
        DateTime endDate = DateTime.UtcNow;
        await _measurementService.CreateAsync(new Measurement { TemperatureCelsius = 18, Timestamp = startDate.AddHours(1), SensorId = targetSensorId });
        await _measurementService.CreateAsync(new Measurement { TemperatureCelsius = 19, Timestamp = startDate.AddHours(2), SensorId = targetSensorId });
        await _measurementService.CreateAsync(new Measurement { TemperatureCelsius = 99, Timestamp = startDate.AddHours(3), SensorId = 2 }); // Other sensor

        // Act
        List<Measurement> result = await _measurementService.GetHistoryForSensorAsync(startDate, endDate, targetSensorId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, m => Assert.Equal(targetSensorId, m.SensorId));
    }
    [Fact]
    public async Task GetAggregatedHistoryForSensorAsync_ShouldReturnCorrectlyAggregatedData()
    {
        // Arrange
        long targetSensorId = 1;
        DateTime day1 = DateTime.UtcNow.AddDays(-2).Date;
        DateTime day2 = DateTime.UtcNow.AddDays(-1).Date;

        await _measurementService.CreateRangeAsync(new List<Measurement>
        {
            // Day 1 data
            new() { TemperatureCelsius = 10, Timestamp = day1.AddHours(1), SensorId = targetSensorId },
            new() { TemperatureCelsius = 20, Timestamp = day1.AddHours(2), SensorId = targetSensorId },
            // Day 2 data
            new() { TemperatureCelsius = 30, Timestamp = day2.AddHours(1), SensorId = targetSensorId },
            new() { TemperatureCelsius = 40, Timestamp = day2.AddHours(2), SensorId = targetSensorId },
            // Data for another sensor (should be ignored)
            new() { TemperatureCelsius = 100, Timestamp = day1.AddHours(1), SensorId = 2 }
        });

        // Act
        List<AggregatedMeasurement> result = await _measurementService.GetAggregatedHistoryForSensorAsync(day1, day2.AddHours(23), Domain.Services.Util.MeasurementHistoryGranularity.Daily, targetSensorId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        AggregatedMeasurement? day1Aggregate = result.SingleOrDefault(r => r.TimeStamp.Date == day1);
        AggregatedMeasurement? day2Aggregate = result.SingleOrDefault(r => r.TimeStamp.Date == day2);

        Assert.NotNull(day1Aggregate);
        Assert.Equal(15, day1Aggregate.AverageTemperatureCelsius); // Average of 10 and 20

        Assert.NotNull(day2Aggregate);
        Assert.Equal(35, day2Aggregate.AverageTemperatureCelsius); // Average of 30 and 40
    }
}
