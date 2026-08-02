namespace ClinicManagementSystem.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default);

    Task SendEmailVerificationAsync(
        string toEmail,
        string toName,
        string verificationLink,
        CancellationToken cancellationToken = default);

    Task SendPasswordResetAsync(
        string toEmail,
        string toName,
        string resetLink,
        CancellationToken cancellationToken = default);

    Task SendPasswordResetEmailAsync(
        string email, string resetToken, 
        CancellationToken ct = default);

    Task SendEmailConfirmationAsync(
        string email, Guid userId, 
        string confirmationToken, 
        CancellationToken ct = default);

    Task SendOtpEmailAsync(
        string toEmail, 
        string otpCode, 
        CancellationToken ct = default);
}
