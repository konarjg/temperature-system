namespace Domain.Services.External;

using Entities;
using Entities.Util;

public interface ITemperatureSensorReader {
  public delegate void OnMeasurementPerformed(Measurement measurement);
  public delegate void OnSensorStateChanged(Sensor sensor);
  
  public event OnMeasurementPerformed MeasurementPerformed;
  public event OnSensorStateChanged SensorStateChanged;
  
  Task<List<Measurement>> ReadAsync(List<Sensor> sensors);
}
