using Domain.Entities;
using TemperatureSystem.Mappers;
using Xunit;

namespace UnitTests;

public class SensorMapperTests
{
    [Fact]
    public void ToDto_ShouldMapEntityToDtoCorrectly()
    {
        // Arrange
        var entity = new Sensor
        {
            Id = 1,
            DisplayName = "Living Room Sensor",
            DeviceAddress = "sensor-address-123"
        };

        // Act
        var dto = SensorMapper.ToDto(entity);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(entity.Id, dto.Id);
        Assert.Equal(entity.DisplayName, dto.DisplayName);
    }
}