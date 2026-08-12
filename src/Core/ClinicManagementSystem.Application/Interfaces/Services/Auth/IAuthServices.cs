using ClinicManagementSystem.Application.Common.Models;
using ClinicManagementSystem.Application.DTOs.Auth.Sessions;
using ClinicManagementSystem.Application.DTOs.Auth.Users;

namespace ClinicManagementSystem.Application.Interfaces.Services.Auth;

public interface IAuthServices
{
    Task<ApiResponse<Guid>> RegisterAsync(RegisterUserRequestDto requestDto, CancellationToken ct = default);
    Task<ApiResponse<UserResponseDto>> LoginAsync(LoginRequestDto Dto, CancellationToken ct = default);
    Task<ApiResponse<TwoFactorRegistrationResponseDto>> InitiateEnableTwoFactorAsync(Guid userId, CancellationToken ct = default);
    Task<ApiResponse<bool>> ConfirmEnableTwoFactorAsync(Guid userId, string otpCode, CancellationToken ct = default); 
    Task<ApiResponse<bool>> RedeemRecoveryCodeAsync(Guid userId, string recoveryCode, CancellationToken ct = default);   
    Task<ApiResponse<bool>> DisableTwoFactorAsync(Guid userId, CancellationToken ct = default);
    Task<ApiResponse<bool>> InitiateEmailVerificationAsync(Guid userId, CancellationToken ct = default);
    Task<ApiResponse<bool>> ConfirmEmailVerificationAsync(Guid userId, string code, CancellationToken ct = default);
    Task<ApiResponse<bool>> InitiatePhoneVerificationAsync(Guid userId, CancellationToken ct = default);
    Task<ApiResponse<bool>> ConfirmPhoneVerificationAsync(Guid userId, string code, CancellationToken ct = default);
    Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto Dto, CancellationToken ct = default);
    Task<ApiResponse<bool>> RevokeSessionAsync(Guid userId, string refreshToken, CancellationToken ct = default);
    Task<ApiResponse<bool>> RevokeAllUserSessionsAsync(Guid userId, CancellationToken ct = default);
    Task<ApiResponse<IEnumerable<UserSessionResponseDto>>> GetActiveSessionsByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<ApiResponse<bool>> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto Dto, CancellationToken ct = default);
}

