using System.Security.Cryptography;
using ClinicManagementSystem.Application.Common.Interfaces;

namespace ClinicManagementSystem.Infrastructure.Identity;


public sealed class Hasher : IHasher
{
    private const int SaltSize       = 32;    
    private const int KeySize        = 64;    
    private const int Iterations     = 350_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA512;

    public (string Hash, string Salt) HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password, nameof(password));

        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password: password,
            salt:     salt,
            iterations: Iterations,
            hashAlgorithm: Algorithm,
            outputLength: KeySize);

        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    public bool VerifyPassword(string password, string storedHash, string storedSalt)
    {
        if (string.IsNullOrWhiteSpace(password)
            || string.IsNullOrWhiteSpace(storedHash)
            || string.IsNullOrWhiteSpace(storedSalt))
            return false;

        byte[] salt          = Convert.FromBase64String(storedSalt);
        byte[] expectedHash  = Convert.FromBase64String(storedHash);
        byte[] computedHash  = Rfc2898DeriveBytes.Pbkdf2(
            password:      password,
            salt:          salt,
            iterations:    Iterations,
            hashAlgorithm: Algorithm,
            outputLength:  KeySize);

        return CryptographicOperations.FixedTimeEquals(computedHash, expectedHash);
    }
    
    public string HashToken(string rawToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rawToken, nameof(rawToken));

        byte[] tokenBytes = System.Text.Encoding.UTF8.GetBytes(rawToken);
        byte[] hashBytes = SHA256.HashData(tokenBytes);

        return Convert.ToBase64String(hashBytes);
    }
}