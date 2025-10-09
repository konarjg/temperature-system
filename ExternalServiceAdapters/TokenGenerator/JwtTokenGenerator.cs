namespace ExternalServiceAdapters.TokenGenerator;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Domain.Entities;
using Domain.Services.External;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

public class JwtTokenGenerator(IConfiguration configuration) : ITokenGenerator {
    private readonly SymmetricSecurityKey _securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? ""));

    static JwtTokenGenerator()
    {
        // Prevents JWT handler from mapping claims to long XML names
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();
    }

    public string GenerateAccessToken(User user)
    {
        List<Claim> claims = new()
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        SigningCredentials credentials = new(_securityKey, SecurityAlgorithms.HmacSha256);

        double expirationMinutes = double.Parse(configuration["Jwt:AccessTokenExpirationMinutes"] ?? "15");

        SecurityTokenDescriptor tokenDescriptor = new()
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = configuration["Jwt:Issuer"],
            Audience = configuration["Jwt:Audience"],
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
            SigningCredentials = credentials
        };

        JwtSecurityTokenHandler tokenHandler = new();
        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    public RefreshToken GenerateRefreshToken(User user)
    {
        byte[] randomNumber = new byte[64];

        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            string tokenString = Convert.ToBase64String(randomNumber);

            int expirationDays = int.Parse(configuration["Jwt:RefreshTokenExpirationDays"] ?? "7");

            RefreshToken refreshToken = new()
            {
                User = user,
                Token = tokenString,
                Expires = DateTime.UtcNow.AddDays(expirationDays),
                Revoked = null
            };

            return refreshToken;
        }
    }

    public VerificationToken GenerateVerificationToken(User user)
    {
        byte[] randomNumber = new byte[20];

        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            string tokenString = Convert.ToBase64String(randomNumber).Replace('+', '-')  
                                        .Replace('/', '_')  
                                        .TrimEnd('=');  

            int expirationDays = int.Parse(configuration["Jwt:UserVerificationTokenExpirationDays"] ?? "30");

            VerificationToken verificationToken = new()
            {
                User = user,
                Token = tokenString,
                Expires = DateTime.UtcNow.AddDays(expirationDays),
                Revoked = null
            };

            return verificationToken;
        }
    }
}
