namespace Domain.Repositories;

using Entities;
using Records;
using Services.Util;

public interface IMeasurementRepository {
  Task<Measurement?> GetByIdAsync(long id);
  Task<Measurement?> GetLatestAsync(long sensorId);
  Task<List<Measurement>> GetHistoryAsync(DateTime startDate, DateTime endDate);
  Task<List<Measurement>> GetHistoryForSensorAsync(DateTime startDate, DateTime endDate, long sensorId);
  Task<List<AggregatedMeasurement>> GetAggregatedHistoryForSensorAsync(DateTime startDate, DateTime endDate, MeasurementHistoryGranularity granularity, long sensorId);
  Task AddAsync(Measurement measurement);
  Task AddRangeAsync(List<Measurement> measurements);
  void Remove(Measurement measurement);
}
