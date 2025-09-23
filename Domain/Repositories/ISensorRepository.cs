namespace Domain.Repositories;

using Entities;

public interface ISensorRepository {
  Task<List<Sensor>> GetAllAsync();
  Task AddRangeAsync(List<Sensor> sensors);
  void RemoveRange(List<Sensor> sensors);
}
