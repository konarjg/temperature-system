namespace TemperatureSystem.Mappers;

using System.Globalization;
using Domain.Entities;
using Domain.Entities.Util;
using Dto;

public static class MeasurementMapper {
  public static MeasurementDto ToDto(this Measurement measurement) {
    return new MeasurementDto(measurement.Id, measurement.Timestamp.ToString(CultureInfo.InvariantCulture), measurement.SensorId, measurement.TemperatureCelsius);
  }

  public static PagedResultDto<MeasurementDto> ToDto(this PagedResult<Measurement> result) {
    return new PagedResultDto<MeasurementDto>(result.Items.Select(m => m.ToDto()).ToList(),result.NextCursor, result.HasNextPage);
  }
}
