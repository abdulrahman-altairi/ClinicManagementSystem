using System.Data;
using ClinicManagementSystem.Application.DTOs.Auth.Permissions;
using ClinicManagementSystem.Application.DTOs.Auth.Role;
using ClinicManagementSystem.Application.DTOs.Auth.Sessions;
using ClinicManagementSystem.Application.DTOs.Auth.UserPermissions;
using ClinicManagementSystem.Application.DTOs.Auth.UserRole;
using ClinicManagementSystem.Application.Common.Interfaces;
using ClinicManagementSystem.Application.Interfaces.Repositories;
using ClinicManagementSystem.Domain.Entities.Auth;
using ClinicManagementSystem.Domain.Enums;
using ClinicManagementSystem.Infrastructure.Persistence.DataMappers;
using Microsoft.Data.SqlClient;

namespace ClinicManagementSystem.Infrastructure.Persistence.Repositories;


public sealed class IdentityRepository : IIdentityRepository
{
    private readonly IUnitOfWork _uow;

    public IdentityRepository(IUnitOfWork uow) => _uow = uow;

    // ── Shared Helper ─────────────────────────────────────────────────────────

    private async Task<SqlCommand> CreateCommandAsync(
    string storedProcedureName, 
    CancellationToken ct, 
    CommandType commandType = CommandType.StoredProcedure) 
    {
        var connection = (SqlConnection)await _uow.GetConnectionAsync(ct);
        var cmd = new SqlCommand(storedProcedureName, connection)
        {
            CommandType = commandType, 
            CommandTimeout = 30
        };

        if (_uow.Transaction is SqlTransaction tx)
            cmd.Transaction = tx;

        return cmd;
    }

    // ════════════════════════════════════════════════════════════════════════
    //  USERS & AUTHENTICATION
    // ════════════════════════════════════════════════════════════════════════

    public Task<ApplicationUser?> GetUserByEmailOrUsernameAsync(string identifier, CancellationToken ct = default)
        => Task.FromResult<ApplicationUser?>(null);

    public Task<Guid> CreateUserAsync(ApplicationUser user, CancellationToken ct = default)
        => Task.FromResult(Guid.Empty);

    public Task<ApplicationUser?> GetUserByIdAsync(Guid userId, CancellationToken ct = default)
        => Task.FromResult<ApplicationUser?>(null);

    public Task<bool> IsEmailTakenAsync(string email, CancellationToken ct = default)
        => Task.FromResult(false);

    public Task<bool> IsPhoneNumberTakenAsync(string phoneNumber, CancellationToken ct = default)
        => Task.FromResult(false);

    public Task<bool> IsUsernameTakenAsync(string username, CancellationToken ct = default)
        => Task.FromResult(false);

