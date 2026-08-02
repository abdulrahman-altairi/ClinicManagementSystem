namespace ClinicManagementSystem.Application.Common.Interfaces;

public interface IPasswordHasher
{
    (string Hash, string Salt) HashPassword(string password);

    bool VerifyPassword(string password, string storedHash, string storedSalt);
}
