namespace Domain.Services;

using Entities;
using Entities.Util;
using External;
using Interfaces;
using Microsoft.Extensions.Logging;
using Records;
using Repositories;
using Util;

public class MeasurementService(ISensorRepository sensorRepository, IMeasurementRepository measurementRepository, ITemperatureSensorReader temperatureSensorReader, INotificationService<Measurement> measurementNotificationService, INotificationService<Sensor> sensorNotificationService, IUnitOfWork unitOfWork) : IMeasurementService {
  
  public async Task<Measurement?> GetByIdAsync(long id) {
    return await measurementRepository.GetByIdAsync(id);
  }

  public async Task<List<Measurement>> GetLatestAsync(long sensorId, int points) {
    return await measurementRepository.GetLatestAsync(sensorId, points);
  }

  public async Task<PagedResult<Measurement>> GetHistoryPageAsync(DateTime startDate,
    DateTime endDate, DateTime? cursor, int pageSize, long? sensorId = null) {
    
    return await measurementRepository.GetHistoryPageAsync(startDate, endDate, cursor, pageSize, sensorId);
  }
  
  public async Task<List<AggregatedMeasurement>> GetAggregatedHistoryForSensorAsync(DateTime startDate,
    DateTime endDate,
    MeasurementHistoryGranularity granularity, long sensorId) {
    
    return await measurementRepository.GetAggregatedHistoryForSensorAsync(startDate,endDate,granularity, sensorId);
  }

  public async Task<OperationResult> DeleteByIdAsync(long id) {
    Measurement? measurement = await measurementRepository.GetByIdAsync(id);

    if (measurement == null) {
      return OperationResult.NotFound;
    }
    
    measurementRepository.Remove(measurement);
    return await unitOfWork.CompleteAsync() != 0 ? OperationResult.Success : OperationResult.ServerError;
  }
  
  public async Task<bool> PerformMeasurements() {
    List<Sensor> sensors = await sensorRepository.GetAllAsync();
    temperatureSensorReader.MeasurementPerformed += OnMeasurementPerformed;
    temperatureSensorReader.SensorStateChanged += OnSensorStateChanged;
    
    List<Measurement> measurements = await temperatureSensorReader.ReadAsync(sensors);
    
    temperatureSensorReader.MeasurementPerformed -= OnMeasurementPerformed;
    temperatureSensorReader.SensorStateChanged -= OnSensorStateChanged;
    
    await measurementRepository.AddRangeAsync(measurements);
    
    return await unitOfWork.CompleteAsync() != 0;
  }

  private void OnMeasurementPerformed(Measurement measurement) {
    measurementNotificationService.NotifyChangeAsync(measurement);
  }

  private void OnSensorStateChanged(Sensor sensor) {
    sensorNotificationService.NotifyChangeAsync(sensor);
  }
}
