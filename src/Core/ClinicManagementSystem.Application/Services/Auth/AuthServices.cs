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
using ClinicManagementSystem.Domain.Enums;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace ClinicManagementSystem.Application.Services.Auth;

public sealed class AuthServices : IAuthServices
{

    private readonly IIdentityRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly IHasher _hasher;
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
            IHasher hasher,
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
        {
            return ApiResponse<Guid>.Failure("Validation failed.", validation.Errors.Select((e) => new ErrorModel(e.PropertyName, e.ErrorMessage, e.ErrorCode)).ToList());
        }

        if (await _repo.IsEmailTakenAsync(requestDto.Email, ct))
        {
            return ApiResponse<Guid>.Failure("Email already exists.", AuthErrors.EmailAlreadyTaken);
        }

        if (await _repo.IsUsernameTakenAsync(requestDto.Username, ct))
        {
            return ApiResponse<Guid>.Failure("Username already exists.", AuthErrors.UsernameAlreadyTaken);
        }

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
                        ValidFrom = now,
                        ValidTo = null 
                    }
                };

                var userRoles = new List<UserRole>
                {
                    new UserRole
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        RoleId = role.RoleId,
                        ValidFrom = now,
                        ValidTo = null,
                        AssignedBy = user.Id 
                    }
                };

                await _repo.AssignRolesToUserAsync(user.Id, userRoles, ct);
                }
                else
                    _logger.LogWarning("Registration requested for non-existent role: {RoleName} for User: {Email}", requestDto.RoleName, requestDto.Email);
            }

            var combinedPasswordHistory = $"{salt}:{hash}";
            var passwordHistory = new PasswordHistory
            {
                UserId = user.Id,
                PasswordHash = combinedPasswordHistory,
                ChangedAtUtc = now,
                ChangedByIp = _currentUser.IpAddress
            };
            await _repo.TrackPasswordHistoryAsync(passwordHistory, ct);
        }, ct);

        _logger.LogInformation("New user registered: {UserId} ({Email})", user.Id, user.Email);

        return ApiResponse<Guid>.Success(user.Id, "User registered successfully");
    }

    public async Task<ApiResponse<UserResponseDto>> LoginAsync(LoginRequestDto requestDto, CancellationToken ct)
    {
        var validation = await _loginValidator.ValidateAsync(requestDto, ct);
        if (!validation.IsValid)
        {
            return ApiResponse<UserResponseDto>.Failure("Login validation failed.", validation.Errors.Select(e => new ErrorModel(e.PropertyName, e.ErrorMessage, e.ErrorCode)).ToList());
        }

        var user = await _repo.GetUserByEmailOrUsernameAsync(requestDto.Identifier, ct);
        if (user is null)
        {
            return ApiResponse<UserResponseDto>.Failure("Invalid credentials.", AuthErrors.InvalidCredentials);
        }

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

        if (!_hasher.VerifyPassword(requestDto.Password, user.PasswordHash, user.PasswordSalt))
        {
            user.RecordFailedLogin(_options.MaxFailedAttempts, _options.LockoutDuration, now);
            await _repo.UpdateLoginAuditAsync(user.Id, now, _currentUser.IpAddress, user.AccessFailedCount, user.LockoutEnd, ct);

            _logger.LogWarning("Failed login for {Identifier} from {Ip}", requestDto.Identifier, _currentUser.IpAddress);
            return ApiResponse<UserResponseDto>.Failure(
                "Invalid credentials.", AuthErrors.InvalidCredentials);
        }

        if (user.TwoFactorEnabled)
        {
            if (string.IsNullOrWhiteSpace(requestDto.TwoFactorCode))
            {
                _logger.LogInformation("Two-factor authentication code required for user {UserId}", user.Id);
                return ApiResponse<UserResponseDto>.Failure(
                    "Two-factor authentication code required. Please enter the code from your authenticator app or a recovery code.",
                    AuthErrors.RequiresTwoFactor);
            }
        
            string cleanCode = requestDto.TwoFactorCode.Trim().ToUpperInvariant();
            bool is2FaValid = false;
            bool isRecoveryCodeUsed = false;
            UserToken? matchedRecoveryToken = null;
        
            if (!string.IsNullOrEmpty(user.TwoFactorSecret))
            {
                try
                {
                    var secretBytes = OtpNet.Base32Encoding.ToBytes(user.TwoFactorSecret);
                    var totp = new OtpNet.Totp(secretBytes);
                    is2FaValid = totp.VerifyTotp(cleanCode, out _, new OtpNet.VerificationWindow(1, 1));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error verifying TOTP during login for user {UserId}", user.Id);
                }
            }
        
            if (!is2FaValid)
            {
                var inputHash = _hasher.HashToken(cleanCode);
                matchedRecoveryToken = await _repo.GetActiveTokenByHashAsync(user.Id, inputHash, tokenTypeId: 4, now, ct);
                
                if (matchedRecoveryToken is not null)
                {
                    is2FaValid = true;
                    isRecoveryCodeUsed = true;
                }
            }
        
            if (!is2FaValid)
            {
                user.RecordFailedLogin(_options.MaxFailedAttempts, _options.LockoutDuration, now);
                await _repo.UpdateLoginAuditAsync(user.Id, now, _currentUser.IpAddress, user.AccessFailedCount, user.LockoutEnd, ct);
        
                _logger.LogWarning("Invalid 2FA code or recovery code attempt for user {UserId}", user.Id);
                return ApiResponse<UserResponseDto>.Failure(
                    "Invalid or expired two-factor authentication code.",
                    AuthErrors.InvalidTwoFactorCode);
            }
        
            if (isRecoveryCodeUsed && matchedRecoveryToken is not null)
            {
                matchedRecoveryToken.IsUsed = true;
                matchedRecoveryToken.UsedAt = now;
                await _repo.UpdateUserTokenAsync(matchedRecoveryToken, ct);
                _logger.LogWarning("User {UserId} logged in using a recovery code. Token consumed.", user.Id);
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

   public async Task<ApiResponse<TwoFactorRegistrationResponseDto>> InitiateEnableTwoFactorAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _repo.GetUserByIdAsync(userId, ct);
        if (user is null)
        {
            return ApiResponse<TwoFactorRegistrationResponseDto>.Failure("User not found.", AuthErrors.UserNotFound);
        }

        if (!user.IsActive)
        {
            return ApiResponse<TwoFactorRegistrationResponseDto>.Failure("User account is inactive.", AuthErrors.AccountInactive);
        }

        if (user.TwoFactorEnabled)
        {
            return ApiResponse<TwoFactorRegistrationResponseDto>.Failure("Two-factor authentication is already enabled.");
        }

        var secretBytes = OtpNet.KeyGeneration.GenerateRandomKey(20);
        var secretKey = OtpNet.Base32Encoding.ToString(secretBytes);

        var formattedEmail = Uri.EscapeDataString(user.Email);
        var issuerName = Uri.EscapeDataString("ClinicManagementSystem");
        var authenticatorUri = $"otpauth://totp/{issuerName}:{formattedEmail}?secret={secretKey}&issuer={issuerName}&digits=6&period=30";

        var recoveryCodes = new List<string>();

        for (int i = 0; i < 5; i++)
        {
            var code = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8).ToUpperInvariant();
            recoveryCodes.Add(code);

            var hashedToken = _hasher.HashToken(code);

            await _repo.SaveUserTokenAsync(new UserToken
            {
                UserId = user.Id,
                TokenType = TokenType.TwoFactorAuth, 
                TokenHash = hashedToken,
                ExpiresAt = _date.UtcNow.AddYears(10), 
                IsUsed = false
            }, ct);
        }

        user.TwoFactorSecret = secretKey; 
        await _repo.UpdateUserAsync(user, ct);

        _logger.LogInformation("Two-factor authentication setup initiated with 5 recovery tokens generated for user {UserId}.", userId);

        return ApiResponse<TwoFactorRegistrationResponseDto>.Success(new TwoFactorRegistrationResponseDto
        {
            SharedKey = secretKey,
            AuthenticatorUri = authenticatorUri,
            RecoveryCodes = recoveryCodes
        }, "Two-factor authentication setup initiated successfully. Please verify the code via your app to complete activation.");
    }

    public async Task<ApiResponse<bool>> ConfirmEnableTwoFactorAsync(Guid userId, string otpCode, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(otpCode))
        {
            return ApiResponse<bool>.Failure("OTP code is required.", AuthErrors.RequiredFieldMissing);
        }

        var user = await _repo.GetUserByIdAsync(userId, ct);
        if (user is null)
        {
            return ApiResponse<bool>.Failure("User not found.", AuthErrors.UserNotFound);
        }

        if (!user.IsActive)
        {
            return ApiResponse<bool>.Failure("User account is inactive.", AuthErrors.AccountInactive);
        }

        if (string.IsNullOrEmpty(user.TwoFactorSecret))
        {
            return ApiResponse<bool>.Failure("Two-factor authentication setup was not initiated for this account.");
        }

        var now = _date.UtcNow;
        bool isCodeValid = false;

        try
        {
            var secretBytes = OtpNet.Base32Encoding.ToBytes(user.TwoFactorSecret);
            var totp = new OtpNet.Totp(secretBytes);
            
            isCodeValid = totp.VerifyTotp(otpCode.Trim(), out _, new OtpNet.VerificationWindow(1, 1));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while verifying TOTP code for user {UserId}", userId);
            isCodeValid = false;
        }

        if (!isCodeValid)
        {
            user.RecordFailedLogin(_options.MaxFailedAttempts, _options.LockoutDuration, now);
            await _repo.UpdateLoginAuditAsync(user.Id, now, _currentUser.IpAddress, user.AccessFailedCount, user.LockoutEnd, ct);
            
            _logger.LogWarning("Invalid verification OTP code provided during 2FA confirmation for user {UserId}", userId);
            return ApiResponse<bool>.Failure("Invalid or expired verification code.");
        }

        await _uow.ExecuteInTransactionAsync(async () =>
        {
            user.TwoFactorEnabled = true; 
            user.EmailVerified = true;    
            user.RecordSuccessfulLogin(now, _currentUser.IpAddress);

            await _repo.UpdateUserAsync(user, ct);
            await _repo.UpdateLoginAuditAsync(user.Id, now, _currentUser.IpAddress, user.AccessFailedCount, user.LockoutEnd, ct);
            
            await _repo.RevokeAllUserSessionsAsync(userId, now, ct);
        }, ct);

        _logger.LogInformation("Two-factor authentication has been successfully enabled and confirmed for user {UserId}.", userId);

        return ApiResponse<bool>.Success(true, "Two-factor authentication enabled successfully. All previous active sessions have been revoked for your security.");
    }

    public async Task<ApiResponse<bool>> RedeemRecoveryCodeAsync(Guid userId, string recoveryCode, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(recoveryCode))
        {
            return ApiResponse<bool>.Failure("Recovery code is required.", AuthErrors.RequiredFieldMissing);
        }

        var user = await _repo.GetUserByIdAsync(userId, ct);
        if (user is null)
        {
            return ApiResponse<bool>.Failure("User not found.", AuthErrors.UserNotFound);
        }

        if (!user.IsActive)
        {
            return ApiResponse<bool>.Failure("User account is inactive.", AuthErrors.AccountInactive);
        }

        var now = _date.UtcNow;
        
        var inputHash = _hasher.HashToken(recoveryCode.Trim().ToUpperInvariant());

        var userToken = await _repo.GetActiveTokenByHashAsync(userId, inputHash, tokenTypeId: 4, now, ct);

        if (userToken is null)
        {
            user.RecordFailedLogin(_options.MaxFailedAttempts, _options.LockoutDuration, now);
            await _repo.UpdateLoginAuditAsync(user.Id, now, _currentUser.IpAddress, user.AccessFailedCount, user.LockoutEnd, ct);
            
            _logger.LogWarning("Invalid or consumed 2FA recovery code attempted for user {UserId}", userId);
            return ApiResponse<bool>.Failure("Invalid or already used recovery code.");
        }

        await _uow.ExecuteInTransactionAsync(async () =>
        {
            userToken.IsUsed = true;
            userToken.UsedAt = now;
            await _repo.UpdateUserTokenAsync(userToken, ct);

            user.TwoFactorEnabled = false;
            user.TwoFactorSecret = null;
            user.RecordSuccessfulLogin(now, _currentUser.IpAddress);

            await _repo.UpdateUserAsync(user, ct);
            await _repo.UpdateLoginAuditAsync(user.Id, now, _currentUser.IpAddress, user.AccessFailedCount, user.LockoutEnd, ct);
            
            await _repo.RevokeAllUserSessionsAsync(userId, now, ct);
            
            await _repo.InvalidateAllUser2FaTokensAsync(userId, now, ct);
        }, ct);

        _logger.LogWarning("User {UserId} successfully bypassed 2FA using a valid recovery code. Two-factor authentication has been deactivated.", userId);

        return ApiResponse<bool>.Success(true, "Recovery code accepted. Two-factor authentication has been temporarily disabled. You can now login normally and re-configure your new device.");
    }

    public async Task<ApiResponse<bool>> DisableTwoFactorAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _repo.GetUserByIdAsync(userId, ct);
        if (user is null)
        {
            return ApiResponse<bool>.Failure("User not found.", AuthErrors.UserNotFound);
        }

        if (!user.TwoFactorEnabled)
        {
            return ApiResponse<bool>.Success(true, "Two-factor authentication is already disabled.");
        }

        var now = _date.UtcNow;

        await _uow.ExecuteInTransactionAsync(async () =>
        {
            user.TwoFactorEnabled = false;
            user.TwoFactorSecret = null; 
            await _repo.UpdateUserAsync(user, ct);
            
            await _repo.InvalidateAllUser2FaTokensAsync(userId, now, ct);
            
            await _repo.RevokeAllUserSessionsAsync(userId, now, ct);
        }, ct);

        _logger.LogWarning("Two-factor authentication was deactivated and disabled for user {UserId} by secure request.", userId);

        return ApiResponse<bool>.Success(true, "Two-factor authentication has been disabled successfully.");
    }

    public async Task<ApiResponse<bool>> InitiateEmailVerificationAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _repo.GetUserByIdAsync(userId, ct);
        if (user is null) return ApiResponse<bool>.Failure("User not found.", AuthErrors.UserNotFound);
        if (user.EmailVerified) return ApiResponse<bool>.Success(true, "Email is already verified.");

        var code = new Random().Next(100000, 999999).ToString();
        var expiry = _date.UtcNow.AddMinutes(15); 
        var tokenHash = _hasher.HashToken(code);

        await _repo.SaveUserTokenAsync(new UserToken
        {
            UserId = user.Id,
            TokenType = TokenType.EmailVerification,
            TokenHash = tokenHash,
            ExpiresAt = expiry,
            IsUsed = false
        }, ct);

        await _email.SendOtpEmailAsync(user.Email, code, ct);

        _logger.LogInformation("Email verification token initiated and sent for user {UserId}", userId);
        return ApiResponse<bool>.Success(true, "Verification code sent to your email.");
    }

    public async Task<ApiResponse<bool>> ConfirmEmailVerificationAsync(Guid userId, string code, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(code)) return ApiResponse<bool>.Failure("Code is required.");

        var user = await _repo.GetUserByIdAsync(userId, ct);
        if (user is null) return ApiResponse<bool>.Failure("User not found.", AuthErrors.UserNotFound);
        if (user.EmailVerified) return ApiResponse<bool>.Success(true, "Email is already verified.");

        var now = _date.UtcNow;
        var inputHash = _hasher.HashToken(code.Trim());

        var userToken = await _repo.GetActiveTokenByHashAsync(userId, inputHash, tokenTypeId: 1, now, ct);

        if (userToken is null)
        {
            user.RecordFailedLogin(_options.MaxFailedAttempts, _options.LockoutDuration, now);
            await _repo.UpdateLoginAuditAsync(user.Id, now, _currentUser.IpAddress, user.AccessFailedCount, user.LockoutEnd, ct);
            return ApiResponse<bool>.Failure("Invalid or expired verification code.");
        }

        await _uow.ExecuteInTransactionAsync(async () =>
        {
            userToken.IsUsed = true;
            userToken.UsedAt = now;
            await _repo.UpdateUserTokenAsync(userToken, ct);

            user.EmailVerified = true;
            await _repo.UpdateUserAsync(user, ct);
        }, ct);

        _logger.LogInformation("User {UserId} successfully verified their email address.", userId);
        return ApiResponse<bool>.Success(true, "Email verified successfully.");
    }

    public async Task<ApiResponse<bool>> InitiatePhoneVerificationAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _repo.GetUserByIdAsync(userId, ct);
        if (user is null) return ApiResponse<bool>.Failure("User not found.", AuthErrors.UserNotFound);
        if (string.IsNullOrWhiteSpace(user.PhoneNumber)) return ApiResponse<bool>.Failure("No phone number is registered for this account.");
        if (user.PhoneVerified) return ApiResponse<bool>.Success(true, "Phone number is already verified.");

        var code = new Random().Next(100000, 999999).ToString();
        var expiry = _date.UtcNow.AddMinutes(10);
        var tokenHash = _hasher.HashToken(code);

        await _repo.SaveUserTokenAsync(new UserToken
        {
            UserId = user.Id,
            TokenType = TokenType.PhoneVerification, 
            TokenHash = tokenHash,
            ExpiresAt = expiry,
            IsUsed = false
        }, ct);

        _logger.LogWarning("SMS Verification Code [{Code}] generated for phone: {Phone}", code, user.PhoneNumber);

        return ApiResponse<bool>.Success(true, "Verification code sent to your registered phone number via SMS.");
    }

    public async Task<ApiResponse<bool>> ConfirmPhoneVerificationAsync(Guid userId, string code, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(code)) return ApiResponse<bool>.Failure("Code is required.");

        var user = await _repo.GetUserByIdAsync(userId, ct);
        if (user is null) return ApiResponse<bool>.Failure("User not found.", AuthErrors.UserNotFound);
        if (user.PhoneVerified) return ApiResponse<bool>.Success(true, "Phone number is already verified.");

        var now = _date.UtcNow;
        var inputHash = _hasher.HashToken(code.Trim());

        var userToken = await _repo.GetActiveTokenByHashAsync(userId, inputHash, tokenTypeId: 2, now, ct);

        if (userToken is null)
        {
            user.RecordFailedLogin(_options.MaxFailedAttempts, _options.LockoutDuration, now);
            await _repo.UpdateLoginAuditAsync(user.Id, now, _currentUser.IpAddress, user.AccessFailedCount, user.LockoutEnd, ct);
            return ApiResponse<bool>.Failure("Invalid or expired verification code.");
        }

        await _uow.ExecuteInTransactionAsync(async () =>
        {
            userToken.IsUsed = true;
            userToken.UsedAt = now;
            await _repo.UpdateUserTokenAsync(userToken, ct);

            user.PhoneVerified = true;
            await _repo.UpdateUserAsync(user, ct);
        }, ct);

        _logger.LogInformation("User {UserId} successfully verified their phone number.", userId);
        return ApiResponse<bool>.Success(true, "Phone number verified successfully.");
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
        {
            return ApiResponse<AuthResponseDto>.Failure(
                "User account is inactive.", AuthErrors.AccountInactive);   
        }

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


    public async Task<ApiResponse<bool>> RevokeAllUserSessionsAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _repo.GetUserByIdAsync(userId, ct);
        if (user is null)
        {
            return ApiResponse<bool>.Failure("User not found.", AuthErrors.UserNotFound);
        }

        var now = _date.UtcNow;

        await _repo.RevokeAllUserSessionsAsync(userId, now, ct);

        _logger.LogInformation("All active sessions have been forcefully revoked for user {UserId} by system request.", userId);

        return ApiResponse<bool>.Success(true, "All user sessions revoked successfully.");
    }

    public async Task<ApiResponse<IEnumerable<UserSessionResponseDto>>> GetActiveSessionsByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _repo.GetUserByIdAsync(userId, ct);
        if (user is null)
        {
            return ApiResponse<IEnumerable<UserSessionResponseDto>>.Failure("User not found.", AuthErrors.UserNotFound);
        }

        var activeSessions = await _repo.GetActiveSessionsByUserIdAsync(userId, ct);

        _logger.LogInformation("Retrieved {Count} active sessions for user {UserId}.", activeSessions.Count, userId);

        return ApiResponse<IEnumerable<UserSessionResponseDto>>.Success(activeSessions, "Active sessions retrieved successfully.");
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
            return ApiResponse<bool>.Failure("User not found.", AuthErrors.UserNotFound); 

        if (!_hasher.VerifyPassword(requestDto.CurrentPassword, user.PasswordHash, user.PasswordSalt))
            return ApiResponse<bool>.Failure(
                "Current password is incorrect.",
                AuthErrors.WrongCurrentPassword);

        var recentHashes = await _repo.GetRecentPasswordHashesAsync(userId, _options.PasswordHistoryDepth, ct);
        foreach (var combinedRecord in recentHashes)
        {
            var parts = combinedRecord.Split(':');
            var oldSalt = parts[0];
            var oldHash = parts[1];

            if (_hasher.VerifyPassword(requestDto.NewPassword, oldHash, oldSalt))
                return ApiResponse<bool>.Failure(
                    $"You cannot reuse any of your last {_options.PasswordHistoryDepth} passwords.",
                    AuthErrors.PasswordReused);
        }

        var (newHash, newSalt) = _hasher.HashPassword(requestDto.NewPassword);
        var now = _date.UtcNow;

        await _uow.ExecuteInTransactionAsync(async () =>
        {
            await _repo.UpdatePasswordAsync(userId, newHash, newSalt, now, ct);

            var combinedPasswordHistory = $"{newSalt}:{newHash}";

            var passwordHistory = new PasswordHistory
            {
                UserId = userId,
                PasswordHash = combinedPasswordHistory,
                ChangedAtUtc = now,
                ChangedByIp = _currentUser.IpAddress
            };

            await _repo.TrackPasswordHistoryAsync(passwordHistory, ct);

            await _repo.RevokeAllUserSessionsAsync(userId, now, ct);
        }, ct);

        _logger.LogInformation("Password changed for user {UserId}", userId);
        return ApiResponse<bool>.Success(true, "Password changed successfully. Please log in again.");
    }
}
