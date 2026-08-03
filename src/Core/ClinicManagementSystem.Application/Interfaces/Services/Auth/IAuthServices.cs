using ClinicManagementSystem.Application.Common.Models;
using ClinicManagementSystem.Application.DTOs.Auth.Sessions;
using ClinicManagementSystem.Application.DTOs.Auth.Users;

namespace ClinicManagementSystem.Application.Interfaces.Services.Auth;

public interface IAuthServices
{
    Task<ApiResponse<Guid>> RegisterAsync(RegisterUserRequestDto requestDto, CancellationToken ct = default);
    Task<ApiResponse<UserResponseDto>> LoginAsync(LoginRequestDto Dto, CancellationToken ct = default);
    Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto Dto, CancellationToken ct = default);
    Task<ApiResponse<bool>> RevokeSessionAsync(Guid userId, string refreshToken, CancellationToken ct = default);
    Task<ApiResponse<bool>> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto Dto, CancellationToken ct = default);
}
