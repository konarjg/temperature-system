namespace TemperatureSystem.Mappers;

using Domain.Entities;
using Domain.Entities.Util;
using Domain.Records;
using Dto;

public static class SensorMapper {
  public static SensorDto ToDto(this Sensor sensor) {
    return new SensorDto(sensor.Id,sensor.DisplayName,sensor.State);
  }

  public static Sensor ToEntity(this SensorRequest data) {
    return new Sensor() {
      DisplayName = data.DisplayName,
      DeviceAddress = data.DeviceAddress,
      State = SensorState.Unavailable
    };
  }

  public static SensorDefinitionUpdateData ToDomainDto(this SensorRequest data) {
    return new SensorDefinitionUpdateData(data.DisplayName, data.DeviceAddress);
  }
}
