namespace TemperatureSystem.Dto;

public record PagedResultDto<T>(List<T> Items, int TotalCount, int Page, int PageSize);
