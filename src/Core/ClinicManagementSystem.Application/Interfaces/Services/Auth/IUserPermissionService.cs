using ClinicManagementSystem.Application.Common.Models;
using ClinicManagementSystem.Application.DTOs.Auth.UserPermissions;

namespace ClinicManagementSystem.Application.Interfaces.Services.Auth;

public interface IUserPermissionService
{
    Task<ApiResponse<UserPermissionOverridesDetailsResponseDto>> GetUserPermissionOverridesAsync(Guid userId, CancellationToken ct = default);
    Task<ApiResponse<Guid>> AddUserPermissionOverrideAsync(AddUserPermissionOverrideRequestDto requestDto, CancellationToken ct = default);
    Task<ApiResponse<bool>> UpdateUserPermissionOverrideAsync(Guid userPermissionId, UpdateUserPermissionOverrideRequestDto requestDto, CancellationToken ct = default);
    Task<ApiResponse<bool>> DeleteUserPermissionOverrideAsync(Guid userPermissionId, CancellationToken ct = default);
    Task<ApiResponse<bool>> SetUserPermissionsBulkAsync(SetUserPermissionsBulkRequestDto requestDto, CancellationToken ct = default);
    Task<ApiResponse<bool>> EvaluateUserPermissionAsync(Guid userId, string permissionCode, CancellationToken ct = default);
    Task<ApiResponse<List<string>>> GetEffectivePermissionsForUserAsync(Guid userId, CancellationToken ct = default);
}