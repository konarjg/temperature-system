namespace Domain.Services.Interfaces;

using Entities;
using Records;
using Util;

public interface IMeasurementService {
  Task<Measurement?> GetByIdAsync(long id);
  Task<Measurement?> GetLatestAsync(long sensorId);
  Task<List<Measurement>> GetHistoryAsync(DateTime startDate, DateTime endDate);
  Task<List<Measurement>> GetHistoryForSensorAsync(DateTime startDate, DateTime endDate, long sensorId);
  Task<List<AggregatedMeasurement>> GetAggregatedHistoryForSensorAsync(DateTime startDate, DateTime endDate, MeasurementHistoryGranularity granularity, long sensorId);
  Task<bool> CreateAsync(Measurement measurement);
  Task<bool> CreateRangeAsync(List<Measurement> measurements);
  Task<bool> DeleteByIdAsync(long id);
}
