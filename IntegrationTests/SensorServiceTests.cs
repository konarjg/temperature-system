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
    public async Task GetAllAsync_ShouldReturnAllSensorsInDatabase()
    {
        // Arrange
        var sensor1 = new Sensor { DeviceAddress = "addr1", DisplayName = "Sensor One" };
        var sensor2 = new Sensor { DeviceAddress = "addr2", DisplayName = "Sensor Two" };
        await DbContext.Sensors.AddRangeAsync(sensor1, sensor2);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _sensorService.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, s => s.DeviceAddress == "addr1");
        Assert.Contains(result, s => s.DeviceAddress == "addr2");
    }

    [Fact]
    public async Task SyncSensorsAsync_ShouldAddNewSensorsToDatabase()
    {
        // Arrange
        var definitions = new List<SensorDefinition>
        {
            new SensorDefinition("new_addr1", "New Sensor One"),
            new SensorDefinition("new_addr2", "New Sensor Two")
        };

        // Act
        await _sensorService.SyncSensorsAsync(definitions);

        // Assert
        var sensorsInDb = await DbContext.Sensors.ToListAsync();
        Assert.Equal(2, sensorsInDb.Count);
        Assert.Contains(sensorsInDb, s => s.DeviceAddress == "new_addr1" && s.DisplayName == "New Sensor One");
        Assert.Contains(sensorsInDb, s => s.DeviceAddress == "new_addr2" && s.DisplayName == "New Sensor Two");
    }

    [Fact]
    public async Task SyncSensorsAsync_ShouldRemoveMissingSensorsFromDatabase()
    {
        // Arrange
        var sensorToRemove = new Sensor { DeviceAddress = "old_addr", DisplayName = "Old Sensor" };
        await DbContext.Sensors.AddAsync(sensorToRemove);
        await DbContext.SaveChangesAsync();

        var definitions = new List<SensorDefinition>(); // No definitions, so old_addr should be removed

        // Act
        await _sensorService.SyncSensorsAsync(definitions);

        // Assert
        var sensorsInDb = await DbContext.Sensors.ToListAsync();
        Assert.Empty(sensorsInDb);
    }

    [Fact]
    public async Task SyncSensorsAsync_ShouldUpdateExistingSensorsInDatabase()
    {
        // Arrange
        var existingSensor = new Sensor { DeviceAddress = "update_addr", DisplayName = "Old Name" };
        await DbContext.Sensors.AddAsync(existingSensor);
        await DbContext.SaveChangesAsync();

        var definitions = new List<SensorDefinition>
        {
            new SensorDefinition("update_addr", "Updated Name")
        };

        // Act
        await _sensorService.SyncSensorsAsync(definitions);

        // Assert
        var sensorInDb = await DbContext.Sensors.SingleOrDefaultAsync(s => s.DeviceAddress == "update_addr");
        Assert.NotNull(sensorInDb);
        Assert.Equal("Updated Name", sensorInDb.DisplayName);
    }

    [Fact]
    public async Task SyncSensorsAsync_ShouldHandleMixedOperationsCorrectly()
    {
        // Arrange
        var existingSensorToUpdate = new Sensor { DeviceAddress = "addr1", DisplayName = "Old Name 1" };
        var existingSensorToRemove = new Sensor { DeviceAddress = "addr2", DisplayName = "Old Name 2" };
        await DbContext.Sensors.AddRangeAsync(existingSensorToUpdate, existingSensorToRemove);
        await DbContext.SaveChangesAsync();

        var definitions = new List<SensorDefinition>
        {
            new SensorDefinition("addr1", "Updated Name 1"), // Update existing
            new SensorDefinition("addr3", "New Sensor 3")    // Add new
        };

        // Act
        await _sensorService.SyncSensorsAsync(definitions);

        // Assert
        var sensorsInDb = await DbContext.Sensors.ToListAsync();
        Assert.Equal(2, sensorsInDb.Count);

        // Verify update
        var updatedSensor = sensorsInDb.SingleOrDefault(s => s.DeviceAddress == "addr1");
        Assert.NotNull(updatedSensor);
        Assert.Equal("Updated Name 1", updatedSensor.DisplayName);

        // Verify new sensor added
        var newSensor = sensorsInDb.SingleOrDefault(s => s.DeviceAddress == "addr3");
        Assert.NotNull(newSensor);
        Assert.Equal("New Sensor 3", newSensor.DisplayName);

        // Verify old sensor removed
        Assert.DoesNotContain(sensorsInDb, s => s.DeviceAddress == "addr2");
    }
}