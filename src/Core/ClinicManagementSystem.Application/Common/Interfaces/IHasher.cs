namespace ClinicManagementSystem.Application.Common.Interfaces;

public interface IHasher
{
    (string Hash, string Salt) HashPassword(string password);

    bool VerifyPassword(string password, string storedHash, string storedSalt);
    string HashToken(string rawToken);
}
