namespace Domain.Services.Interfaces;

using Entities;
using Entities.Util;
using Records;
using Util;

public interface IMeasurementService {
  Task<Measurement?> GetByIdAsync(long id);
  Task<List<Measurement>> GetLatestAsync(long sensorId, int points);
  Task<PagedResult<Measurement>> GetHistoryPageAsync(DateTime startDate, DateTime endDate, DateTime? cursor, int pageSize, long? sensorId = null);
  Task<List<AggregatedMeasurement>> GetAggregatedHistoryForSensorAsync(DateTime startDate, DateTime endDate, MeasurementHistoryGranularity granularity, long sensorId);
  Task<OperationResult> DeleteByIdAsync(long id);
  Task<bool> PerformMeasurements();
}
