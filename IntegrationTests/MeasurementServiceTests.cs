using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Services.Interfaces;
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
}