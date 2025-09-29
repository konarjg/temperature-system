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

    return await databaseContext.Measurements.Where(m => m.Timestamp >= startDate && m.Timestamp < endDate && m.SensorId == sensorId).OrderByDescending(m => m.Timestamp).ToListAsync();
  }

  public async Task<List<AggregatedMeasurement>> GetAggregatedHistoryForSensorAsync(DateTime startDate,
    DateTime endDate,
    MeasurementHistoryGranularity granularity, long sensorId) {
    IQueryable<Measurement> query = databaseContext.Measurements.Where(m =>
        m.Timestamp >= startDate && m.Timestamp < endDate && m.SensorId == sensorId);

    List<AggregatedMeasurement> aggregatedResult = await GroupAndAggregateAsync(query, granularity);

    return aggregatedResult
        .OrderByDescending(a => a.TimeStamp)
        .ToList();
}

private async Task<List<AggregatedMeasurement>> GroupAndAggregateAsync(IQueryable<Measurement> query,
    MeasurementHistoryGranularity granularity) {
    switch (granularity) {
        case MeasurementHistoryGranularity.Hourly:
            return await query
                .GroupBy(m => new { m.Timestamp.Year, m.Timestamp.Month, m.Timestamp.Day, m.Timestamp.Hour })
                .Select(g => new AggregatedMeasurement(
                    new DateTime(g.Key.Year, g.Key.Month, g.Key.Day, g.Key.Hour, 0, 0),
                    g.Average(m => m.TemperatureCelsius)
                )).ToListAsync();

        case MeasurementHistoryGranularity.Daily:
            return await query
                .GroupBy(m => new { m.Timestamp.Year, m.Timestamp.Month, m.Timestamp.Day })
                .Select(g => new AggregatedMeasurement(
                    new DateTime(g.Key.Year, g.Key.Month, g.Key.Day),
                    g.Average(m => m.TemperatureCelsius)
                )).ToListAsync();

        case MeasurementHistoryGranularity.Monthly:
            return await query
                .GroupBy(m => new { m.Timestamp.Year, m.Timestamp.Month })
                .Select(g => new AggregatedMeasurement(
                    new DateTime(g.Key.Year, g.Key.Month, 1),
                    g.Average(m => m.TemperatureCelsius)
                )).ToListAsync();

        default:
            throw new ArgumentOutOfRangeException(nameof(granularity));
    }
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
