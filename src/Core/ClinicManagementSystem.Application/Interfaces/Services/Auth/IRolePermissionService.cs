using ClinicManagementSystem.Application.Common.Models;
using ClinicManagementSystem.Application.DTOs.Auth.AssignPermissions;

namespace ClinicManagementSystem.Application.Interfaces.Services.Auth;

public interface IRolePermissionService
{
    Task<ApiResponse<RolePermissionsDetailsResponseDto>> GetPermissionsByRoleIdAsync(Guid roleId, CancellationToken ct = default);
    Task<ApiResponse<bool>> AssignPermissionsToRoleAsync(AssignPermissionsToRoleRequestDto requestDto, CancellationToken ct = default);
    Task<ApiResponse<Guid>> AddPermissionToRoleAsync(Guid roleId, Guid permissionId, CancellationToken ct = default);
    Task<ApiResponse<bool>> RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId, CancellationToken ct = default);
    Task<ApiResponse<bool>> HasPermissionAsync(Guid roleId, string permissionCode, CancellationToken ct = default);
}