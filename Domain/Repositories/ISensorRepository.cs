namespace Domain.Repositories;

using Entities;
using Entities.Util;

public interface ISensorRepository {
  Task<Sensor?> GetByIdAsync(long id);
  Task<List<Sensor>> GetAllAsync();
  Task<List<Sensor>> GetAllByStateAsync(SensorState state);
  Task AddAsync(Sensor sensor); 
  void Remove(Sensor sensor);
}
