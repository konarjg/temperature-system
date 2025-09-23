namespace TemperatureSystem.Mappers;

using System.Globalization;
using Domain.Entities;
using Dto;

public static class MeasurementMapper {
  public static MeasurementDto ToDto(this Measurement measurement) {
    return new MeasurementDto(measurement.Id, measurement.Timestamp.ToString(CultureInfo.InvariantCulture), measurement.SensorId, measurement.TemperatureCelsius);
  }

  public static Measurement ToEntity(this CreateMeasurementDto dto) {
    return new Measurement() {
      TemperatureCelsius = dto.TemperatureCelsius,
      Timestamp = DateTime.Parse(dto.Timestamp),
      SensorId = dto.SensorId
    };
  }
}
