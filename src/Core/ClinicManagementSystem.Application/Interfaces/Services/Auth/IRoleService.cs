using ClinicManagementSystem.Application.Common.Models;
using ClinicManagementSystem.Application.DTOs.Auth.Role;

namespace ClinicManagementSystem.Application.Interfaces.Services.Auth;

public interface IRoleService
{
    Task<ApiResponse<IReadOnlyList<RoleResponseDto>>> GetAllRolesAsync(CancellationToken ct = default);
    Task<ApiResponse<RoleResponseDto>> GetRoleByIdAsync(Guid roleId, CancellationToken ct = default);
    Task<ApiResponse<Guid>> CreateRoleAsync(CreateRoleRequestDto requestDto, CancellationToken ct = default);
    Task<ApiResponse<bool>> UpdateRoleAsync(Guid roleId, UpdateRoleRequestDto requestDto, CancellationToken ct = default);
    Task<ApiResponse<bool>> DeleteRoleAsync(Guid roleId, CancellationToken ct = default);
}
