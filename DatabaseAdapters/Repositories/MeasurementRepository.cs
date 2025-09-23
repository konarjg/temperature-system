namespace DatabaseAdapters.Repositories;

using System.ComponentModel;
using Domain.Entities;
using Domain.Records;
using Domain.Repositories;
using Domain.Services.Util;
using Microsoft.EntityFrameworkCore;

public class MeasurementRepository(IDatabaseContext databaseContext) : IMeasurementRepository{

  public async Task<Measurement?> GetByIdAsync(long id) {
    return await databaseContext.Measurements.Include(m => m.Sensor).FirstOrDefaultAsync(m => m.Id == id);
  }
  
  public async Task<Measurement?> GetLatestAsync(long sensorId) {
    return await databaseContext.Measurements.Where(m => m.SensorId == sensorId).OrderByDescending(m => m.Timestamp).FirstOrDefaultAsync();
  }

  public async Task<List<Measurement>> GetHistoryAsync(DateTime startDate,
    DateTime endDate) {

    return await databaseContext.Measurements.Where(m => m.Timestamp >= startDate && m.Timestamp < endDate).ToListAsync();
  }
  
  public async Task<List<Measurement>> GetHistoryForSensorAsync(DateTime startDate,
    DateTime endDate, long sensorId) {

    return await databaseContext.Measurements.Where(m => m.Timestamp >= startDate && m.Timestamp < endDate && m.SensorId == sensorId).ToListAsync();
  }

  public async Task<List<AggregatedMeasurement>> GetAggregatedHistoryForSensorAsync(DateTime startDate,
    DateTime endDate,
    MeasurementHistoryGranularity granularity, long sensorId) {

    return await GroupAndAggregate(databaseContext.Measurements.Where(m =>
                   m.Timestamp >= startDate && m.Timestamp < endDate && m.SensorId == sensorId), granularity)
                 .OrderBy(a => a.TimeStamp)
                 .ToListAsync();
  }

  private IQueryable<AggregatedMeasurement> GroupAndAggregate(IQueryable<Measurement> query,
    MeasurementHistoryGranularity granularity) {

    switch (granularity) {
      case MeasurementHistoryGranularity.Hourly:
        return query.GroupBy(m => 
          new DateTime(m.Timestamp.Year,m.Timestamp.Month,m.Timestamp.Day,m.Timestamp.Hour,0,0))
                    .Select(g => 
                      new AggregatedMeasurement(g.Key,g.Average(m => 
                        m.TemperatureCelsius)));
      
      case MeasurementHistoryGranularity.Daily:
        return query.GroupBy(m => 
                     m.Timestamp.Date)
                    .Select(g => 
                      new AggregatedMeasurement(g.Key,g.Average(m => 
                        m.TemperatureCelsius)));
      
      case MeasurementHistoryGranularity.Monthly:
        return query.GroupBy(m => 
                      new DateTime(m.Timestamp.Year,m.Timestamp.Month,1))
                    .Select(g => 
                      new AggregatedMeasurement(g.Key,g.Average(m => 
                        m.TemperatureCelsius)));
    }

    throw new InvalidEnumArgumentException();
  }
  
  public async Task AddAsync(Measurement measurement) {
    await databaseContext.Measurements.AddAsync(measurement);
  }
  
  public async Task AddRangeAsync(List<Measurement> measurements) {
    await databaseContext.Measurements.AddRangeAsync(measurements);
  }

  public void Remove(Measurement measurement) {
    databaseContext.Measurements.Remove(measurement);
  }
}
