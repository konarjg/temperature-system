namespace Domain.Records;

using Entities;

public record AuthResult(User User, RefreshToken RefreshToken, string AccessToken);
