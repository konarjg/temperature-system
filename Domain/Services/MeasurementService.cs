namespace Domain.Services;

using Entities;
using Interfaces;
using Records;
using Repositories;
using Util;

public class MeasurementService(IMeasurementRepository measurementRepository, IUnitOfWork unitOfWork) : IMeasurementService {

  public async Task<Measurement?> GetByIdAsync(long id) {
    return await measurementRepository.GetByIdAsync(id);
  }

  public async Task<Measurement?> GetLatestAsync() {
    return await measurementRepository.GetLatestAsync();
  }

  public async Task<List<Measurement>> GetHistoryAsync(DateTime startDate,
    DateTime endDate) {
    
    return await measurementRepository.GetHistoryAsync(startDate, endDate);
  }

  public async Task<List<AggregatedMeasurement>> GetAggregatedHistoryAsync(DateTime startDate,
    DateTime endDate,
    MeasurementHistoryGranularity granularity) {
    
    return await measurementRepository.GetAggregatedHistoryAsync(startDate,endDate,granularity);
  }

  public async Task<bool> CreateAsync(Measurement measurement) {
    await measurementRepository.AddAsync(measurement);
    return await unitOfWork.CompleteAsync() != 0;
  }

  public async Task<bool> DeleteByIdAsync(long id) {
    Measurement? measurement = await measurementRepository.GetByIdAsync(id);

    if (measurement == null) {
      return false;
    }
    
    measurementRepository.Remove(measurement);
    return await unitOfWork.CompleteAsync() != 0;
  }
}
