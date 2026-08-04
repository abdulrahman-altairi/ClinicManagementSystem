using ClinicManagementSystem.Application.Common.Errors;
using ClinicManagementSystem.Application.Common.Interfaces;
using ClinicManagementSystem.Application.Common.Models;
using ClinicManagementSystem.Application.DTOs.Auth.Sessions;
using ClinicManagementSystem.Application.DTOs.Auth.Users;
using ClinicManagementSystem.Application.Interfaces.Repositories;
using ClinicManagementSystem.Application.Interfaces.Services.Auth;
using ClinicManagementSystem.Domain.Entities.Auth;

namespace ClinicManagementSystem.Application.Services.Auth;

public sealed class UserSessionService : IUserSessionService
{
    private readonly IIdentityRepository _repo;
    private readonly IJwtTokenGenerator _jwt;
    private readonly IDateTimeProvider _date;
    private readonly IUnitOfWork _uow;

    public UserSessionService
        (
        IIdentityRepository repo, 
        IJwtTokenGenerator jwt,
        IDateTimeProvider date,
        IUnitOfWork uow
        )
    {
        _repo = repo;
        _jwt = jwt;
        _date = date;
        _uow = uow;
    }

    public async Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto requestDto, string? ipAddress, string? userAgent, CancellationToken ct = default)
    {
        var session = await _repo.GetSessionByRefreshTokenAsync(requestDto.RefreshToken, ct);
        if (session is null)
        {
            return ApiResponse<AuthResponseDto>.Failure("Invalid or non-existent refresh token.", SessionErrors.SessionNotFound);
        }

        var now = _date.UtcNow;

        if (session.IsRevoked)
        {
            await _repo.RevokeAllUserSessionsAsync(session.UserId, now, ct);
            return ApiResponse<AuthResponseDto>.Failure("Refresh token has been revoked. All active sessions were terminated for security.", SessionErrors.TokenRevoked);
        }

        if (session.ExpiresAtUtc <= now)
        {
            return ApiResponse<AuthResponseDto>.Failure("Refresh token has expired. Please log in again.", SessionErrors.TokenExpired);
        }

        var user = await _repo.GetUserByIdAsync(session.UserId, ct);
        if (user is null || !user.IsActive)
        {
            return ApiResponse<AuthResponseDto>.Failure("User account associated with this token was not found or is inactive.", UserErrors.UserNotFound);
        }

        var roles = await _repo.GetUserRolesAsync(user.Id, ct);
        var permissions = await _repo.GetUserPermissionsAsync(user.Id, ct);

        var newAccessToken = _jwt.GenerateAccessToken(user, roles, permissions);
        var newRefreshToken = _jwt.GenerateRefreshToken();
        var refreshTokenExpiresAt = _date.UtcNow.AddDays(7);

        var newSession = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            RefreshToken = newRefreshToken,
            DeviceInfo = null,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            IssuedAtUtc = now,
            ExpiresAtUtc = refreshTokenExpiresAt,
            IsRevoked = false
        };

        await _uow.ExecuteInTransactionAsync(async () =>
        {
            await _repo.RevokeSessionAsync(session.Id, newRefreshToken, now, ct);
            await _repo.CreateSessionAsync(newSession, ct);
        }, ct);

        var response = new AuthResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        };

        return ApiResponse<AuthResponseDto>.Success(response, "Token refreshed successfully.");
    }

    public async Task<ApiResponse<bool>> RevokeTokenAsync(RevokeTokenRequestDto requestDto, string? ipAddress, CancellationToken ct = default)
    {
        var session = await _repo.GetSessionByRefreshTokenAsync(requestDto.RefreshToken, ct);
        if (session is null)
        {
            return ApiResponse<bool>.Failure("Invalid or non-existent refresh token.", SessionErrors.SessionNotFound);
        }

        if (session.IsRevoked)
        {
            return ApiResponse<bool>.Success(true);
        }

        await _repo.RevokeSessionAsync(session.Id, null, _date.UtcNow, ct);
        return ApiResponse<bool>.Success(true, "Session revoked successfully.");
    }

    public async Task<ApiResponse<bool>> RevokeAllUserSessionsAsync(Guid userId, CancellationToken ct = default)
    {
        await _repo.RevokeAllUserSessionsAsync(userId, _date.UtcNow, ct);
        return ApiResponse<bool>.Success(true, "All user sessions revoked successfully.");
    }

    public async Task<ApiResponse<IEnumerable<UserSessionResponseDto>>> GetActiveSessionsByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _repo.GetUserByIdAsync(userId, ct);
        if (user is null)
        {
            return ApiResponse<IEnumerable<UserSessionResponseDto>>.Failure("User not found.",UserErrors.UserNotFound);
        }

        var activeSessions = await _repo.GetActiveSessionsByUserIdAsync(userId, ct);
        return ApiResponse<IEnumerable<UserSessionResponseDto>>.Success(activeSessions, "Active sessions retrieved successfully.");
    }
}