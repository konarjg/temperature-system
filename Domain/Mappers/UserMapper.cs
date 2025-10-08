namespace Domain.Mappers;

using Entities;
using Records;
using Services.External;

public static class UserMapper {
  public static void UpdateCredentials(this User user, UserCredentialsUpdateData data, IPasswordSecurity passwordSecurity) {
    user.Email = data.Email;
    user.PasswordHash = passwordSecurity.Hash(data.Password);
  }

  public static void UpdateRole(this User user,
    UserRoleUpdateData data) {
    user.Role = data.Role;
  }

  public static User ToEntity(this UserCreateData data,
    IPasswordSecurity passwordSecurity) {
    return new User() {
      Email = data.Email,
      PasswordHash = passwordSecurity.Hash(data.Password),
      Role = data.Role
    };
  } 
}
