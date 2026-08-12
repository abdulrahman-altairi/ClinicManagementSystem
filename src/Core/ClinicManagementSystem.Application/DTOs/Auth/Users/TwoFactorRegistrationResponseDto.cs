namespace ClinicManagementSystem.Application.DTOs.Auth.Users;

public class TwoFactorRegistrationResponseDto
{
    public string SharedKey { get; set; } = string.Empty;

    public string AuthenticatorUri { get; set; } = string.Empty;

    public IEnumerable<string> RecoveryCodes { get; set; } = Enumerable.Empty<string>();
}