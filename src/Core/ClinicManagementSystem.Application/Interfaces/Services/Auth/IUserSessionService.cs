using ClinicManagementSystem.Application.Common.Models;
using ClinicManagementSystem.Application.DTOs.Auth.Sessions;
using ClinicManagementSystem.Application.DTOs.Auth.Users;

namespace ClinicManagementSystem.Application.Interfaces.Services.Auth;

public interface IUserSessionService
{
    Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto requestDto, string? ipAddress, string? userAgent, CancellationToken ct = default);
    Task<ApiResponse<bool>> RevokeTokenAsync(RevokeTokenRequestDto requestDto, string? ipAddress, CancellationToken ct = default);
    Task<ApiResponse<bool>> RevokeAllUserSessionsAsync(Guid userId, CancellationToken ct = default);
    Task<ApiResponse<IEnumerable<UserSessionResponseDto>>> GetActiveSessionsByUserIdAsync(Guid userId, CancellationToken ct = default);
}