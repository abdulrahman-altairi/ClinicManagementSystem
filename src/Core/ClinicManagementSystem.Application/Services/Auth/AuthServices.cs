using ClinicManagementSystem.Application.Common.Errors;
using ClinicManagementSystem.Application.Common.Interfaces;
using ClinicManagementSystem.Application.Common.Models;
using ClinicManagementSystem.Application.Common.Options;
using ClinicManagementSystem.Application.DTOs.Auth.Sessions;
using ClinicManagementSystem.Application.DTOs.Auth.UserRole;
using ClinicManagementSystem.Application.DTOs.Auth.Users;
using ClinicManagementSystem.Application.Interfaces.Repositories;
using ClinicManagementSystem.Application.Interfaces.Services.Auth;
using ClinicManagementSystem.Domain.Entities.Auth;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace ClinicManagementSystem.Application.Services.Auth;

public sealed class AuthServices : IAuthServices
{

    private readonly IIdentityRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;
    private readonly IDateTimeProvider _date;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<RegisterUserRequestDto> _logger;
    private readonly AuthOptions _options;
    private readonly IJwtTokenGenerator _jwt;
    private readonly IEmailService _email;
    private readonly IValidator<RegisterUserRequestDto> _registerValidator;
    private readonly IValidator<LoginRequestDto> _loginValidator;
    private readonly IValidator<RefreshTokenRequestDto> _refreshTokenValidator;
    private readonly IValidator<ChangePasswordRequestDto> _changePasswordValidator;

    public AuthServices
        (
            IIdentityRepository repo,
            IUnitOfWork uow,
            IPasswordHasher hasher,
            IDateTimeProvider date,
            ICurrentUserService currentUser,
            ILogger<RegisterUserRequestDto> logger,
            AuthOptions options,
            IJwtTokenGenerator jwt,
            IEmailService email,
            IValidator<RegisterUserRequestDto> registerValidator,
            IValidator<LoginRequestDto> loginValidator,
            IValidator<RefreshTokenRequestDto> refreshTokenValidator,
            IValidator<ChangePasswordRequestDto> changePasswordValidator
        )
    {
        _repo = repo;
        _uow = uow;
        _hasher = hasher;
        _date = date;
        _currentUser = currentUser;
        _logger = logger;
        _options = options;
        _jwt = jwt;
        _email = email;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _refreshTokenValidator = refreshTokenValidator;
        _changePasswordValidator = changePasswordValidator;
    }

    public async Task<ApiResponse<Guid>> RegisterAsync(RegisterUserRequestDto requestDto, CancellationToken ct)
    {
        var validation = await _registerValidator.ValidateAsync(requestDto, ct);
        if (!validation.IsValid)
            return ApiResponse<Guid>.Failure("Validation failed.", validation.Errors.Select((e) => new ErrorModel(e.PropertyName, e.ErrorMessage, e.ErrorCode)).ToList());

        if (await _repo.IsEmailTakenAsync(requestDto.Email, ct))
            return ApiResponse<Guid>.Failure("Email already exists.", AuthErrors.EmailAlreadyTaken);

        if (await _repo.IsUsernameTakenAsync(requestDto.Username, ct))
            return ApiResponse<Guid>.Failure("Username already exists.", AuthErrors.UsernameAlreadyTaken);

        var (hash, salt) = _hasher.HashPassword(requestDto.Password);
        var now = _date.UtcNow;

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            FirstName = requestDto.FirstName,
            LastName = requestDto.LastName,
            Email = requestDto.Email,
            NormalizedEmail = requestDto.Email.ToUpperInvariant(),
            Username = requestDto.Username,
            NormalizedUsername = requestDto.Username.ToUpperInvariant(),
            PasswordHash = hash,
            PasswordSalt = salt,
            CreatedAt = now,
            UpdatedAt = now,
            PhoneNumber = requestDto.PhoneNumber,
        };



        await _uow.ExecuteInTransactionAsync(async () =>
        {
            await _repo.CreateUserAsync(user, ct);

            if (!string.IsNullOrWhiteSpace(requestDto.RoleName))
            {
                var role = await _repo.GetRoleByNameAsync(requestDto.RoleName.Trim(), ct);

                if (role is not null)
                {
                    var roleAssignments = new List<UserRoleAssignmentDto>
            {
                new UserRoleAssignmentDto
                {
                    RoleId = role.RoleId,
                    ValidFrom = DateTimeOffset.UtcNow,
                    ValidTo = null 
                }
            };

                    await _repo.AssignRolesToUserAsync(user.Id, roleAssignments, user.Id, ct);
                }
            }

            await _repo.TrackPasswordHistoryAsync(user.Id, hash, _currentUser.IpAddress, ct);
        }, ct);

        _logger.LogInformation("New user registered: {UserId} ({Email})", user.Id, user.Email);

