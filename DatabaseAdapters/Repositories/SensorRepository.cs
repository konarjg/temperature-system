namespace DatabaseAdapters.Repositories;

using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

public class SensorRepository(IDatabaseContext databaseContext) : ISensorRepository {

  public async Task<List<Sensor>> GetAllAsync() {
    return await databaseContext.Sensors.ToListAsync();
  }
  
  public async Task AddRangeAsync(List<Sensor> sensors) {
    await databaseContext.Sensors.AddRangeAsync(sensors);
  }
  
  public void RemoveRange(List<Sensor> sensors) {
    databaseContext.Sensors.RemoveRange(sensors);
  }
}
