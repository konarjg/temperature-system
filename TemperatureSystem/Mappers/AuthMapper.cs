namespace TemperatureSystem.Mappers;

using Domain.Entities;
using Domain.Entities.Util;
using Domain.Records;
using Dto;

public static class AuthMapper {
  public static AuthResultDto ToDto(this AuthResult result) {
    return new AuthResultDto(result.User.Id, result.AccessToken);
  }
}
