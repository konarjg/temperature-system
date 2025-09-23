namespace TemperatureSystem.Mappers;

using Domain.Entities;
using Dto;

public static class SensorMapper {
  public static SensorDto ToDto(this Sensor sensor) {
    return new SensorDto(sensor.Id,sensor.DisplayName);
  }
}
