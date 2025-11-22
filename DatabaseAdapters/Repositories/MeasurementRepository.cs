namespace DatabaseAdapters.Repositories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Entities.Util;
using Domain.Records;
using Domain.Repositories;
using Domain.Services.Util;
using Microsoft.EntityFrameworkCore;

public class MeasurementRepository(IDatabaseContext databaseContext) : IMeasurementRepository {

  public async Task<Measurement?> GetByIdAsync(long id) {
    // AsNoTracking improves performance for read-only operations
    return await databaseContext.Measurements
        .AsNoTracking()
        .Include(m => m.Sensor)
        .FirstOrDefaultAsync(m => m.Id == id);
  }
  
  public async Task<List<Measurement>> GetLatestAsync(long sensorId, int points) {
    return await databaseContext.Measurements
        .AsNoTracking()
        .Where(m => m.SensorId == sensorId)
        .OrderByDescending(m => m.Timestamp)
        .Take(points)
        .ToListAsync();
  }

  public async Task<PagedResult<Measurement>> GetHistoryPageAsync(
    DateTime startDate,
    DateTime endDate,
    DateTime? cursor,
    int pageSize,
    long? sensorId = null) {
    
      IQueryable<Measurement> query = databaseContext.Measurements
          .AsNoTracking()
          .Where(m => m.Timestamp >= startDate && m.Timestamp < endDate);
    
      if (sensorId != null) {
          query = query.Where(m => m.SensorId == sensorId.Value);
      }

      if (cursor.HasValue) {
          query = query.Where(m => m.Timestamp < cursor.Value);
      }
    
      query = query.OrderByDescending(m => m.Timestamp);

      List<Measurement> items = await query.Take(pageSize + 1).ToListAsync();
    
      bool hasNextPage = items.Count > pageSize;
    
      if (hasNextPage) {
          items.RemoveAt(pageSize);
      }

      DateTime? nextCursor = items.LastOrDefault()?.Timestamp;
    
      return new PagedResult<Measurement>(items, nextCursor, hasNextPage);
  }

  public async Task<List<AggregatedMeasurement>> GetAggregatedHistoryForSensorAsync(
    DateTime startDate,
    DateTime endDate,
    MeasurementHistoryGranularity granularity, 
    long sensorId) {
    
    IQueryable<Measurement> query = databaseContext.Measurements
        .AsNoTracking()
        .Where(m => m.Timestamp >= startDate && m.Timestamp < endDate && m.SensorId == sensorId);

    List<AggregatedMeasurement> aggregatedResult = await GroupAndAggregateAsync(query, granularity);

    return aggregatedResult.OrderByDescending(a => a.TimeStamp).ToList();
  }

  private async Task<List<AggregatedMeasurement>> GroupAndAggregateAsync(
    IQueryable<Measurement> query,
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