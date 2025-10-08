namespace Domain.Mappers;

using Entities;
using Records;

public static class SensorMapper {
  public static void UpdateDefinition(this Sensor sensor, SensorDefinitionUpdateData data) {
    sensor.DisplayName = data.DisplayName;
    sensor.DeviceAddress = data.DeviceAddress;
  }

  public static void UpdateState(this Sensor sensor, SensorStateUpdateData data) {
    sensor.State = data.State;
  }
}
