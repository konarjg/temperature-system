using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Services.Interfaces;
using Domain.Services.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IntegrationTests;

public class SensorServiceTests : BaseServiceTests
{
    private readonly ISensorService _sensorService;

    public SensorServiceTests()
    {
        _sensorService = ServiceProvider.GetRequiredService<ISensorService>();
    }

    [Fact]
    public async Task SyncSensorsAsync_ShouldAddNewSensorsToDatabase()
    {
        // Arrange
        var definitions = new List<SensorDefinition>
        {
            new("New Sensor 1", "address1"),
            new("New Sensor 2", "address2")
        };

        // Act
        await _sensorService.SyncSensorsAsync(definitions);

        // Assert
        var sensors = await DbContext.Sensors.AsNoTracking().ToListAsync();
        Assert.Equal(2, sensors.Count);
        Assert.True(sensors.Any(s => s.DeviceAddress == "address1"));
        Assert.True(sensors.Any(s => s.DeviceAddress == "address2"));
    }

    [Fact]
    public async Task SyncSensorsAsync_ShouldRemoveOrphanedSensorsFromDatabase()
    {
        // Arrange
        var initialSensors = new List<Sensor>
        {
            new() { DisplayName = "Keep Sensor", DeviceAddress = "keep-address" },
            new() { DisplayName = "Remove Sensor", DeviceAddress = "remove-address" }
        };
        await DbContext.Sensors.AddRangeAsync(initialSensors);
        await DbContext.SaveChangesAsync();

        var definitions = new List<SensorDefinition>
        {
            new("Keep Sensor", "keep-address")
        };

        // Act
        await _sensorService.SyncSensorsAsync(definitions);

        // Assert
        var sensors = await DbContext.Sensors.AsNoTracking().ToListAsync();
        Assert.Single(sensors);
        Assert.Equal("keep-address", sensors.First().DeviceAddress);
    }

    [Fact]
    public async Task SyncSensorsAsync_ShouldUpdateExistingSensorDisplayName()
    {
        // Arrange
        var sensor = new Sensor { DisplayName = "Old Name", DeviceAddress = "update-address" };
        await DbContext.Sensors.AddAsync(sensor);
        await DbContext.SaveChangesAsync();

        var definitions = new List<SensorDefinition>
        {
            new("New Name", "update-address")
        };

        // Act
        await _sensorService.SyncSensorsAsync(definitions);

        // Assert
        var updatedSensor = await DbContext.Sensors.AsNoTracking().SingleAsync(s => s.DeviceAddress == "update-address");
        Assert.Equal("New Name", updatedSensor.DisplayName);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllSensorsFromDatabase()
    {
        // Arrange
        var sensorsToAdd = new List<Sensor>
        {
            new() { DisplayName = "Sensor A", DeviceAddress = "addressA" },
            new() { DisplayName = "Sensor B", DeviceAddress = "addressB" }
        };
        await DbContext.Sensors.AddRangeAsync(sensorsToAdd);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _sensorService.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);
    }
}