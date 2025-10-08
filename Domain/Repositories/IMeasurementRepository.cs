namespace Domain.Repositories;

using Entities;
using Entities.Util;
using Records;
using Services.Util;

public interface IMeasurementRepository {
  Task<Measurement?> GetByIdAsync(long id);
  Task<List<Measurement>> GetLatestAsync(long sensorId, int points);
  Task<PagedResult<Measurement>> GetHistoryPageAsync(DateTime startDate, DateTime endDate, int page, int pageSize, long? sensorId = null);
  Task<List<AggregatedMeasurement>> GetAggregatedHistoryForSensorAsync(DateTime startDate, DateTime endDate, MeasurementHistoryGranularity granularity, long sensorId);
  Task AddAsync(Measurement measurement);
  Task AddRangeAsync(List<Measurement> measurements);
  void Remove(Measurement measurement);
}
