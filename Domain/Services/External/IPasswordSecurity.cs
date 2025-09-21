namespace Domain.Services.External;

public interface IPasswordSecurity {
  string Hash(string password);
  bool Verify(string password, string hashedPassword);
}
