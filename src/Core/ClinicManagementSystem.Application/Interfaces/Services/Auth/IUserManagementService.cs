using ClinicManagementSystem.Application.Common.Models;
using ClinicManagementSystem.Application.DTOs.Auth.Users;

namespace ClinicManagementSystem.Application.Interfaces.Services.Auth;

public interface IUserManagementService
{
    Task<ApiResponse<UserResponseDto>> GetUserByIdAsync(Guid userId, CancellationToken ct = default);
    Task<ApiResponse<PaginatedList<UserResponseDto>>> GetAllUsersAsync(UserQueryParams queryParams, CancellationToken ct = default);
    Task<ApiResponse<IEnumerable<UserResponseDto>>> SearchUsersAsync(string searchTerm, CancellationToken ct = default);
    Task<ApiResponse<bool>> UpdateProfileAsync(Guid userId, UpdateUserProfileRequestDto requestDto, Stream? avatarStream = null, string? avatarFileName = null, CancellationToken ct = default);
    Task<ApiResponse<bool>> DeleteAvatarAsync(Guid userId, CancellationToken ct = default);
    Task<ApiResponse<bool>> AdminUpdateUserAsync(Guid userId, AdminUpdateUserRequestDto requestDto, CancellationToken ct = default);
    Task<ApiResponse<bool>> ToggleUserStatusAsync(Guid userId, bool isActive, CancellationToken ct = default);
    Task<ApiResponse<bool>> UnlockUserAsync(Guid userId, CancellationToken ct = default);
}