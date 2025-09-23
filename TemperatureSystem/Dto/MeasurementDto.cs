namespace TemperatureSystem.Dto;

public record MeasurementDto(long Id, string Timestamp, long SensorId, float TemperatureCelsius);