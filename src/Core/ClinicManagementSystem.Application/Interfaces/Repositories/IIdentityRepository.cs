using ClinicManagementSystem.Application.DTOs.Auth.Permissions;
using ClinicManagementSystem.Application.DTOs.Auth.Role;
using ClinicManagementSystem.Application.DTOs.Auth.UserPermissions;
using ClinicManagementSystem.Application.DTOs.Auth.UserRole;
using ClinicManagementSystem.Domain.Entities.Auth;

namespace ClinicManagementSystem.Application.Interfaces.Repositories;

public interface IIdentityRepository
{
    Task<ApplicationUser?> GetUserByIdAsync(Guid userId, CancellationToken ct = default);
    Task<ApplicationUser?> GetUserByEmailOrUsernameAsync(string identifier, CancellationToken ct = default);
    Task<bool> IsEmailTakenAsync(string email, CancellationToken ct = default);
    Task<bool> IsUsernameTakenAsync(string username, CancellationToken ct = default);
    Task<Guid> CreateUserAsync(ApplicationUser user, CancellationToken ct = default);
    Task UpdateUserAsync(ApplicationUser user, CancellationToken ct = default);
    Task UpdateLoginAuditAsync(Guid userId, DateTimeOffset loginTime, string? ipAddress, byte accessFailedCount, DateTimeOffset? lockoutEnd, CancellationToken ct = default);
    Task UpdatePasswordAsync(Guid userId, string passwordHash, string passwordSalt, DateTimeOffset changedAt, CancellationToken ct = default);
    Task TrackPasswordHistoryAsync(Guid userId, string passwordHash, string? ipAddress, CancellationToken ct = default);
    Task<List<string>> GetRecentPasswordHashesAsync(Guid userId, int takeLast = 5, CancellationToken ct = default);
    Task<List<string>> GetUserRolesAsync(Guid userId, CancellationToken ct = default);
    Task<List<string>> GetUserPermissionsAsync(Guid userId, CancellationToken ct = default);
    Task AssignRolesToUserAsync(Guid userId, IEnumerable<UserRoleAssignmentDto> roles, Guid? assignedBy, CancellationToken ct = default); 
    Task AssignPermissionsToRoleAsync(Guid roleId, IEnumerable<Guid> permissionIds, CancellationToken ct = default);
    Task SetUserPermissionOverridesAsync(Guid userId, IEnumerable<UserPermissionOverrideItemDto> overrides, Guid grantedBy, CancellationToken ct = default);
    Task CreateSessionAsync(UserSession session, CancellationToken ct = default);
    Task<UserSession?> GetSessionByRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    Task RevokeSessionAsync(Guid sessionId, string? replacedByToken, DateTimeOffset revokedAt, CancellationToken ct = default);
    Task RevokeAllUserSessionsAsync(Guid userId, DateTimeOffset revokedAt, CancellationToken ct = default);
    Task SaveEmailOtpAsync(Guid userId, string otpCode, DateTimeOffset expiresAt, CancellationToken ct = default);
    Task<bool> ValidateAndConsumeEmailOtpAsync(Guid userId, string otpCode, DateTimeOffset currentTime, CancellationToken ct = default);

    Task<IReadOnlyList<RoleResponseDto>> GetAllRolesAsync(CancellationToken ct = default);
    Task<RoleResponseDto?> GetRoleByIdAsync(Guid roleId, CancellationToken ct = default);
    Task<bool> RoleExistsByNameAsync(string normalizedName, CancellationToken ct = default);
    Task<bool> RoleExistsByIdAsync(Guid roleId, CancellationToken ct = default);
    Task CreateRoleAsync(Guid roleId, string name, string normalizedName, string? description, CancellationToken ct = default);
    Task UpdateRoleAsync(Guid roleId, string name, string normalizedName, string? description, CancellationToken ct = default);
    Task<int> GetAssignedUserCountForRoleAsync(Guid roleId, CancellationToken ct = default);
    Task DeleteRoleAsync(Guid roleId, CancellationToken ct = default);

