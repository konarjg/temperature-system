namespace Domain.Security;

public interface IPasswordSecurity {
  string Hash(string password);
  bool Verify(string password, string hash);
}
