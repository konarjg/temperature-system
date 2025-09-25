namespace Domain.Entities;

public class Measurement {
  public long Id { get; set; }
  public required DateTime Timestamp { get; set; }
  public required float TemperatureCelsius { get; set; }
  public Sensor? Sensor { get; set; }
  public required long SensorId { get; set; }
}
