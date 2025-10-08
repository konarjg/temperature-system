namespace TemperatureSystem.Dto;

using Domain.Entities.Util;

public record SensorDto(long Id, string DisplayName, SensorState State);
