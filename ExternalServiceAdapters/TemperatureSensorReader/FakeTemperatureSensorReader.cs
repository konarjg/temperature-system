namespace ExternalServiceAdapters.TemperatureSensorReader;

using Domain.Entities;
using Domain.Entities.Util;
using Domain.Repositories;
using Domain.Services.External;
using Domain.Services.Interfaces;

public class FakeTemperatureSensorReader(ISensorService sensorService) : ITemperatureSensorReader {

  private readonly Random _random = new();
  private const float BaseTemperatureCelsius = 22.5f;
  private const float MaxFluctuation = 0.5f;
  private const int ConversionTimeMs = 750;
  private const float FailureChance = 0.05f;
  
  public async Task<List<Measurement>> ReadAsync() {
    List<Sensor> sensors = await sensorService.GetAllAsync();
    
    List<Measurement> measurements = new();

    foreach (Sensor sensor in sensors) {
      await Task.Delay(ConversionTimeMs);

      if (_random.NextSingle() < FailureChance) {
        sensor.State = SensorState.Unavailable;
        continue;
      }

      float fluctuation = (_random.NextSingle() * 2 - 1) * MaxFluctuation;
      float currentTemperature = BaseTemperatureCelsius + fluctuation;

      Measurement newMeasurement = new() {
        Timestamp = DateTime.UtcNow,
        SensorId = sensor.Id,
        Sensor = sensor,
        TemperatureCelsius = currentTemperature
      };
      
      sensor.State = SensorState.Operational;
      measurements.Add(newMeasurement);
    }

    return measurements;
  }
}
