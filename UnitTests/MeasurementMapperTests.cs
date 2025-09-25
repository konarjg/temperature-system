using System;
using Domain.Entities;
using TemperatureSystem.Mappers;
using Xunit;

namespace UnitTests;

public class MeasurementMapperTests
{
    [Fact]
    public void ToDto_ShouldMapEntityToDtoCorrectly()
    {
        // Arrange
        var entity = new Measurement
        {
            Id = 1,
            TemperatureCelsius = 25.5f,
            Timestamp = DateTime.UtcNow,
            SensorId = 101
        };

        // Act
        var dto = MeasurementMapper.ToDto(entity);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(entity.Id, dto.Id);
        Assert.Equal(entity.TemperatureCelsius, dto.TemperatureCelsius);
        Assert.Equal(entity.Timestamp, dto.Timestamp);
        Assert.Equal(entity.SensorId, dto.SensorId);
    }
}