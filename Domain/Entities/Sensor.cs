namespace Domain.Entities;

using Util;

public class Sensor {
  public long Id { get; set; }
  public required string DisplayName { get; set; }
  public required string DeviceAddress { get; set; }
  public required SensorState State { get; set; }
}
