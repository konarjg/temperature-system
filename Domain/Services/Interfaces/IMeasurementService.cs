namespace Domain.Services.Interfaces;

using Entities;
using Records;
using Util;

public interface IMeasurementService {
  Task<Measurement?> GetByIdAsync(long id);
  Task<Measurement?> GetLatestAsync();
  Task<List<Measurement>> GetHistoryAsync(DateTime startDate, DateTime endDate);
  Task<List<AggregatedMeasurement>> GetAggregatedHistoryAsync(DateTime startDate, DateTime endDate, MeasurementHistoryGranularity granularity);
  Task<bool> CreateAsync(Measurement measurement);
  Task<bool> DeleteByIdAsync(long id);
}
