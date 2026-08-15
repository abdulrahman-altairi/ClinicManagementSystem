using ClinicManagementSystem.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace ClinicManagementSystem.Infrastructure.ExternalServices;

public sealed class SmsService : ISmsService
{
    private readonly ILogger<SmsService> _logger;

    public SmsService(ILogger<SmsService> logger)
    {
        _logger = logger;
    }

    public Task SendSmsAsync(
        string toPhoneNumber,
        string message,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[SmsService] Sending SMS to {PhoneNumber} � Message length: {Length} chars",
            toPhoneNumber, message.Length);

        return Task.CompletedTask;
    }

    public Task SendOtpAsync(
        string toPhoneNumber,
        string otpCode,
        int expiryMinutes = 5,
        CancellationToken cancellationToken = default)
    {
        var message = $"Your Clinic Management System verification code is: {otpCode}. " +
                      $"Valid for {expiryMinutes} minutes. Do not share this code.";

        return SendSmsAsync(toPhoneNumber, message, cancellationToken);
    }
}