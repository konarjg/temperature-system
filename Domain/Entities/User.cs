namespace Domain.Entities;

using Util;

public class User {
  public long Id { get; set; }
  public required string Email { get; set; }
  public required string PasswordHash { get; set; }
  public required Role Role  { get; set; }
  public DateTime? Deleted  { get; set; }

  public bool IsActive => Deleted == null && Role != Role.Unverified;
}
