namespace ClinicManagementSystem.Application.Common.Interfaces;

public interface ISmsService
{
    Task SendSmsAsync(
        string toPhoneNumber,
        string message,
        CancellationToken cancellationToken = default);

    Task SendOtpAsync(
        string toPhoneNumber,
        string otpCode,
        int expiryMinutes = 5,
        CancellationToken cancellationToken = default);
}
