using ClinicManagementSystem.Application.Common.Interfaces;
using ClinicManagementSystem.Application.Common.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Resend;

namespace ClinicManagementSystem.Infrastructure.ExternalServices;

public sealed class EmailService : IEmailService
{
    private readonly IResend _resend;
    private readonly ResendOptions _options;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IResend resend,
        IOptions<ResendOptions> options,
        ILogger<EmailService> logger)
    {
        _resend = resend;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new EmailMessage
            {
                From = _options.FromEmail,
                To = { toEmail },
                Subject = subject,
                HtmlBody = htmlBody
            };

            var response = await _resend.EmailSendAsync(message, cancellationToken);

            _logger.LogInformation(
                "[Resend] Email sent successfully to {ToEmail}. MessageId: {MessageId}",
                toEmail, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Resend] Failed to send email to {ToEmail}", toEmail);
            throw;
        }
    }

    public Task SendEmailVerificationAsync(
        string toEmail,
        string toName,
        string verificationLink,
        CancellationToken cancellationToken = default)
    {
        var html = $"""
            <h2>Verify Your Email</h2>
            <p>Hello {toName},</p>
            <p>Click the link below to verify your email address:</p>
            <a href="{verificationLink}">Verify Email</a>
            <p>This link expires in 24 hours.</p>
            """;

        return SendEmailAsync(toEmail, toName, "Verify Your Email Address", html, cancellationToken);
    }

    public Task SendPasswordResetAsync(
        string toEmail,
        string toName,
        string resetLink,
        CancellationToken cancellationToken = default)
    {
        var html = $"""
            <h2>Reset Your Password</h2>
            <p>Hello {toName},</p>
            <p>We received a request to reset your password. Click the link below:</p>
            <a href="{resetLink}">Reset Password</a>
            <p>This link expires in 1 hour. If you did not request this, ignore this email.</p>
            """;

        return SendEmailAsync(toEmail, toName, "Password Reset Request", html, cancellationToken);
    }

    public Task SendPasswordResetEmailAsync(
        string email, 
        string resetToken, 
        CancellationToken ct = default)
    {
        var html = $"""
            <h2>Password Reset Code</h2>
            <p>Your password reset token is:</p>
            <h3>{resetToken}</h3>
            <p>If you did not request a password reset, please ignore this email.</p>
            """;

        return SendEmailAsync(email, email, "Password Reset Token", html, ct);
    }

    public Task SendEmailConfirmationAsync(
        string email, 
        Guid userId, 
        string confirmationToken, 
        CancellationToken ct = default)
    {
        var html = $"""
            <h2>Confirm Your Email</h2>
            <p>Your confirmation token is:</p>
            <h3>{confirmationToken}</h3>
            <p>User ID: {userId}</p>
            """;

        return SendEmailAsync(email, email, "Confirm Your Email", html, ct);
    }

    public Task SendOtpEmailAsync(
        string toEmail, 
        string otpCode, 
        CancellationToken ct = default)
    {
        var html = $"""
            <h2>Verification Code (OTP)</h2>
            <p>Your OTP verification code is:</p>
            <h1 style="letter-spacing: 5px;">{otpCode}</h1>
            <p>This code is valid for a limited time. Do not share it with anyone.</p>
            """;

        return SendEmailAsync(toEmail, toEmail, "Your Verification Code (OTP)", html, ct);
    }
}