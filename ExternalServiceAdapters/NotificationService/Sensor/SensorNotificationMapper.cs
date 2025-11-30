namespace ExternalServiceAdapters.NotificationService.Sensor;

using Domain.Entities;

public static class SensorNotificationMapper {
  public static SensorNotification ToNotification(this Sensor sensor) {
    return new SensorNotification(sensor.Id,sensor.State);
  }
}
