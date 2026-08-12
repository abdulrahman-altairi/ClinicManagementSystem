using ClinicManagementSystem.Application.DTOs.Auth.Permissions;
using ClinicManagementSystem.Application.DTOs.Auth.Role;
using ClinicManagementSystem.Application.DTOs.Auth.Sessions;
using ClinicManagementSystem.Application.DTOs.Auth.UserPermissions;
using ClinicManagementSystem.Application.DTOs.Auth.UserRole;
using ClinicManagementSystem.Domain.Entities.Auth;
using ClinicManagementSystem.Domain.Enums;

namespace ClinicManagementSystem.Application.Interfaces.Repositories;

public interface IIdentityRepository
{

    Task<ApplicationUser?> GetUserByIdAsync(Guid userId, CancellationToken ct = default);
    Task<ApplicationUser?> GetUserByEmailOrUsernameAsync(string identifier, CancellationToken ct = default);
    Task<bool> IsEmailTakenAsync(string email, CancellationToken ct = default);
    Task<bool> IsPhoneNumberTakenAsync(string phoneNumber, CancellationToken ct = default);
    Task<bool> IsUsernameTakenAsync(string username, CancellationToken ct = default);
    Task<Guid> CreateUserAsync(ApplicationUser user, CancellationToken ct = default);
    Task UpdateUserAsync(ApplicationUser user, CancellationToken ct = default);
    Task UpdateLoginAuditAsync(Guid userId, DateTimeOffset loginTime, string? ipAddress, byte accessFailedCount, DateTimeOffset? lockoutEnd, CancellationToken ct = default);
    Task UpdatePasswordAsync(Guid userId, string passwordHash, string passwordSalt, DateTimeOffset changedAt, CancellationToken ct = default);
    Task TrackPasswordHistoryAsync(PasswordHistory history, CancellationToken ct = default);
    Task<List<string>> GetRecentPasswordHashesAsync(Guid userId, int takeLast = 5, CancellationToken ct = default);

    Task<IReadOnlyList<RoleResponseDto>> GetAllRolesAsync(CancellationToken ct = default);
    Task<RoleResponseDto?> GetRoleByIdAsync(Guid roleId, CancellationToken ct = default);
    Task<bool> RoleExistsByNameAsync(string normalizedName, CancellationToken ct = default);
    Task<bool> RoleExistsByIdAsync(Guid roleId, CancellationToken ct = default);
    Task CreateRoleAsync(Role request, CancellationToken ct = default);
    Task UpdateRoleAsync(Role request, CancellationToken ct = default);
    Task<int> GetAssignedUserCountForRoleAsync(Guid roleId, CancellationToken ct = default);
    Task DeleteRoleAsync(Guid roleId, CancellationToken ct = default);

    Task<IReadOnlyList<PermissionResponseDto>> GetAllPermissionsAsync(CancellationToken ct = default);
    Task<PermissionResponseDto?> GetPermissionByIdAsync(Guid permissionId, CancellationToken ct = default);
    Task<PermissionResponseDto?> GetPermissionByCodeAsync(string permissionCode, CancellationToken ct = default);
    Task<bool> PermissionExistsByCodeAsync(string permissionCode, CancellationToken ct = default);
    Task CreatePermissionAsync(Permission request, CancellationToken ct = default);
    Task UpdatePermissionAsync(Permission request, CancellationToken ct = default);
    Task DeletePermissionAsync(Guid permissionId, CancellationToken ct = default);
    Task<int> GetAssignedRoleCountForPermissionAsync(Guid permissionId, CancellationToken ct = default);

