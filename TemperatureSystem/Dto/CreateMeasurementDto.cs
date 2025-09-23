namespace TemperatureSystem.Dto;

public record CreateMeasurementDto(string Timestamp, long SensorId, float TemperatureCelsius);