    Task<IReadOnlyList<PermissionResponseDto>> GetAllPermissionsAsync(CancellationToken ct = default);
    Task<PermissionResponseDto?> GetPermissionByIdAsync(Guid permissionId, CancellationToken ct = default);
    Task<PermissionResponseDto?> GetPermissionByCodeAsync(string permissionCode, CancellationToken ct = default);
    Task<bool> PermissionExistsByCodeAsync(string permissionCode, CancellationToken ct = default);
    Task CreatePermissionAsync(Guid permissionId, string permissionCode, string permissionName, string module, string? description, CancellationToken ct = default);
    Task UpdatePermissionAsync(Guid permissionId, string permissionName, string module, string? description, bool isActive, CancellationToken ct = default);
    Task DeletePermissionAsync(Guid permissionId, CancellationToken ct = default);
    Task<int> GetAssignedRoleCountForPermissionAsync(Guid permissionId, CancellationToken ct = default);

    Task<IReadOnlyList<PermissionResponseDto>> GetPermissionsByRoleIdAsync(Guid roleId, CancellationToken ct = default);
    Task<IReadOnlyList<PermissionResponseDto>> GetPermissionsByIdsAsync(IEnumerable<Guid> permissionIds, CancellationToken ct = default);
    Task<bool> RoleHasPermissionAsync(Guid roleId, Guid permissionId, CancellationToken ct = default);
    Task<bool> RoleHasPermissionByCodeAsync(Guid roleId, string permissionCode, CancellationToken ct = default);
    Task AddPermissionToRoleAsync(Guid rolePermissionId, Guid roleId, Guid permissionId, CancellationToken ct = default);
    Task RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId, CancellationToken ct = default);
    Task RemoveAllPermissionsFromRoleAsync(Guid roleId, CancellationToken ct = default);

    Task<IReadOnlyList<RoleResponseDto>> GetRolesByIdsAsync(IEnumerable<Guid> roleIds, CancellationToken ct = default);
    Task<RoleResponseDto?> GetRoleByNameAsync(string roleName, CancellationToken ct = default);
    Task<IReadOnlyList<UserRoleResponseDto>> GetRolesByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<bool> UserHasRoleAsync(Guid userId, Guid roleId, CancellationToken ct = default);
    Task<bool> UserHasActiveRoleByCodeAsync(Guid userId, string roleCode, CancellationToken ct = default);
    Task AddUserRoleAsync(Guid userRoleId, Guid userId, Guid roleId, DateTimeOffset validFrom, DateTimeOffset? validTo, Guid? assignedBy, CancellationToken ct = default);
    Task RemoveUserRoleAsync(Guid userId, Guid roleId, CancellationToken ct = default);
    Task RemoveAllRolesFromUserAsync(Guid userId, CancellationToken ct = default);

    Task<UserPermissionResponseDto?> GetUserPermissionOverrideByIdAsync(Guid userPermissionId, CancellationToken ct = default);
    Task<IReadOnlyList<UserPermissionResponseDto>> GetUserPermissionOverridesAsync(Guid userId, CancellationToken ct = default);
    Task<bool> UserPermissionOverrideExistsAsync(Guid userId, Guid permissionId, string grantType, CancellationToken ct = default);
    Task CreateUserPermissionOverrideAsync(Guid userPermissionId, Guid userId, Guid permissionId, string grantType, string? reason, DateTimeOffset validFrom, DateTimeOffset? validTo, Guid? grantedBy, CancellationToken ct = default);
    Task UpdateUserPermissionOverrideAsync(Guid userPermissionId, string grantType, string? reason, DateTimeOffset validFrom, DateTimeOffset? validTo, bool isActive, Guid? updatedBy, CancellationToken ct = default);
    Task DeleteUserPermissionOverrideAsync(Guid userPermissionId, CancellationToken ct = default);
    Task RemoveAllPermissionOverridesFromUserAsync(Guid userId, CancellationToken ct = default);
    Task AddUserPermissionOverridesBulkAsync(Guid userId, IEnumerable<UserPermissionOverrideItemDto> overrides, Guid? grantedBy, CancellationToken ct = default);
    Task<bool> HasActiveDenyOverrideAsync(Guid userId, string permissionCode, CancellationToken ct = default);
    Task<bool> HasActiveGrantOverrideAsync(Guid userId, string permissionCode, CancellationToken ct = default);

}
