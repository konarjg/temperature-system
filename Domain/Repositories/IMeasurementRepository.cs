namespace Domain.Repositories;

using Entities;
using Records;
using Services.Util;

public interface IMeasurementRepository {
  Task<Measurement?> GetByIdAsync(long id);
  Task<Measurement?> GetLatestAsync();
  Task<List<Measurement>> GetHistoryAsync(DateTime startDate, DateTime endDate);
  Task<List<AggregatedMeasurement>> GetAggregatedHistoryAsync(DateTime startDate, DateTime endDate, MeasurementHistoryGranularity granularity);
  Task AddAsync(Measurement measurement);
  void Remove(Measurement measurement);
}
