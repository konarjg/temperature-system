namespace ExternalServiceAdapters.PasswordSecurity;

using System.Security.Cryptography;
using System.Text;
using Domain.Services.External;
using Konscious.Security.Cryptography;
using Microsoft.Extensions.Configuration;

public class Argon2PasswordSecurity(IConfiguration configuration) : IPasswordSecurity {
    private readonly int _degreeOfParallelism = int.Parse(configuration["Security:PasswordHashing:Argon2:DegreeOfParallelism"] ?? "8");
    private readonly int _memorySizeKiB = int.Parse(configuration["Security:PasswordHashing:Argon2:MemorySizeKiB"] ?? "131072");
    private readonly int _iterations = int.Parse(configuration["Security:PasswordHashing:Argon2:Iterations"] ?? "4");
    private readonly int _saltSizeInBytes = int.Parse(configuration["Security:PasswordHashing:Argon2:SaltSizeInBytes"] ?? "16");
    private readonly int _hashSizeInBytes = int.Parse(configuration["Security:PasswordHashing:Argon2:HashSizeInBytes"] ?? "32");

    public string Hash(string password)
    {
        byte[] salt = new byte[_saltSizeInBytes];
        RandomNumberGenerator.Fill(salt);

        using (Argon2id argon2 = new Argon2id(Encoding.UTF8.GetBytes(password)))
        {
            argon2.Salt = salt;
            argon2.DegreeOfParallelism = _degreeOfParallelism;
            argon2.MemorySize = _memorySizeKiB;
            argon2.Iterations = _iterations;

            byte[] hashBytes = argon2.GetBytes(_hashSizeInBytes);

            return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hashBytes)}";
        }
    }

    public bool Verify(string password, string hash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
        {
            return false;
        }

        try
        {
            string[] parts = hash.Split(':');
            if (parts.Length != 2)
            {
                return false;
            }

            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] storedHashBytes = Convert.FromBase64String(parts[1]);

            using (Argon2id argon2 = new Argon2id(Encoding.UTF8.GetBytes(password)))
            {
                argon2.Salt = salt;
                argon2.DegreeOfParallelism = _degreeOfParallelism;
                argon2.MemorySize = _memorySizeKiB;
                argon2.Iterations = _iterations;

                byte[] computedHashBytes = argon2.GetBytes(_hashSizeInBytes);

                return CryptographicOperations.FixedTimeEquals(computedHashBytes, storedHashBytes);
            }
        }
        catch (Exception)
        {
            return false;
        }
    }
}
