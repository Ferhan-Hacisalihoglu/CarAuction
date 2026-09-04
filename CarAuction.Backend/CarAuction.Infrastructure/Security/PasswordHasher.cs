using System.Security.Cryptography;
using CarAuction.Application.Interfaces.Services;

namespace CarAuction.Infrastructure.Security;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 64;

    public string GenerateSalt()
    {
        var saltBytes = RandomNumberGenerator.GetBytes(SaltSize);
        return Convert.ToBase64String(saltBytes);
    }

    public string HashPassword(string password, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        using var hmac = new HMACSHA512(saltBytes);
        var hashBytes = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashBytes);
    }

    public bool VerifyPassword(string password, string salt, string hash)
    {
        var computedHash = HashPassword(password, salt);
        return CryptographicOperations.FixedTimeEquals(
            Convert.FromBase64String(computedHash),
            Convert.FromBase64String(hash)
        );
    }
}
