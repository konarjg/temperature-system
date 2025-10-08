namespace ExternalServiceAdapters.NotificationService.Measurement;

using System.Globalization;
using Domain.Entities;

public static class MeasurementNotificationMapper {
  public static MeasurementNotification ToNotification(this Measurement measurement) {
    return new MeasurementNotification(measurement.Timestamp.ToString(CultureInfo.InvariantCulture), measurement.TemperatureCelsius);
  }
}
