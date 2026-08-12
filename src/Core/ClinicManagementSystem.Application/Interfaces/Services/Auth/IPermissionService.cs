using ClinicManagementSystem.Application.Common.Models;
using ClinicManagementSystem.Application.DTOs.Auth.Permissions;

namespace ClinicManagementSystem.Application.Interfaces.Services.Auth;

public interface IPermissionService
{
    Task<ApiResponse<IReadOnlyList<PermissionResponseDto>>> GetAllPermissionsAsync(CancellationToken ct = default);
    Task<ApiResponse<IReadOnlyList<GroupedPermissionsResponseDto>>> GetGroupedPermissionsAsync(CancellationToken ct = default);
    Task<ApiResponse<PermissionResponseDto>> GetPermissionByIdAsync(Guid permissionId, CancellationToken ct = default);
    Task<ApiResponse<PermissionResponseDto>> GetPermissionByCodeAsync(string permissionCode, CancellationToken ct = default);
    Task<ApiResponse<Guid>> CreatePermissionAsync(CreatePermissionRequestDto request, CancellationToken ct = default);
    Task<ApiResponse<bool>> UpdatePermissionAsync(Guid permissionId, UpdatePermissionRequestDto request, CancellationToken ct = default);
    Task<ApiResponse<bool>> DeletePermissionAsync(Guid permissionId, CancellationToken ct = default);

    Task<ApiResponse<PaginatedList<PermissionResponseDto>>> SearchPermissionsAsync(PermissionSearchFilter filter, CancellationToken ct = default);
    Task<ApiResponse<bool>> TogglePermissionStatusAsync(Guid permissionId, bool isActive, CancellationToken ct = default);
}