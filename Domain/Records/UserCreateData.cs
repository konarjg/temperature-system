namespace Domain.Records;

using Entities.Util;

public record UserCreateData(string Email, string Password, Role Role);