    Task<IReadOnlyList<PermissionResponseDto>> GetPermissionsByRoleIdAsync(Guid roleId, CancellationToken ct = default);
    Task<IReadOnlyList<PermissionResponseDto>> GetPermissionsByIdsAsync(IEnumerable<Guid> permissionIds, CancellationToken ct = default);
    Task<bool> RoleHasPermissionAsync(Guid roleId, Guid permissionId, CancellationToken ct = default);
    Task<bool> RoleHasPermissionByCodeAsync(Guid roleId, string permissionCode, CancellationToken ct = default);
    Task AddPermissionToRoleAsync(RolePermission rolePermission, CancellationToken ct = default);
    Task RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId, CancellationToken ct = default);
    Task RemoveAllPermissionsFromRoleAsync(Guid roleId, CancellationToken ct = default);
    Task AssignPermissionsToRoleAsync(Guid roleId, IEnumerable<RolePermission> rolePermissions, CancellationToken ct = default);

    Task<IReadOnlyList<RoleResponseDto>> GetRolesByIdsAsync(IEnumerable<Guid> roleIds, CancellationToken ct = default);
    Task<RoleResponseDto?> GetRoleByNameAsync(string roleName, CancellationToken ct = default);
    Task<IReadOnlyList<UserRoleResponseDto>> GetRolesByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<bool> UserHasRoleAsync(Guid userId, Guid roleId, CancellationToken ct = default);
    Task<bool> UserHasActiveRoleByCodeAsync(Guid userId, string roleCode, CancellationToken ct = default);
    Task AddUserRoleAsync(UserRole userRole, CancellationToken ct = default);
    Task RemoveUserRoleAsync(Guid userId, Guid roleId, CancellationToken ct = default);
    Task RemoveAllRolesFromUserAsync(Guid userId, CancellationToken ct = default);
    Task AssignRolesToUserAsync(Guid userId, IEnumerable<UserRole> userRoles, CancellationToken ct = default); 

    Task<UserPermissionResponseDto?> GetUserPermissionOverrideByIdAsync(Guid userPermissionId, CancellationToken ct = default);
    Task<IReadOnlyList<UserPermissionResponseDto>> GetUserPermissionOverridesAsync(Guid userId, CancellationToken ct = default);
    Task<bool> UserPermissionOverrideExistsAsync(Guid userId, Guid permissionId, string grantType, CancellationToken ct = default);
    Task CreateUserPermissionOverrideAsync(UserPermission userPermission, CancellationToken ct = default);
    Task UpdateUserPermissionOverrideAsync(UserPermission userPermission, CancellationToken ct = default);
    Task DeleteUserPermissionOverrideAsync(Guid userPermissionId, CancellationToken ct = default);
    Task RemoveAllPermissionOverridesFromUserAsync(Guid userId, CancellationToken ct = default);
    Task AddUserPermissionOverridesBulkAsync(Guid userId, IEnumerable<UserPermission> overrides, CancellationToken ct = default);
    Task<bool> AnyRoleHasPermissionByCodeAsync(IEnumerable<Guid> roleIds, string permissionCode, CancellationToken ct = default);
    Task<bool> HasActiveDenyOverrideAsync(Guid userId, string permissionCode, CancellationToken ct = default);
    Task<bool> HasActiveGrantOverrideAsync(Guid userId, string permissionCode, CancellationToken ct = default);
    Task<List<string>> GetUserRolesAsync(Guid userId, CancellationToken ct = default);
    Task<List<string>> GetUserPermissionsAsync(Guid userId, CancellationToken ct = default);

    Task CreateSessionAsync(UserSession session, CancellationToken ct = default);
    Task<UserSession?> GetSessionByRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    Task RevokeSessionAsync(Guid sessionId, string? replacedByToken, DateTimeOffset revokedAt, CancellationToken ct = default);
    Task RevokeAllUserSessionsAsync(Guid userId, DateTimeOffset revokedAt, CancellationToken ct = default);
    Task<IReadOnlyList<UserSessionResponseDto>> GetActiveSessionsByUserIdAsync(Guid userId, CancellationToken ct = default);

    Task SaveEmailOtpAsync(Guid userId, string otpCode, DateTimeOffset expiresAt, CancellationToken ct = default);
    Task<bool> ValidateAndConsumeEmailOtpAsync(Guid userId, string otpCode, DateTimeOffset currentTime, CancellationToken ct = default);
    Task SetEmailVerificationTokenAsync(Guid userId, string token, DateTimeOffset expiresAt, CancellationToken ct = default);
    Task SetPasswordResetTokenAsync(Guid userId, string token, DateTimeOffset expiresAt, CancellationToken ct = default);
    Task<ApplicationUser?> GetUserByPasswordResetTokenAsync(string token, DateTimeOffset now, CancellationToken ct = default);
    Task SaveUserTokenAsync(UserToken userToken, CancellationToken ct = default);
    Task<Guid?> GetUserIdByValidTokenAsync(string token, TokenType tokenType, CancellationToken ct = default);
    Task MarkTokenAsUsedAsync(string token, CancellationToken ct = default);



    Task<UserToken?> GetActiveTokenByHashAsync(Guid userId, string tokenHash, byte tokenTypeId, DateTimeOffset now, CancellationToken ct = default);
    Task UpdateUserTokenAsync(UserToken userToken, CancellationToken ct = default);
    Task InvalidateAllUser2FaTokensAsync(Guid userId, DateTimeOffset now, CancellationToken ct = default);
    Task<(List<Role> Roles, int TotalCount)> SearchRolesAsync(RoleSearchFilter filter, CancellationToken ct = default);
    Task<List<Role>> GetSystemRolesAsync(CancellationToken ct = default);
    Task<(List<Permission> Permissions, int TotalCount)> SearchPermissionsAsync(PermissionSearchFilter filter, CancellationToken ct = default);
}
