using ClinicManagementSystem.Application.Common.Models;
using ClinicManagementSystem.Application.DTOs.Auth.UserPermissions;

namespace ClinicManagementSystem.Application.Interfaces.Services.Auth;

public interface IUserPermissionService
{
    Task<ApiResponse<UserPermissionOverridesDetailsResponseDto>> GetUserPermissionOverridesAsync(Guid userId, CancellationToken ct = default);
    Task<ApiResponse<Guid>> AddUserPermissionOverrideAsync(AddUserPermissionOverrideRequestDto requestDto, Guid? grantedBy = null, CancellationToken ct = default);
    Task<ApiResponse<bool>> UpdateUserPermissionOverrideAsync(Guid userPermissionId, UpdateUserPermissionOverrideRequestDto requestDto, Guid? updatedBy = null, CancellationToken ct = default);
    Task<ApiResponse<bool>> DeleteUserPermissionOverrideAsync(Guid userPermissionId, CancellationToken ct = default);
    Task<ApiResponse<bool>> SetUserPermissionsBulkAsync(SetUserPermissionsBulkRequestDto requestDto, Guid? grantedBy = null, CancellationToken ct = default);
    Task<ApiResponse<bool>> EvaluateUserPermissionAsync(Guid userId, string permissionCode, CancellationToken ct = default);
}