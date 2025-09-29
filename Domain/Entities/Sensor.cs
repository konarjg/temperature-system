namespace Domain.Entities;

public class Sensor {
  public long Id { get; set; }
  public required string DeviceAddress { get; set; }
  public required string DisplayName { get; set; }
}