    public Task UpdateUserAsync(ApplicationUser user, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task UpdateLoginAuditAsync(Guid userId, DateTimeOffset loginTime, string? ipAddress, byte accessFailedCount, DateTimeOffset? lockoutEnd, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task UpdatePasswordAsync(Guid userId, string passwordHash, string passwordSalt, DateTimeOffset changedAt, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task TrackPasswordHistoryAsync(PasswordHistory history, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task<List<string>> GetRecentPasswordHashesAsync(Guid userId, int takeLast = 5, CancellationToken ct = default)
        => Task.FromResult(new List<string>());

    // ════════════════════════════════════════════════════════════════════════
    //  ROLES MANAGEMENT
    // ════════════════════════════════════════════════════════════════════════

    public Task<IReadOnlyList<RoleResponseDto>> GetAllRolesAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<RoleResponseDto>>(Array.Empty<RoleResponseDto>());

    public Task<RoleResponseDto?> GetRoleByIdAsync(Guid roleId, CancellationToken ct = default)
        => Task.FromResult<RoleResponseDto?>(null);

    public Task<bool> RoleExistsByNameAsync(string normalizedName, CancellationToken ct = default)
        => Task.FromResult(false);

    public Task<bool> RoleExistsByIdAsync(Guid roleId, CancellationToken ct = default)
        => Task.FromResult(false);

    public Task CreateRoleAsync(Role request, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task UpdateRoleAsync(Role request, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task<int> GetAssignedUserCountForRoleAsync(Guid roleId, CancellationToken ct = default)
        => Task.FromResult(0);

    public Task DeleteRoleAsync(Guid roleId, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task<(List<Role> Roles, int TotalCount)> SearchRolesAsync(RoleSearchFilter filter, CancellationToken ct = default)
        => Task.FromResult((new List<Role>(), 0));

    public Task<List<Role>> GetSystemRolesAsync(CancellationToken ct = default)
        => Task.FromResult(new List<Role>());

    // ════════════════════════════════════════════════════════════════════════
    //  PERMISSIONS MANAGEMENT
    // ════════════════════════════════════════════════════════════════════════

    public Task<IReadOnlyList<PermissionResponseDto>> GetAllPermissionsAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<PermissionResponseDto>>(Array.Empty<PermissionResponseDto>());

    public Task<PermissionResponseDto?> GetPermissionByIdAsync(Guid permissionId, CancellationToken ct = default)
        => Task.FromResult<PermissionResponseDto?>(null);

    public Task<PermissionResponseDto?> GetPermissionByCodeAsync(string permissionCode, CancellationToken ct = default)
        => Task.FromResult<PermissionResponseDto?>(null);

    public Task<bool> PermissionExistsByCodeAsync(string permissionCode, CancellationToken ct = default)
        => Task.FromResult(false);

    public Task CreatePermissionAsync(Permission request, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task UpdatePermissionAsync(Permission request, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task DeletePermissionAsync(Guid permissionId, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task<int> GetAssignedRoleCountForPermissionAsync(Guid permissionId, CancellationToken ct = default)
        => Task.FromResult(0);

    public Task<(List<Permission> Permissions, int TotalCount)> SearchPermissionsAsync(PermissionSearchFilter filter, CancellationToken ct = default)
        => Task.FromResult((new List<Permission>(), 0));

    // ════════════════════════════════════════════════════════════════════════
    //  ROLE - PERMISSIONS
    // ════════════════════════════════════════════════════════════════════════

    public Task<IReadOnlyList<PermissionResponseDto>> GetPermissionsByRoleIdAsync(Guid roleId, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<PermissionResponseDto>>(Array.Empty<PermissionResponseDto>());

    public Task<IReadOnlyList<PermissionResponseDto>> GetPermissionsByIdsAsync(IEnumerable<Guid> permissionIds, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<PermissionResponseDto>>(Array.Empty<PermissionResponseDto>());

    public Task<bool> RoleHasPermissionAsync(Guid roleId, Guid permissionId, CancellationToken ct = default)
        => Task.FromResult(false);

    public Task<bool> RoleHasPermissionByCodeAsync(Guid roleId, string permissionCode, CancellationToken ct = default)
        => Task.FromResult(false);

    public Task AddPermissionToRoleAsync(RolePermission rolePermission, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task RemoveAllPermissionsFromRoleAsync(Guid roleId, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task AssignPermissionsToRoleAsync(Guid roleId, IEnumerable<RolePermission> rolePermissions, CancellationToken ct = default)
        => Task.CompletedTask;

    // ════════════════════════════════════════════════════════════════════════
    //  USER - ROLES
    // ════════════════════════════════════════════════════════════════════════

    public Task<IReadOnlyList<RoleResponseDto>> GetRolesByIdsAsync(IEnumerable<Guid> roleIds, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<RoleResponseDto>>(Array.Empty<RoleResponseDto>());

    public Task<RoleResponseDto?> GetRoleByNameAsync(string roleName, CancellationToken ct = default)
        => Task.FromResult<RoleResponseDto?>(null);

    public Task<IReadOnlyList<UserRoleResponseDto>> GetRolesByUserIdAsync(Guid userId, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<UserRoleResponseDto>>(Array.Empty<UserRoleResponseDto>());

    public Task<bool> UserHasRoleAsync(Guid userId, Guid roleId, CancellationToken ct = default)
        => Task.FromResult(false);

    public Task<bool> UserHasActiveRoleByCodeAsync(Guid userId, string roleCode, CancellationToken ct = default)
        => Task.FromResult(false);

    public Task AddUserRoleAsync(UserRole userRole, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task RemoveUserRoleAsync(Guid userId, Guid roleId, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task RemoveAllRolesFromUserAsync(Guid userId, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task AssignRolesToUserAsync(Guid userId, IEnumerable<UserRole> userRoles, CancellationToken ct = default)
        => Task.CompletedTask;

    // ════════════════════════════════════════════════════════════════════════
    //  USER - PERMISSION OVERRIDES
    // ════════════════════════════════════════════════════════════════════════

    public Task<UserPermissionResponseDto?> GetUserPermissionOverrideByIdAsync(Guid userPermissionId, CancellationToken ct = default)
        => Task.FromResult<UserPermissionResponseDto?>(null);

    public Task<IReadOnlyList<UserPermissionResponseDto>> GetUserPermissionOverridesAsync(Guid userId, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<UserPermissionResponseDto>>(Array.Empty<UserPermissionResponseDto>());

    public Task<bool> UserPermissionOverrideExistsAsync(Guid userId, Guid permissionId, string grantType, CancellationToken ct = default)
        => Task.FromResult(false);

    public Task CreateUserPermissionOverrideAsync(UserPermission userPermission, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task UpdateUserPermissionOverrideAsync(UserPermission userPermission, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task DeleteUserPermissionOverrideAsync(Guid userPermissionId, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task RemoveAllPermissionOverridesFromUserAsync(Guid userId, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task AddUserPermissionOverridesBulkAsync(Guid userId, IEnumerable<UserPermission> overrides, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task<bool> AnyRoleHasPermissionByCodeAsync(IEnumerable<Guid> roleIds, string permissionCode, CancellationToken ct = default)
        => Task.FromResult(false);

    public Task<bool> HasActiveDenyOverrideAsync(Guid userId, string permissionCode, CancellationToken ct = default)
        => Task.FromResult(false);

    public Task<bool> HasActiveGrantOverrideAsync(Guid userId, string permissionCode, CancellationToken ct = default)
        => Task.FromResult(false);

    public Task<List<string>> GetUserRolesAsync(Guid userId, CancellationToken ct = default)
        => Task.FromResult(new List<string> { "User" });

    public async Task<List<string>> GetUserPermissionsAsync(Guid userId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT DISTINCT p.PermissionCode, CAST(1 AS BIT) AS IsGranted
            FROM   auth.Permissions        p
            JOIN   auth.RolePermissions    rp ON rp.PermissionId = p.PermissionId
            JOIN   auth.UserRoles          ur ON ur.RoleId       = rp.RoleId
            WHERE  ur.UserId  = @UserId
              AND  p.IsActive = 1
              AND  p.IsDeleted = 0
              AND  (ur.ValidTo IS NULL OR ur.ValidTo > SYSDATETIMEOFFSET())

            UNION ALL

            SELECT p.PermissionCode,
                   CASE WHEN up.GrantType = 'GRANT' THEN CAST(1 AS BIT)
                        ELSE CAST(0 AS BIT) END AS IsGranted
            FROM   auth.UserPermissions up
            JOIN   auth.Permissions     p  ON p.PermissionId = up.PermissionId
            WHERE  up.UserId    = @UserId
              AND  up.IsActive  = 1
              AND  p.IsActive   = 1
              AND  p.IsDeleted  = 0
              AND  (up.ValidTo IS NULL OR up.ValidTo > SYSDATETIMEOFFSET());
            """;

        await using var cmd = await CreateCommandAsync(sql, ct);
        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });

        var granted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var denied = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var code = reader.GetString(0);
            var isGranted = reader.GetBoolean(1);

            if (isGranted) granted.Add(code);
            else denied.Add(code);
        }

        granted.ExceptWith(denied);
        return granted.ToList();
    }

    // ════════════════════════════════════════════════════════════════════════
    //  SESSIONS MANAGEMENT
    // ════════════════════════════════════════════════════════════════════════

    public Task CreateSessionAsync(UserSession session, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task<UserSession?> GetSessionByRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
        => Task.FromResult<UserSession?>(null);

    public Task RevokeSessionAsync(Guid sessionId, string? replacedByToken, DateTimeOffset revokedAt, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task RevokeAllUserSessionsAsync(Guid userId, DateTimeOffset revokedAt, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task<IReadOnlyList<UserSessionResponseDto>> GetActiveSessionsByUserIdAsync(Guid userId, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<UserSessionResponseDto>>(Array.Empty<UserSessionResponseDto>());

    // ════════════════════════════════════════════════════════════════════════
    //  TOKENS & OTP MANAGEMENT
    // ════════════════════════════════════════════════════════════════════════

    public Task SaveEmailOtpAsync(Guid userId, string otpCode, DateTimeOffset expiresAt, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task<bool> ValidateAndConsumeEmailOtpAsync(Guid userId, string otpCode, DateTimeOffset currentTime, CancellationToken ct = default)
        => Task.FromResult(false);

    public Task SetEmailVerificationTokenAsync(Guid userId, string token, DateTimeOffset expiresAt, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task SetPasswordResetTokenAsync(Guid userId, string token, DateTimeOffset expiresAt, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task<ApplicationUser?> GetUserByPasswordResetTokenAsync(string token, DateTimeOffset now, CancellationToken ct = default)
        => Task.FromResult<ApplicationUser?>(null);

    public Task SaveUserTokenAsync(UserToken userToken, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task<Guid?> GetUserIdByValidTokenAsync(string token, TokenType tokenType, CancellationToken ct = default)
        => Task.FromResult<Guid?>(null);

    public Task MarkTokenAsUsedAsync(string token, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task<UserToken?> GetActiveTokenByHashAsync(Guid userId, string tokenHash, byte tokenTypeId, DateTimeOffset now, CancellationToken ct = default)
        => Task.FromResult<UserToken?>(null);

    public Task UpdateUserTokenAsync(UserToken userToken, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task InvalidateAllUser2FaTokensAsync(Guid userId, DateTimeOffset now, CancellationToken ct = default)
        => Task.CompletedTask;
}