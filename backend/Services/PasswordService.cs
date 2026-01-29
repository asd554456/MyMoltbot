using System.Security.Cryptography;

namespace Backend.Services;

public class PasswordService
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100000;

    public string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
        
        var result = new byte[SaltSize + HashSize];
        Buffer.BlockCopy(salt, 0, result, 0, SaltSize);
        Buffer.BlockCopy(hash, 0, result, SaltSize, HashSize);
        
        return Convert.ToBase64String(result);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        var decoded = Convert.FromBase64String(hashedPassword);
        var salt = decoded[..SaltSize];
        var storedHash = decoded[SaltSize..];
        
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
        
        return CryptographicOperations.FixedTimeEquals(hash, storedHash);
    }
}
