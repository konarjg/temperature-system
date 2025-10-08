namespace Domain.Services.Interfaces;

using Entities;
using Entities.Util;
using Records;
using Util;

public interface ISensorService {
  Task<Sensor?> GetByIdAsync(long id);
  Task<List<Sensor>> GetAllAsync();
  Task<List<Sensor>> GetAllByStateAsync(SensorState state);
  Task<bool> CreateAsync(Sensor sensor);
  Task<OperationResult> DeleteByIdAsync(long id);
  Task<OperationResult> UpdateDefinitionByIdAsync(long id, SensorDefinitionUpdateData data);
  Task<OperationResult> UpdateStateByIdAsync(long id, SensorStateUpdateData data);
}
