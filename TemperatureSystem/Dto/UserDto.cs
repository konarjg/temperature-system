namespace TemperatureSystem.Dto;

using Domain.Entities.Util;

public record UserDto(long Id, String Email, Role Role, bool IsActive);
