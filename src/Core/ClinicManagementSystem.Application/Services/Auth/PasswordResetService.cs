using System.Security.Cryptography;
using ClinicManagementSystem.Application.Common.Interfaces;
using ClinicManagementSystem.Application.Common.Models;
using ClinicManagementSystem.Application.Common.Options;
using ClinicManagementSystem.Application.DTOs.Auth.ResetPassword;
using ClinicManagementSystem.Application.Interfaces.Repositories;
using ClinicManagementSystem.Application.Interfaces.Services.Auth;
using ClinicManagementSystem.Domain.Entities.Auth;
using ClinicManagementSystem.Domain.Enums;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicManagementSystem.Application.Services.Auth;


public sealed class PasswordResetService : IPasswordResetService
{
    private readonly IIdentityRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly IEmailService _email;
    private readonly ICurrentUserService _currentUser;
    private readonly IHasher _hasher;
    private readonly AuthOptions _options;
    private readonly IDateTimeProvider _date;
    private readonly ILogger<PasswordResetService> _logger;
    private readonly IValidator<ForgotPasswordRequestDto> _forgotPasswordValidator;
    private readonly IValidator<ConfirmEmailRequestDto> _confirmEmailValidator;

    public PasswordResetService
    (
        IIdentityRepository repo,
        IUnitOfWork uow,
        IEmailService email,
        ICurrentUserService currentUser,
        IHasher hasher,
        AuthOptions options,
        IDateTimeProvider date,
        ILogger<PasswordResetService> logger,
        IValidator<ForgotPasswordRequestDto> forgotPasswordValidatoe,
        IValidator<ConfirmEmailRequestDto> confirmEmailValidator
    )
    {
        _repo = repo;
        _uow = uow;
        _email = email;
        _currentUser = currentUser;
        _hasher = hasher;
        _options = options;
        _date = date;
        _logger = logger;
        _forgotPasswordValidator = forgotPasswordValidatoe;
        _confirmEmailValidator = confirmEmailValidator;
    }


