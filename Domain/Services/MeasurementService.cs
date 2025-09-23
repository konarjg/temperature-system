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

  public async Task<Measurement?> GetLatestAsync(long sensorId) {
    return await measurementRepository.GetLatestAsync(sensorId);
  }

  public async Task<List<Measurement>> GetHistoryAsync(DateTime startDate,
    DateTime endDate) {
    
    return await measurementRepository.GetHistoryAsync(startDate, endDate);
  }
  
  public async Task<List<Measurement>> GetHistoryForSensorAsync(DateTime startDate,
    DateTime endDate, long sensorId) {
    
    return await measurementRepository.GetHistoryForSensorAsync(startDate, endDate, sensorId);
  }
  
  public async Task<List<AggregatedMeasurement>> GetAggregatedHistoryForSensorAsync(DateTime startDate,
    DateTime endDate,
    MeasurementHistoryGranularity granularity, long sensorId) {
    
    return await measurementRepository.GetAggregatedHistoryForSensorAsync(startDate,endDate,granularity, sensorId);
  }

  public async Task<bool> CreateAsync(Measurement measurement) {
    await measurementRepository.AddAsync(measurement);
    return await unitOfWork.CompleteAsync() != 0;
  }
  public async Task<bool> CreateRangeAsync(List<Measurement> measurements) {
    await measurementRepository.AddRangeAsync(measurements);
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
