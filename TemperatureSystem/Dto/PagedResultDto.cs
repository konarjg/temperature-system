namespace TemperatureSystem.Dto;

public record PagedResultDto<T>(List<T> Items, DateTime? NextCursor, bool HasNextPage);