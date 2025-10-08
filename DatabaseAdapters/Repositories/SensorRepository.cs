namespace DatabaseAdapters.Repositories;

using Domain.Entities;
using Domain.Entities.Util;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

public class SensorRepository(IDatabaseContext databaseContext) : ISensorRepository {
  public async Task<Sensor?> GetByIdAsync(long id) => await databaseContext.Sensors.FindAsync(id);

  public async Task<List<Sensor>> GetAllAsync() => await databaseContext.Sensors.ToListAsync();

  public async Task<List<Sensor>> GetAllByStateAsync(SensorState state) {
    return await databaseContext.Sensors.Where(s => s.State == state).ToListAsync();
  }

  public async Task AddAsync(Sensor sensor) {
    await databaseContext.Sensors.AddAsync(sensor);
  }

  public void Remove(Sensor sensor) {
    databaseContext.Sensors.Remove(sensor);
  }
}
