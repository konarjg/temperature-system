namespace Domain.Entities;

public class Sensor {
  public long Id { get; set; }
  public required String DeviceAddress { get; set; }
  public required String DisplayName { get; set; }
}
