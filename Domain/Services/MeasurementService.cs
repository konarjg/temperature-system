namespace Domain.Services;

using Entities;
using Entities.Util;
using Interfaces;
using Records;
using Repositories;
using Util;

public class MeasurementService(IMeasurementRepository measurementRepository, IUnitOfWork unitOfWork) : IMeasurementService {

  public async Task<Measurement?> GetByIdAsync(long id) {
    return await measurementRepository.GetByIdAsync(id);
  }

  public async Task<List<Measurement>> GetLatestAsync(long sensorId, int points) {
    return await measurementRepository.GetLatestAsync(sensorId, points);
  }

  public async Task<PagedResult<Measurement>> GetHistoryPageAsync(DateTime startDate,
    DateTime endDate, int page, int pageSize, long? sensorId = null) {
    
    return await measurementRepository.GetHistoryPageAsync(startDate, endDate, page, pageSize, sensorId);
  }
  
  public async Task<List<AggregatedMeasurement>> GetAggregatedHistoryForSensorAsync(DateTime startDate,
    DateTime endDate,
    MeasurementHistoryGranularity granularity, long sensorId) {
    
    return await measurementRepository.GetAggregatedHistoryForSensorAsync(startDate,endDate,granularity, sensorId);
  }
  
  public async Task<bool> CreateRangeAsync(List<Measurement> measurements) {
    await measurementRepository.AddRangeAsync(measurements);
    return await unitOfWork.CompleteAsync() != 0;
  }

  public async Task<OperationResult> DeleteByIdAsync(long id) {
    Measurement? measurement = await measurementRepository.GetByIdAsync(id);

    if (measurement == null) {
      return OperationResult.NotFound;
    }
    
    measurementRepository.Remove(measurement);
    return await unitOfWork.CompleteAsync() != 0 ? OperationResult.Success : OperationResult.ServerError;
  }
}
