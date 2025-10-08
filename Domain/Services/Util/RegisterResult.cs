namespace Domain.Services.Util;

using Entities;

public record RegisterResult(User User, RegisterState State);
