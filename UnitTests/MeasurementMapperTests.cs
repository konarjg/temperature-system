using System;
using Domain.Entities;
using TemperatureSystem.Dto;
using TemperatureSystem.Mappers;
using Xunit;

namespace UnitTests;

public class MeasurementMapperTests
{
    [Fact]
    public void ToDto_ShouldMapEntityToDtoCorrectly()
    {
        // Arrange
        Measurement entity = new()
        {
            Id = 1,
            TemperatureCelsius = 25.5f,
            Timestamp = DateTime.UtcNow,
            SensorId = 101
        };

        // Act
        MeasurementDto dto = MeasurementMapper.ToDto(entity);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(entity.Id, dto.Id);
        Assert.Equal(entity.TemperatureCelsius, dto.TemperatureCelsius);
        Assert.Equal(entity.Timestamp.ToString(System.Globalization.CultureInfo.InvariantCulture), dto.Timestamp);
        Assert.Equal(entity.SensorId, dto.SensorId);
    }
}