    public async Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequestDto request, CancellationToken ct = default)
    {
        var validation = await _forgotPasswordValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return ApiResponse<bool>.Failure(
                "Invalid email format.",
                validation.Errors.Select(e => new ErrorModel(e.PropertyName, e.ErrorMessage, e.ErrorCode)).ToList());
        }

        var user = await _repo.GetUserByEmailOrUsernameAsync(request.Email.Trim(), ct);

        if (user is null || user.IsDeleted || !user.IsActive)
        {
            _logger.LogInformation("Password reset requested for non-existent or inactive email: {Email}", request.Email);
            return ApiResponse<bool>.Success(true, "If your email is registered, a password reset link has been sent.");
        }

        var now = _date.UtcNow;
        var resetToken = GenerateSecureToken();
        var expiresAt = now.AddHours(2);


        var userToken = new UserToken
        {
            UserId = user.Id,
            TokenType = TokenType.PasswordReset,
            TokenHash = resetToken,
            ExpiresAt = expiresAt,
            CreatedAt = now,
            CreatedByIp = _currentUser.IpAddress,
        };

        await _repo.SaveUserTokenAsync(userToken, ct);

        try
        {
            await _email.SendPasswordResetEmailAsync(user.Email, resetToken, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", user.Email);
        }

        return ApiResponse<bool>.Success(true, "If your email is registered, a password reset link has been sent.");
    }

    public async Task<ApiResponse<bool>> ValidateResetTokenAsync(string token, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return ApiResponse<bool>.Failure(
                "Reset token is required.",
                ErrorModel.Global("Token missing.", "INVALID_TOKEN"));
        }

        var userId = await _repo.GetUserIdByValidTokenAsync(token, TokenType.PasswordReset, ct);
        if (!userId.HasValue)
        {
            return ApiResponse<bool>.Failure(
                "Invalid or expired password reset token.",
                ErrorModel.Global("Invalid or expired token.", "TOKEN_EXPIRED"));
        }

        return ApiResponse<bool>.Success(true, "Reset token is valid.");
    }

    public async Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return ApiResponse<bool>.Failure(
                "Token and new password are required.",
                ErrorModel.Global("Missing required fields.", "INVALID_INPUT"));
        }

        var userId = await _repo.GetUserIdByValidTokenAsync(request.Token, TokenType.PasswordReset, ct);
        if (!userId.HasValue)
        {
            return ApiResponse<bool>.Failure(
                "Invalid or expired password reset token.",
                ErrorModel.Global("Invalid or expired token.", "TOKEN_EXPIRED"));
        }

        var user = await _repo.GetUserByIdAsync(userId.Value, ct);
        if (user is null)
        {
            return ApiResponse<bool>.Failure("User not found.", ErrorModel.Global("User not found.", "USER_NOT_FOUND"));
        }

        var recentHashes = await _repo.GetRecentPasswordHashesAsync(user.Id, takeLast: _options.PasswordHistoryDepth, ct);
        foreach (var oldHash in recentHashes)
        {
            if (_hasher.VerifyPassword(request.NewPassword, oldHash, user.PasswordSalt))
            {
                return ApiResponse<bool>.Failure(
                    "You cannot reuse a recently used password.",
                    ErrorModel.Global("Password reused.", "PASSWORD_REUSED"));
            }
        }

        var (hash, salt) = _hasher.HashPassword(request.NewPassword);
        var now = _date.UtcNow;
        var combinedPasswordHistory = $"{salt}:{hash}";
        var passwordHistory = new PasswordHistory
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                PasswordHash = combinedPasswordHistory,
                ChangedAtUtc = now,
                ChangedByIp = _currentUser.IpAddress
            };

        await _uow.ExecuteInTransactionAsync(async () =>
        {
            await _repo.UpdatePasswordAsync(user.Id, hash, salt, now, ct);
            await _repo.MarkTokenAsUsedAsync(request.Token, ct);
            await _repo.TrackPasswordHistoryAsync(passwordHistory, ct);
            await _repo.RevokeAllUserSessionsAsync(user.Id, now, ct);
        }, ct);

        _logger.LogInformation("Password successfully reset for user {UserId}", user.Id);
        return ApiResponse<bool>.Success(true, "Password has been successfully reset. Please log in with your new password.");
    }

    public async Task<ApiResponse<bool>> ConfirmEmailAsync(ConfirmEmailRequestDto request, CancellationToken ct = default)
    {
        var validation = await _confirmEmailValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return ApiResponse<bool>.Failure(
                "Invalid verification request.",
                validation.Errors.Select(e => new ErrorModel(e.PropertyName, e.ErrorMessage, e.ErrorCode)).ToList());
        }

        var validUserId = await _repo.GetUserIdByValidTokenAsync(request.Token, TokenType.EmailVerification, ct);
        if (!validUserId.HasValue || validUserId.Value != request.UserId)
        {
            return ApiResponse<bool>.Failure(
                "Invalid or expired verification token.",
                ErrorModel.Global("Invalid verification token.", "TOKEN_INVALID"));
        }

        var user = await _repo.GetUserByIdAsync(request.UserId, ct);
        if (user is null)
        {
            return ApiResponse<bool>.Failure("User not found.", ErrorModel.Global("User not found.", "USER_NOT_FOUND"));
        }

        if (user.EmailVerified)
        {
            return ApiResponse<bool>.Success(true, "Email is already verified.");
        }

        user.EmailVerified = true;

        await _uow.ExecuteInTransactionAsync(async () =>
        {
            await _repo.UpdateUserAsync(user, ct);
            await _repo.MarkTokenAsUsedAsync(request.Token, ct);
        }, ct);

        _logger.LogInformation("Email verified for user {UserId}", user.Id);
        return ApiResponse<bool>.Success(true, "Email address confirmed successfully.");
    }

    public async Task<ApiResponse<bool>> ResendEmailConfirmationAsync(string email, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return ApiResponse<bool>.Failure(
                "Email address is required.",
                ErrorModel.Global("Email missing.", "EMAIL_REQUIRED"));
        }

        var user = await _repo.GetUserByEmailOrUsernameAsync(email.Trim(), ct);
        if (user is null || user.EmailVerified)
        {
            return ApiResponse<bool>.Success(true, "If account is eligible, a new confirmation link has been sent.");
        }


        var now = _date.UtcNow;
        var confirmToken = GenerateSecureToken();
        var expiresAt = now.AddDays(1);

        var userToken = new UserToken
        {
            UserId = user.Id,
            TokenType = TokenType.EmailVerification,
            TokenHash = confirmToken,
            ExpiresAt = expiresAt,
            CreatedAt = now,
            CreatedByIp = _currentUser.IpAddress,
        };

        await _repo.SaveUserTokenAsync(userToken, ct);

        try
        {
            await _email.SendEmailConfirmationAsync(user.Email, user.Id, confirmToken, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resend email confirmation to {Email}", user.Email);
        }

        return ApiResponse<bool>.Success(true, "If account is eligible, a new confirmation link has been sent.");
    }

    private static string GenerateSecureToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToHexString(randomBytes);
    }
}