using ClinicManagementSystem.Application.Common.Models;
using ClinicManagementSystem.Application.DTOs.Auth.UserRole;

namespace ClinicManagementSystem.Application.Interfaces.Services.Auth;

public interface IUserRoleService
{
    Task<ApiResponse<UserRolesDetailsResponseDto>> GetUserRolesAsync(Guid userId, CancellationToken ct = default);
    Task<ApiResponse<bool>> AssignRolesToUserAsync(AssignRolesToUserRequestDto requestDto, Guid? assignedBy = null, CancellationToken ct = default);
    Task<ApiResponse<Guid>> AddRoleToUserAsync(AddRoleToUserRequestDto requestDto, Guid? assignedBy = null, CancellationToken ct = default);
    Task<ApiResponse<bool>> RemoveRoleFromUserAsync(Guid userId, Guid roleId, CancellationToken ct = default);
    Task<ApiResponse<bool>> UserHasActiveRoleAsync(Guid userId, string roleCode, CancellationToken ct = default);
}