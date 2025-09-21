namespace Domain.Entities;

public class RefreshToken {
  public long Id { get; set; }
  public User User { get; set; }
  public required string Token { get; set; }
  public required DateTime Expires { get; set; }
  public DateTime? Revoked { get; set; }
  
  public bool IsActive => Revoked == null && Expires >= DateTime.UtcNow;
}
