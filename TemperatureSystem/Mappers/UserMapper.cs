namespace TemperatureSystem.Mappers;

using Domain.Entities;
using Domain.Entities.Util;
using Domain.Records;
using Dto;

public static class UserMapper {
  public static UserDto ToDto(this User user) {
    return new UserDto(user.Id,user.Email,user.Role,user.IsActive);
  }

  public static UserCreateData ToDomainCreateDto(this UserRequest request) {
    return new UserCreateData(request.Email,request.Password,Role.Unverified);
  }
  
  public static UserCredentialsUpdateData ToDomainUpdateDto(this UserRequest request) {
    return new UserCredentialsUpdateData(request.Email,request.Password);
  }

  public static UserRoleUpdateData ToDomainDto(this UserRoleRequest request) {
    return new UserRoleUpdateData(request.Role);
  }
}
