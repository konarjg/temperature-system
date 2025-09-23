namespace Domain.Services.External;

using Entities;

public interface ITemperatureSensorReader {
  Task<List<Measurement>> ReadAsync();
}
