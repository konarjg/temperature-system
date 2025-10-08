namespace Domain.Services;

using Entities;
using Entities.Util;
using Interfaces;
using Mappers;
using Records;
using Repositories;
using Util;

public class SensorService(ISensorRepository sensorRepository, IUnitOfWork unitOfWork) : ISensorService {
  
  public async Task<Sensor?> GetByIdAsync(long id) {
    return await sensorRepository.GetByIdAsync(id);
  }
  
  public async Task<List<Sensor>> GetAllAsync() {
    return await sensorRepository.GetAllAsync();
  }

  public async Task<List<Sensor>> GetAllByStateAsync(SensorState state) {
    return await sensorRepository.GetAllByStateAsync(state);
  }

  public async Task<bool> CreateAsync(Sensor sensor) {
    await sensorRepository.AddAsync(sensor);
    return await unitOfWork.CompleteAsync() != 0;
  }
  public async Task<OperationResult> DeleteByIdAsync(long id) {
    Sensor? sensor = await sensorRepository.GetByIdAsync(id);

    if (sensor == null) {
      return OperationResult.NotFound;
    }
    
    sensorRepository.Remove(sensor);
    return await unitOfWork.CompleteAsync() != 0 ? OperationResult.Success : OperationResult.ServerError;
  }

  public async Task<OperationResult> UpdateDefinitionByIdAsync(long id, SensorDefinitionUpdateData data) {
    Sensor? sensor = await sensorRepository.GetByIdAsync(id);

    if (sensor == null) {
      return OperationResult.NotFound;
    }

    sensor.UpdateDefinition(data);
    return await unitOfWork.CompleteAsync() != 0 ? OperationResult.Success : OperationResult.ServerError;
  }

  public async Task<OperationResult> UpdateStateByIdAsync(long id, SensorStateUpdateData data) {
    Sensor? sensor = await sensorRepository.GetByIdAsync(id);

    if (sensor == null) {
      return OperationResult.NotFound;
    }

    sensor.UpdateState(data);
    return await unitOfWork.CompleteAsync() != 0 ? OperationResult.Success : OperationResult.ServerError;
  }
  
}
