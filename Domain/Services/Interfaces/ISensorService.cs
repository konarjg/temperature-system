namespace Domain.Services.Interfaces;

using Entities;
using Util;

public interface ISensorService {
  Task SyncSensorsAsync(List<SensorDefinition> definitions);
  Task<List<Sensor>> GetAllAsync();
}
