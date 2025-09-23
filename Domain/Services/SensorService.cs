namespace Domain.Services;

using Entities;
using Interfaces;
using Repositories;
using Util;

public class SensorService(ISensorRepository sensorRepository, IUnitOfWork unitOfWork) : ISensorService {

  public async Task SyncSensorsAsync(List<SensorDefinition> definitions) {
    Dictionary<string, SensorDefinition> definitionsMap = definitions.ToDictionary(d => d.Address);
    List<Sensor> existingSensors = await sensorRepository.GetAllAsync();
    
    List<Sensor> toRemove = existingSensors
                            .Where(s => !definitionsMap.ContainsKey(s.DeviceAddress))
                            .ToList();

    HashSet<string> existingAddresses = existingSensors.Select(s => s.DeviceAddress).ToHashSet();
    List<Sensor> toAdd = definitions
                         .Where(d => !existingAddresses.Contains(d.Address))
                         .Select(d => new Sensor {
                           DisplayName = d.DisplayName,
                           DeviceAddress = d.Address
                         })
                         .ToList();

    foreach (Sensor sensor in existingSensors) {
      if (!definitionsMap.TryGetValue(sensor.DeviceAddress, out SensorDefinition? definition)) {
        continue;
      }

      if (sensor.DisplayName == definition.DisplayName) {
        continue;
      }

      sensor.DisplayName = definition.DisplayName;
    }

    await sensorRepository.AddRangeAsync(toAdd);
    sensorRepository.RemoveRange(toRemove);

    await unitOfWork.CompleteAsync();
  }

  public async Task<List<Sensor>> GetAllAsync() {
    return await sensorRepository.GetAllAsync();
  }
}