        return ApiResponse<Guid>.Success(user.Id, "User registered successfully");
    }

    public async Task<ApiResponse<UserResponseDto>> LoginAsync(LoginRequestDto requestDto, CancellationToken ct)
    {
        var validation = await _loginValidator.ValidateAsync(requestDto, ct);
        if (validation.IsValid)
            return ApiResponse<UserResponseDto>.Failure("Login valid.", validation.Errors.Select(e => new ErrorModel(e.PropertyName, e.ErrorMessage, e.ErrorCode)).ToList());

        var user = await _repo.GetUserByEmailOrUsernameAsync(requestDto.Identifier, ct);
        if (user is null)
            return ApiResponse<UserResponseDto>.Failure("Invalid credentials.", AuthErrors.InvalidCredentials);

        var now = _date.UtcNow;

        if (user.IsLockedOut(now))
        {
            _logger.LogWarning("Login attempt on locked account {UserId}", user.Id);
            return ApiResponse<UserResponseDto>.Failure(
               $"Account locked until {user.LockoutEnd:u}. Please try again later.",
               AuthErrors.AccountLocked);
        }

        if (!user.IsActive)
        {
            return ApiResponse<UserResponseDto>.Failure(
               "Account is suspended.", AuthErrors.AccountInactive);
        }

        if (_hasher.VerifyPassword(requestDto.Password, user.PasswordHash, user.PasswordSalt))
        {
            user.RecordFailedLogin(_options.MaxFailedAttempts, _options.LockoutDuration, now);
            await _repo.UpdateLoginAuditAsync(user.Id, now, _currentUser.IpAddress, user.AccessFailedCount, user.LockoutEnd, ct);

            _logger.LogWarning("Failed login for {Identifier} from {Ip}", requestDto.Identifier, _currentUser.IpAddress);
            return ApiResponse<UserResponseDto>.Failure(
                "Invalid credentials.", AuthErrors.InvalidCredentials);
        }

        if (user.TwoFactorEnabled && user.EmailVerified)
        {

            if (string.IsNullOrWhiteSpace(requestDto.TwoFactorCode))
            {
                var otpCode = new Random().Next(100000, 999999).ToString();
                var otpExpiry = now.AddMinutes(5); 

                await _repo.SaveEmailOtpAsync(user.Id, otpCode, otpExpiry, ct);

                await _email.SendOtpEmailAsync(user.Email, otpCode, ct);

                _logger.LogInformation("Sent 2FA Email OTP code to user {UserId}", user.Id);

                return ApiResponse<UserResponseDto>.Failure(
                    "Two-factor authentication code required. An OTP has been sent to your email.",
                    AuthErrors.RequiresTwoFactor);
            }

            var isOtpValid = await _repo.ValidateAndConsumeEmailOtpAsync(user.Id, requestDto.TwoFactorCode.Trim(), now, ct);
            if (!isOtpValid)
            {
                _logger.LogWarning("Invalid or expired 2FA OTP code provided for user {UserId}", user.Id);
                return ApiResponse<UserResponseDto>.Failure(
                    "Invalid or expired OTP code.",
                    AuthErrors.InvalidTwoFactorCode);
            }
        }


        var roles = await _repo.GetUserRolesAsync(user.Id, ct);
        var permissions = await _repo.GetUserPermissionsAsync(user.Id, ct);

        var accessToken = _jwt.GenerateAccessToken(user, roles, permissions);
        var refreshToken = _jwt.GenerateRefreshToken();
        var tokenExpiry = now.AddMinutes(_options.AccessTokenExpiryMinutes);
        var sessionExpiry = now.AddDays(_options.RefreshTokenExpiryDays);

        var session = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            RefreshToken = refreshToken,
            DeviceInfo = null,
            IpAddress = _currentUser.IpAddress,
            UserAgent = _currentUser.UserAgent,
            IssuedAtUtc = now,
            ExpiresAtUtc = sessionExpiry,
            CreatedAt = now
        };

        user.RecordSuccessfulLogin(now, _currentUser.IpAddress);

        await _uow.ExecuteInTransactionAsync(async () =>
        {
            await _repo.CreateSessionAsync(session, ct);
            await _repo.UpdateLoginAuditAsync(user.Id, now, _currentUser.IpAddress,
                user.AccessFailedCount, user.LockoutEnd, ct);
        }, ct);

        _logger.LogInformation("User {UserId} logged in from {Ip}", user.Id, _currentUser.IpAddress);



        var userAuth = new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiration = tokenExpiry,
        };

        var userProfile = new UserResponseDto
        {
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            DisplayName = user.DisplayName,
            PhoneNumber = user.PhoneNumber,
            PhoneVerified = user.PhoneVerified,
            EmailVerified = user.EmailVerified,
            TwoFactorEnabled = user.TwoFactorEnabled,
            IsActive = user.IsActive,
            AvatarUrl = user.AvatarUrl,
            LastLoginUtc = user.LastLoginUtc,
            CreatedAt = user.CreatedAt,
            Roles = roles,
            AuthResponseDto = userAuth,
        };


        return ApiResponse<UserResponseDto>.Success(userProfile, "Authentication successful.");

    }

    public async Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto requestDto, CancellationToken ct)
    {
        var validation = await _refreshTokenValidator.ValidateAsync(requestDto, ct);
        if (!validation.IsValid)
            return ApiResponse<AuthResponseDto>.Failure("Invalid token refresh request.");

        var principal = _jwt.GetPrincipalFromExpiredToken(requestDto.AccessToken);
        if (principal is null)
            return ApiResponse<AuthResponseDto>.Failure(
                "Invalid access token", AuthErrors.InvalidAccessToken);

        var session = await _repo.GetSessionByRefreshTokenAsync(requestDto.RefreshToken, ct);
        var now = _date.UtcNow;

        if (session is null || !session.IsActive(now))
        {
            _logger.LogWarning("Invalid or expired refresh token attempt from {Ip}", _currentUser.IpAddress);
            return ApiResponse<AuthResponseDto>.Failure(
                "Refresh token is invalid or has expired.",
                AuthErrors.InvalidRefreshToken);
        }

        var user = await _repo.GetUserByIdAsync(session.UserId, ct);
        if (user is null || !user.IsActive)
            return ApiResponse<AuthResponseDto>.Failure(
                "User account is inactive.", AuthErrors.AccountInactive);

        var roles = await _repo.GetUserRolesAsync(user.Id, ct);
        var permissions = await _repo.GetUserPermissionsAsync(user.Id, ct);
        var newAccessToken = _jwt.GenerateAccessToken(user, roles, permissions);
        var newRefreshToken = _jwt.GenerateRefreshToken();
        var newExpiry = now.AddDays(_options.RefreshTokenExpiryDays);

        var newSession = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            RefreshToken = newRefreshToken,
            IpAddress = _currentUser.IpAddress,
            UserAgent = _currentUser.UserAgent,
            IssuedAtUtc = now,
            ExpiresAtUtc = newExpiry,
            CreatedAt = now
        };

        await _uow.ExecuteInTransactionAsync(async () =>
        {
            await _repo.RevokeSessionAsync(session.Id, newRefreshToken, now, ct);
            await _repo.CreateSessionAsync(newSession, ct);
        }, ct);

        return ApiResponse<AuthResponseDto>.Success(new AuthResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            AccessTokenExpiration = now.AddMinutes(_options.AccessTokenExpiryMinutes),
        }, "Tokens refreshed successfully.");
    }


    public async Task<ApiResponse<bool>> RevokeSessionAsync(Guid userId, string refreshToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return ApiResponse<bool>.Failure("Refresh token is required.", AuthErrors.RequiredFieldMissing);

        var session = await _repo.GetSessionByRefreshTokenAsync(refreshToken, ct);
        var now = _date.UtcNow;

        if (session is null || session.UserId != userId)
            return ApiResponse<bool>.Failure(
                "Session not found.", AuthErrors.SessionNotFound);

        if (session.IsRevoked)
            return ApiResponse<bool>.Success(true, "Session was already revoked.");

        await _repo.RevokeSessionAsync(session.Id, null, now, ct);

        _logger.LogInformation("Session {SessionId} revoked for user {UserId}", session.Id, userId);
        return ApiResponse<bool>.Success(true, "Session revoked successfully.");
    }

    public async Task<ApiResponse<bool>> ChangePasswordAsync(
        Guid userId, ChangePasswordRequestDto requestDto, CancellationToken ct = default)
    {
        var validation = await _changePasswordValidator.ValidateAsync(requestDto, ct);
        if (!validation.IsValid)
            return ApiResponse<bool>.Failure(
                "Password change validation failed.",
                validation.Errors.Select(e => new ErrorModel(e.PropertyName, e.ErrorMessage, e.ErrorCode)).ToList());

        var user = await _repo.GetUserByIdAsync(userId, ct);
        if (user is null)
            return ApiResponse<bool>.Failure("User not found.", AuthErrors.RoleNotFound);

        if (!_hasher.VerifyPassword(requestDto.CurrentPassword, user.PasswordHash, user.PasswordSalt))
            return ApiResponse<bool>.Failure(
                "Current password is incorrect.",
                AuthErrors.WrongCurrentPassword);

        var recentHashes = await _repo.GetRecentPasswordHashesAsync(userId, _options.PasswordHistoryDepth, ct);
        foreach (var oldHash in recentHashes)
        {
            if (_hasher.VerifyPassword(requestDto.NewPassword, oldHash, user.PasswordSalt))
                return ApiResponse<bool>.Failure(
                    $"You cannot reuse any of your last {_options.PasswordHistoryDepth} passwords.",
                    AuthErrors.PasswordReused);
        }

        var (newHash, newSalt) = _hasher.HashPassword(requestDto.NewPassword);
        var now = _date.UtcNow;

        await _uow.ExecuteInTransactionAsync(async () =>
        {
            await _repo.UpdatePasswordAsync(userId, newHash, newSalt, now, ct);
            await _repo.TrackPasswordHistoryAsync(userId, newHash, _currentUser.IpAddress, ct);
            await _repo.RevokeAllUserSessionsAsync(userId, now, ct);
        }, ct);

        _logger.LogInformation("Password changed for user {UserId}", userId);
        return ApiResponse<bool>.Success(true, "Password changed successfully. Please log in again.");
    }
}
