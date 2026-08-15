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

    public async Task<ApplicationUser?> GetUserByEmailOrUsernameAsync(string identifier, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            return null;
    
        await using var cmd = await CreateCommandAsync("auth.sp_GetUserByEmailOrUsername", ct, CommandType.StoredProcedure);
    
        cmd.Parameters.Add(new SqlParameter("@Identifier", SqlDbType.NVarChar, 256)
        {
            Value = identifier.Trim()
        });
    
        await using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow, ct);
    
        return await reader.ReadAsync(ct)
            ? UserMapper.MapToApplicationUser(reader)
            : null;
    }

    public async Task<Guid> CreateUserAsync(ApplicationUser user, CancellationToken ct = default)
    {
        await using var cmd = await CreateCommandAsync("auth.sp_CreateUser", ct, CommandType.StoredProcedure);

        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = user.Id });
        cmd.Parameters.Add(new SqlParameter("@Username", SqlDbType.NVarChar, 100) { Value = user.Username });
        cmd.Parameters.Add(new SqlParameter("@NormalizedUsername", SqlDbType.NVarChar, 100) { Value = user.NormalizedUsername });
        cmd.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 256) { Value = user.Email });
        cmd.Parameters.Add(new SqlParameter("@NormalizedEmail", SqlDbType.NVarChar, 256) { Value = user.NormalizedEmail });
        cmd.Parameters.Add(new SqlParameter("@PasswordHash", SqlDbType.NVarChar, 512) { Value = user.PasswordHash });
        cmd.Parameters.Add(new SqlParameter("@PasswordSalt", SqlDbType.NVarChar, 256) { Value = user.PasswordSalt });
        cmd.Parameters.Add(new SqlParameter("@FirstName", SqlDbType.NVarChar, 100) { Value = user.FirstName });
        cmd.Parameters.Add(new SqlParameter("@LastName", SqlDbType.NVarChar, 100) { Value = user.LastName });

        cmd.Parameters.Add(new SqlParameter("@PhoneNumber", SqlDbType.VarChar, 30) { Value = (object?)user.PhoneNumber ?? DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@PhoneVerified", SqlDbType.Bit) { Value = user.PhoneVerified });
        cmd.Parameters.Add(new SqlParameter("@EmailVerified", SqlDbType.Bit) { Value = user.EmailVerified });
        cmd.Parameters.Add(new SqlParameter("@TwoFactorEnabled", SqlDbType.Bit) { Value = user.TwoFactorEnabled });
        cmd.Parameters.Add(new SqlParameter("@TwoFactorSecret", SqlDbType.NVarChar, 256) { Value = (object?)user.TwoFactorSecret ?? DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@LockoutEnabled", SqlDbType.Bit) { Value = user.LockoutEnabled });
        cmd.Parameters.Add(new SqlParameter("@LockoutEnd", SqlDbType.DateTimeOffset) { Value = (object?)user.LockoutEnd ?? DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@AccessFailedCount", SqlDbType.TinyInt) { Value = user.AccessFailedCount });
        cmd.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.Bit) { Value = user.IsActive });
        cmd.Parameters.Add(new SqlParameter("@AvatarUrl", SqlDbType.NVarChar, 500) { Value = (object?)user.AvatarUrl ?? DBNull.Value });

        cmd.Parameters.Add(new SqlParameter("@CreatedAt", SqlDbType.DateTimeOffset) { Value = user.CreatedAt });
        cmd.Parameters.Add(new SqlParameter("@CreatedBy", SqlDbType.UniqueIdentifier) { Value = (object?)user.CreatedBy ?? DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@UpdatedAt", SqlDbType.DateTimeOffset) { Value = user.UpdatedAt });
        cmd.Parameters.Add(new SqlParameter("@UpdatedBy", SqlDbType.UniqueIdentifier) { Value = (object?)user.UpdatedBy ?? DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@IsDeleted", SqlDbType.Bit) { Value = user.IsDeleted });

        var result = await cmd.ExecuteScalarAsync(ct);

        return result is Guid createdId ? createdId : user.Id;
    }
    
    public async Task<ApplicationUser?> GetUserByIdAsync(Guid userId, CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
            return null;

        await using var cmd = await CreateCommandAsync("auth.sp_GetUserById", ct, CommandType.StoredProcedure);

        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier)
        {
            Value = userId
        });

        await using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow, ct);

        return await reader.ReadAsync(ct)
            ? UserMapper.MapToApplicationUser(reader)
            : null;
    }

    public async Task<bool> IsEmailTakenAsync(string email, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;
    
        await using var cmd = await CreateCommandAsync("auth.sp_IsEmailTaken", ct, CommandType.StoredProcedure);
    
        cmd.Parameters.Add(new SqlParameter("@NormalizedEmail", SqlDbType.NVarChar, 256)
        {
            Value = email.Trim().ToUpperInvariant()
        });
    
        var result = await cmd.ExecuteScalarAsync(ct);
    
        return result is bool isTaken && isTaken;
    }

    public async Task<bool> IsPhoneNumberTakenAsync(string phoneNumber, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        await using var cmd = await CreateCommandAsync("auth.sp_IsPhoneNumberTaken", ct, CommandType.StoredProcedure);

        cmd.Parameters.Add(new SqlParameter("@PhoneNumber", SqlDbType.VarChar, 30)
        {
            Value = phoneNumber.Trim()
        });

        var result = await cmd.ExecuteScalarAsync(ct);

        return result is bool isTaken && isTaken;
    }

    public async Task<bool> IsUsernameTakenAsync(string username, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(username))
            return false;
    
        await using var cmd = await CreateCommandAsync("auth.sp_IsUsernameTaken", ct, CommandType.StoredProcedure);
    
        cmd.Parameters.Add(new SqlParameter("@NormalizedUsername", SqlDbType.NVarChar, 100)
        {
            Value = username.Trim().ToUpperInvariant()
        });
    
        var result = await cmd.ExecuteScalarAsync(ct);
    
        return result is bool isTaken && isTaken;
    }

    public async Task UpdateUserAsync(ApplicationUser user, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        await using var cmd = await CreateCommandAsync("auth.sp_UpdateUser", ct, CommandType.StoredProcedure);

        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = user.Id });
        cmd.Parameters.Add(new SqlParameter("@Username", SqlDbType.NVarChar, 100) { Value = user.Username });
        cmd.Parameters.Add(new SqlParameter("@NormalizedUsername", SqlDbType.NVarChar, 100) { Value = user.NormalizedUsername });
        cmd.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 256) { Value = user.Email });
        cmd.Parameters.Add(new SqlParameter("@NormalizedEmail", SqlDbType.NVarChar, 256) { Value = user.NormalizedEmail });
        cmd.Parameters.Add(new SqlParameter("@PasswordHash", SqlDbType.NVarChar, 512) { Value = user.PasswordHash });
        cmd.Parameters.Add(new SqlParameter("@PasswordSalt", SqlDbType.NVarChar, 256) { Value = user.PasswordSalt });
        cmd.Parameters.Add(new SqlParameter("@FirstName", SqlDbType.NVarChar, 100) { Value = user.FirstName });
        cmd.Parameters.Add(new SqlParameter("@LastName", SqlDbType.NVarChar, 100) { Value = user.LastName });
        cmd.Parameters.Add(new SqlParameter("@PhoneNumber", SqlDbType.VarChar, 30) { Value = (object?)user.PhoneNumber ?? DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@PhoneVerified", SqlDbType.Bit) { Value = user.PhoneVerified });
        cmd.Parameters.Add(new SqlParameter("@EmailVerified", SqlDbType.Bit) { Value = user.EmailVerified });
        cmd.Parameters.Add(new SqlParameter("@TwoFactorEnabled", SqlDbType.Bit) { Value = user.TwoFactorEnabled });
        cmd.Parameters.Add(new SqlParameter("@TwoFactorSecret", SqlDbType.NVarChar, 256) { Value = (object?)user.TwoFactorSecret ?? DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@LockoutEnabled", SqlDbType.Bit) { Value = user.LockoutEnabled });
        cmd.Parameters.Add(new SqlParameter("@LockoutEnd", SqlDbType.DateTimeOffset) { Value = (object?)user.LockoutEnd ?? DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@AccessFailedCount", SqlDbType.TinyInt) { Value = user.AccessFailedCount });
        cmd.Parameters.Add(new SqlParameter("@LastLoginUtc", SqlDbType.DateTimeOffset) { Value = (object?)user.LastLoginUtc ?? DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@LastLoginIp", SqlDbType.VarChar, 45) { Value = (object?)user.LastLoginIp ?? DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@PasswordChangedUtc", SqlDbType.DateTimeOffset) { Value = (object?)user.PasswordChangedUtc ?? DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.Bit) { Value = user.IsActive });
        cmd.Parameters.Add(new SqlParameter("@AvatarUrl", SqlDbType.NVarChar, 500) { Value = (object?)user.AvatarUrl ?? DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@IsDeleted", SqlDbType.Bit) { Value = user.IsDeleted });
        cmd.Parameters.Add(new SqlParameter("@UpdatedAt", SqlDbType.DateTimeOffset) { Value = user.UpdatedAt });
        cmd.Parameters.Add(new SqlParameter("@UpdatedBy", SqlDbType.UniqueIdentifier) { Value = (object?)user.UpdatedBy ?? DBNull.Value });

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<bool> UpdateUserProfileAsync(ApplicationUser user, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        await using var cmd = await CreateCommandAsync("auth.sp_UpdateUserProfile", ct, CommandType.StoredProcedure);

        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = user.Id });
        cmd.Parameters.Add(new SqlParameter("@FirstName", SqlDbType.NVarChar, 100) { Value = user.FirstName });
        cmd.Parameters.Add(new SqlParameter("@LastName", SqlDbType.NVarChar, 100) { Value = user.LastName });
        cmd.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 256) { Value = user.Email });
        cmd.Parameters.Add(new SqlParameter("@NormalizedEmail", SqlDbType.NVarChar, 256) { Value = user.NormalizedEmail });
        cmd.Parameters.Add(new SqlParameter("@PhoneNumber", SqlDbType.VarChar, 30) { Value = (object?)user.PhoneNumber ?? DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@UpdatedAt", SqlDbType.DateTimeOffset) { Value = user.UpdatedAt });
        cmd.Parameters.Add(new SqlParameter("@UpdatedBy", SqlDbType.UniqueIdentifier) { Value = (object?)user.UpdatedBy ?? DBNull.Value });

        var rowsAffected = await cmd.ExecuteNonQueryAsync(ct);

        return rowsAffected > 0;
    }

    public async Task UpdateLoginAuditAsync(Guid userId, DateTimeOffset loginTime, string? ipAddress, byte accessFailedCount, DateTimeOffset? lockoutEnd, CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
            return;

        await using var cmd = await CreateCommandAsync("auth.sp_UpdateLoginAudit", ct, CommandType.StoredProcedure);

        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });
        cmd.Parameters.Add(new SqlParameter("@LastLoginUtc", SqlDbType.DateTimeOffset) { Value = loginTime });
        cmd.Parameters.Add(new SqlParameter("@LastLoginIp", SqlDbType.VarChar, 45) { Value = (object?)ipAddress ?? DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@AccessFailedCount", SqlDbType.TinyInt) { Value = accessFailedCount });
        cmd.Parameters.Add(new SqlParameter("@LockoutEnd", SqlDbType.DateTimeOffset) { Value = (object?)lockoutEnd ?? DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@UpdatedAt", SqlDbType.DateTimeOffset) { Value = DateTimeOffset.UtcNow });

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task UpdatePasswordAsync(Guid userId, string passwordHash, string passwordSalt, DateTimeOffset changedAt, CancellationToken ct = default)
    {
        await using var cmd = await CreateCommandAsync("auth.sp_UpdatePassword", ct, CommandType.StoredProcedure);

        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });
        cmd.Parameters.Add(new SqlParameter("@PasswordHash", SqlDbType.NVarChar, 512) { Value = passwordHash });
        cmd.Parameters.Add(new SqlParameter("@PasswordSalt", SqlDbType.NVarChar, 256) { Value = passwordSalt });
        cmd.Parameters.Add(new SqlParameter("@PasswordChangedUtc", SqlDbType.DateTimeOffset) { Value = changedAt });

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task TrackPasswordHistoryAsync(PasswordHistory history, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(history);

        await using var cmd = await CreateCommandAsync("auth.sp_TrackPasswordHistory", ct, CommandType.StoredProcedure);

        var historyId = history.Id == Guid.Empty ? Guid.NewGuid() : history.Id;

        cmd.Parameters.Add(new SqlParameter("@PasswordHistoryId", SqlDbType.UniqueIdentifier) { Value = historyId });
        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = history.UserId });
        cmd.Parameters.Add(new SqlParameter("@PasswordHash", SqlDbType.NVarChar, 512) { Value = history.PasswordHash });
        cmd.Parameters.Add(new SqlParameter("@ChangedAtUtc", SqlDbType.DateTimeOffset) { Value = history.ChangedAtUtc });
        cmd.Parameters.Add(new SqlParameter("@ChangedByIp", SqlDbType.VarChar, 45) { Value = (object?)history.ChangedByIp ?? DBNull.Value });

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<List<string>> GetRecentPasswordHashesAsync(Guid userId, int takeLast = 5, CancellationToken ct = default)
    {
        if (takeLast <= 0)
            return new List<string>();

        await using var cmd = await CreateCommandAsync("auth.sp_GetRecentPasswordHashes", ct, CommandType.StoredProcedure);

        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });
        cmd.Parameters.Add(new SqlParameter("@TakeLast", SqlDbType.Int) { Value = takeLast });

        var hashes = new List<string>(takeLast);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            if (!reader.IsDBNull(0))
            {
                hashes.Add(reader.GetString(0));
            }
        }

        return hashes;
    }

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
        // const string sql = """
        //     SELECT DISTINCT p.PermissionCode, CAST(1 AS BIT) AS IsGranted
        //     FROM   auth.Permissions        p
        //     JOIN   auth.RolePermissions    rp ON rp.PermissionId = p.PermissionId
        //     JOIN   auth.UserRoles          ur ON ur.RoleId       = rp.RoleId
        //     WHERE  ur.UserId  = @UserId
        //       AND  p.IsActive = 1
        //       AND  p.IsDeleted = 0
        //       AND  (ur.ValidTo IS NULL OR ur.ValidTo > SYSDATETIMEOFFSET())

        //     UNION ALL

        //     SELECT p.PermissionCode,
        //            CASE WHEN up.GrantType = 'GRANT' THEN CAST(1 AS BIT)
        //                 ELSE CAST(0 AS BIT) END AS IsGranted
        //     FROM   auth.UserPermissions up
        //     JOIN   auth.Permissions     p  ON p.PermissionId = up.PermissionId
        //     WHERE  up.UserId    = @UserId
        //       AND  up.IsActive  = 1
        //       AND  p.IsActive   = 1
        //       AND  p.IsDeleted  = 0
        //       AND  (up.ValidTo IS NULL OR up.ValidTo > SYSDATETIMEOFFSET());
        //     """;

        // await using var cmd = await CreateCommandAsync(sql, ct);
        // cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });

        // var granted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        // var denied = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // await using var reader = await cmd.ExecuteReaderAsync(ct);
        // while (await reader.ReadAsync(ct))
        // {
        //     var code = reader.GetString(0);
        //     var isGranted = reader.GetBoolean(1);

        //     if (isGranted) granted.Add(code);
        //     else denied.Add(code);
        // }

        // granted.ExceptWith(denied);
        return await Task.FromResult(new List<string> { "ViewDashboard", "EditProfile" });
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