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
using ClinicManagementSystem.Application.DTOs.Auth.Users;

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

        if (await reader.ReadAsync(ct))
        {
            var ordinals = new UserDataMapperExtensions.UserOrdinals(reader);
            return reader.MapToApplicationUser(ordinals);
        }

        return null;
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
        cmd.Parameters.Add(new SqlParameter("@UpdatedAt", SqlDbType.DateTimeOffset) { Value = (object?)user.UpdatedAt ?? DBNull.Value });
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

        if (await reader.ReadAsync(ct))
        {
            var ordinals = new UserDataMapperExtensions.UserOrdinals(reader);
            return reader.MapToApplicationUser(ordinals);
        }

        return null;
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

        cmd.Parameters.Add(new SqlParameter("@PasswordHistoryId", SqlDbType.UniqueIdentifier) { Value = history.Id });
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

    public async Task<(IReadOnlyList<ApplicationUser> Users, Dictionary<Guid, List<string>> UserRoles, int TotalCount)> GetPagedUsersAsync(UserQueryParams queryParams, CancellationToken ct = default)
    {
        await using var cmd = await CreateCommandAsync("auth.sp_GetPagedUsers", ct, CommandType.StoredProcedure);

        cmd.Parameters.Add(new SqlParameter("@SearchTerm", SqlDbType.NVarChar, 100) { Value = (object?)queryParams.SearchTerm ?? DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.Bit) { Value = (object?)queryParams.IsActive ?? DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@SortBy", SqlDbType.VarChar, 50) { Value = (object?)queryParams.SortBy ?? DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@IsDescending", SqlDbType.Bit) { Value = queryParams.IsDescending });
        cmd.Parameters.Add(new SqlParameter("@PageNumber", SqlDbType.Int) { Value = queryParams.PageNumber });
        cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = queryParams.PageSize });

        var users = new List<ApplicationUser>();
        var userRolesMap = new Dictionary<Guid, List<string>>();
        int totalCount = 0;

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        var userOrdinals = new UserDataMapperExtensions.UserOrdinals(reader);
        int totalCountOrd = reader.GetOrdinal("TotalCount");

        while (await reader.ReadAsync(ct))
        {
            if (totalCount == 0)
            {
                totalCount = reader.GetInt32(totalCountOrd);
            }

            var user = reader.MapToApplicationUser(userOrdinals);
            users.Add(user);
            userRolesMap[user.Id] = new List<string>();
        }

        if (await reader.NextResultAsync(ct))
        {
            int userIdOrd = reader.GetOrdinal("UserId");
            int roleNameOrd = reader.GetOrdinal("RoleName");

            while (await reader.ReadAsync(ct))
            {
                var userId = reader.GetGuid(userIdOrd);
                var roleName = reader.GetString(roleNameOrd);

                if (userRolesMap.TryGetValue(userId, out var roleList))
                {
                    roleList.Add(roleName);
                }
            }
        }

        return (users.AsReadOnly(), userRolesMap, totalCount);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  ROLES MANAGEMENT
    // ════════════════════════════════════════════════════════════════════════

    public async Task<IReadOnlyList<Role>> GetAllRolesAsync(CancellationToken ct = default)
    {
        await using var cmd = await CreateCommandAsync("auth.sp_GetAllRoles", ct, CommandType.StoredProcedure);

        var roles = new List<Role>();

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        var ordinals = new RoleDataMapperExtensions.RoleOrdinals(reader);

        while (await reader.ReadAsync(ct))
        {
            roles.Add(RoleDataMapperExtensions.MapToRole(reader, ordinals));
        }

        return roles.AsReadOnly();
    }

    public async Task<Role?> GetRoleByIdAsync(Guid roleId, CancellationToken ct = default)
    {
        if (roleId == Guid.Empty)
            return null;

        await using var cmd = await CreateCommandAsync("auth.sp_GetRoleById", ct, CommandType.StoredProcedure);
        cmd.Parameters.Add(new SqlParameter("@RoleId", SqlDbType.UniqueIdentifier) { Value = roleId });

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        if (!await reader.ReadAsync(ct))
            return null;

        return RoleDataMapperExtensions.MapToRole(reader, new RoleDataMapperExtensions.RoleOrdinals(reader));
    }

    public async Task<bool> RoleExistsByNameAsync(string normalizedName, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(normalizedName))
            return false;

        await using var cmd = await CreateCommandAsync("auth.sp_RoleExistsByName", ct);
        cmd.Parameters.Add(new SqlParameter("@NormalizedName", SqlDbType.NVarChar, 100) 
        { 
            Value = normalizedName 
        });

        var result = await cmd.ExecuteScalarAsync(ct);

        return result is not null && Convert.ToBoolean(result);
    }

    public async Task<bool> RoleExistsByIdAsync(Guid roleId, CancellationToken ct = default)
    {
        if (roleId == Guid.Empty)
            return false;

        await using var cmd = await CreateCommandAsync("auth.sp_RoleExistsById", ct);
        cmd.Parameters.Add(new SqlParameter("@RoleId", SqlDbType.UniqueIdentifier) 
        { 
            Value = roleId 
        });

        var result = await cmd.ExecuteScalarAsync(ct);

        return result is not null && Convert.ToBoolean(result);
    }

    public async Task CreateRoleAsync(Role request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Id == Guid.Empty)
        {
            request.Id = Guid.NewGuid();
        }

        await using var cmd = await CreateCommandAsync("auth.sp_CreateRole", ct);

        cmd.Parameters.Add(new SqlParameter("@RoleId", SqlDbType.UniqueIdentifier) { Value = request.Id });
        cmd.Parameters.Add(new SqlParameter("@RoleName", SqlDbType.NVarChar, 100) { Value = request.RoleName });
        cmd.Parameters.Add(new SqlParameter("@NormalizedName", SqlDbType.NVarChar, 100) { Value = request.NormalizedName });
        cmd.Parameters.Add(new SqlParameter("@Description", SqlDbType.NVarChar, 500) 
        { 
            Value = (object?)request.Description ?? DBNull.Value 
        });
        cmd.Parameters.Add(new SqlParameter("@IsSystemRole", SqlDbType.Bit) { Value = request.IsSystemRole });
        cmd.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.Bit) { Value = request.IsActive });
        cmd.Parameters.Add(new SqlParameter("@CreatedBy", SqlDbType.UniqueIdentifier) 
        { 
            Value = (object?)request.CreatedBy ?? DBNull.Value 
        });

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task UpdateRoleAsync(Role request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
    
        if (request.Id == Guid.Empty)
            throw new ArgumentException("Role ID cannot be empty.", nameof(request));
    
        await using var cmd = await CreateCommandAsync("auth.sp_UpdateRole", ct);
    
        cmd.Parameters.Add(new SqlParameter("@RoleId", SqlDbType.UniqueIdentifier) { Value = request.Id });
        cmd.Parameters.Add(new SqlParameter("@RoleName", SqlDbType.NVarChar, 100) { Value = request.RoleName });
        cmd.Parameters.Add(new SqlParameter("@NormalizedName", SqlDbType.NVarChar, 100) { Value = request.NormalizedName });
        cmd.Parameters.Add(new SqlParameter("@Description", SqlDbType.NVarChar, 500) 
        { 
            Value = (object?)request.Description ?? DBNull.Value 
        });
        cmd.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.Bit) { Value = request.IsActive });
        cmd.Parameters.Add(new SqlParameter("@UpdatedBy", SqlDbType.UniqueIdentifier) 
        { 
            Value = (object?)request.UpdatedBy ?? DBNull.Value 
        });
    
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<int> GetAssignedUserCountForRoleAsync(Guid roleId, CancellationToken ct = default)
    {
        if (roleId == Guid.Empty)
            return 0;

        await using var cmd = await CreateCommandAsync("auth.sp_GetAssignedUserCountForRole", ct);

        cmd.Parameters.Add(new SqlParameter("@RoleId", SqlDbType.UniqueIdentifier) { Value = roleId });

        var result = await cmd.ExecuteScalarAsync(ct);

        return result is null or DBNull ? 0 : Convert.ToInt32(result);
    }

    public async Task DeleteRoleAsync(Guid roleId, CancellationToken ct = default)
    {
        if (roleId == Guid.Empty)
            throw new ArgumentException("Role ID cannot be empty.", nameof(roleId));
    
        await using var cmd = await CreateCommandAsync("auth.sp_DeleteRole", ct);
    
        cmd.Parameters.Add(new SqlParameter("@RoleId", SqlDbType.UniqueIdentifier) { Value = roleId });
    
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<(List<Role> Roles, int TotalCount)> SearchRolesAsync(RoleSearchFilter filter, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        await using var cmd = await CreateCommandAsync("auth.sp_SearchRoles", ct);

        cmd.Parameters.Add(new SqlParameter("@SearchTerm", SqlDbType.NVarChar, 100) 
        { 
            Value = string.IsNullOrWhiteSpace(filter.SearchTerm) ? DBNull.Value : filter.SearchTerm.Trim() 
        });
        cmd.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.Bit) 
        { 
            Value = (object?)filter.IsActive ?? DBNull.Value 
        });
        cmd.Parameters.Add(new SqlParameter("@IsSystemRole", SqlDbType.Bit) 
        { 
            Value = (object?)filter.IsSystem ?? DBNull.Value 
        });
        cmd.Parameters.Add(new SqlParameter("@PageNumber", SqlDbType.Int) { Value = filter.PageNumber < 1 ? 1 : filter.PageNumber });
        cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = filter.PageSize < 1 ? 10 : filter.PageSize });
        cmd.Parameters.Add(new SqlParameter("@SortBy", SqlDbType.NVarChar, 50) 
        { 
            Value = string.IsNullOrWhiteSpace(filter.SortBy) ? "CreatedAt" : filter.SortBy 
        });
        cmd.Parameters.Add(new SqlParameter("@SortDirection", SqlDbType.NVarChar, 4) 
        { 
            Value = string.IsNullOrWhiteSpace(filter.SortDirection) ? "DESC" : filter.SortDirection.ToUpperInvariant() 
        });

        var roles = new List<Role>();
        var totalCount = 0;

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        var ordinals = new RoleDataMapperExtensions.RoleOrdinals(reader);
        var totalCountOrd = reader.GetOrdinal("TotalCount");

        while (await reader.ReadAsync(ct))
        {

            if (totalCount == 0 && !reader.IsDBNull(totalCountOrd))
            {
                totalCount = reader.GetInt32(totalCountOrd);
            }

            roles.Add(RoleDataMapperExtensions.MapToRole(reader, ordinals));
        }
        return (roles, totalCount);
    }

    public async Task<List<Role>> GetSystemRolesAsync(CancellationToken ct = default)
    {
        await using var cmd = await CreateCommandAsync("auth.sp_GetSystemRoles", ct);

        var roles = new List<Role>();

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        var ordinals = new RoleDataMapperExtensions.RoleOrdinals(reader);

        while (await reader.ReadAsync(ct))
        {
            roles.Add(RoleDataMapperExtensions.MapToRole(reader, ordinals));
        }

        return roles;
    }

    // ════════════════════════════════════════════════════════════════════════
    //  PERMISSIONS MANAGEMENT
    // ════════════════════════════════════════════════════════════════════════

    public async Task<IReadOnlyList<Permission>> GetAllPermissionsAsync(CancellationToken ct = default)
    {
        await using var cmd = await CreateCommandAsync("auth.sp_GetAllPermissions", ct);

        var permissions = new List<Permission>();

        await using var reader = await cmd.ExecuteReaderAsync(ct);


        var ordinals = new PermissionDataMapperExtensions.PermissionOrdinals(reader);

        while (await reader.ReadAsync(ct))
        {
            permissions.Add(reader.MapToPermission(ordinals));
        }

        return permissions;
    }
    
    public async Task<Permission?> GetPermissionByIdAsync(Guid permissionId, CancellationToken ct = default)
    {
        await using var cmd = await CreateCommandAsync("auth.sp_GetPermissionById", ct);

        cmd.Parameters.Add(new SqlParameter("@PermissionId", SqlDbType.UniqueIdentifier) { Value = permissionId });

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        if (await reader.ReadAsync(ct))
        {
            return reader.MapToPermission();
        }

        return null;
    }

    public async Task<Permission?> GetPermissionByCodeAsync(string permissionCode, CancellationToken ct = default)
    {
        await using var cmd = await CreateCommandAsync("auth.sp_GetPermissionByCode", ct);

        cmd.Parameters.Add(new SqlParameter("@PermissionCode", SqlDbType.NVarChar, 150) { Value = permissionCode });

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        if (await reader.ReadAsync(ct))
        {
            return reader.MapToPermission();
        }

        return null;
    }

    public async Task<bool> PermissionExistsByCodeAsync(string permissionCode, CancellationToken ct = default)
    {
        await using var cmd = await CreateCommandAsync("auth.sp_PermissionExistsByCode", ct);

        cmd.Parameters.Add(new SqlParameter("@PermissionCode", SqlDbType.NVarChar, 150) { Value = permissionCode });

        var result = await cmd.ExecuteScalarAsync(ct);

        return result is not null && Convert.ToBoolean(result);
    }

    public async Task CreatePermissionAsync(Permission request, CancellationToken ct = default)
    {
        await using var cmd = await CreateCommandAsync("auth.sp_CreatePermission", ct);

        var idParam = new SqlParameter("@PermissionId", SqlDbType.UniqueIdentifier)
        {
            Direction = ParameterDirection.InputOutput,
            Value = request.Id == Guid.Empty ? DBNull.Value : request.Id
        };
        cmd.Parameters.Add(idParam);

        cmd.Parameters.Add(new SqlParameter("@PermissionCode", SqlDbType.NVarChar, 100) { Value = request.PermissionCode });
        cmd.Parameters.Add(new SqlParameter("@PermissionName", SqlDbType.NVarChar, 100) { Value = request.PermissionName });
        cmd.Parameters.Add(new SqlParameter("@Module", SqlDbType.NVarChar, 50) { Value = request.Module });
        cmd.Parameters.Add(new SqlParameter("@Description", SqlDbType.NVarChar, 500) 
        { 
            Value = string.IsNullOrWhiteSpace(request.Description) ? DBNull.Value : request.Description 
        });
        cmd.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.Bit) { Value = request.IsActive });
        cmd.Parameters.Add(new SqlParameter("@CreatedBy", SqlDbType.UniqueIdentifier) 
        { 
            Value = (object?)request.CreatedBy ?? DBNull.Value 
        });

        await cmd.ExecuteNonQueryAsync(ct);

        if (idParam.Value != DBNull.Value && idParam.Value is Guid generatedId)
        {
            request.Id = generatedId;
        }
    }

    public async Task UpdatePermissionAsync(Permission request, CancellationToken ct = default)
    {
        await using var cmd = await CreateCommandAsync("auth.sp_UpdatePermission", ct);

        cmd.Parameters.Add(new SqlParameter("@PermissionId", SqlDbType.UniqueIdentifier) { Value = request.Id });
        cmd.Parameters.Add(new SqlParameter("@PermissionName", SqlDbType.NVarChar, 200) { Value = request.PermissionName });
        cmd.Parameters.Add(new SqlParameter("@Module", SqlDbType.NVarChar, 100) { Value = request.Module });
        cmd.Parameters.Add(new SqlParameter("@Description", SqlDbType.NVarChar, 500) 
        { 
            Value = string.IsNullOrWhiteSpace(request.Description) ? DBNull.Value : request.Description 
        });
        cmd.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.Bit) { Value = request.IsActive });
        cmd.Parameters.Add(new SqlParameter("@UpdatedBy", SqlDbType.UniqueIdentifier) 
        { 
            Value = (object?)request.UpdatedBy ?? DBNull.Value 
        });

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task DeletePermissionAsync(Guid permissionId, Guid? userId, CancellationToken ct = default)
    {
        await using var cmd = await CreateCommandAsync("auth.sp_DeletePermission", ct);

        cmd.Parameters.Add(new SqlParameter("@PermissionId", SqlDbType.UniqueIdentifier) { Value = permissionId });
        cmd.Parameters.Add(new SqlParameter("@UpdatedBy", SqlDbType.UniqueIdentifier) 
        { 
            Value = (object?)userId ?? DBNull.Value 
        });

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<int> GetAssignedRoleCountForPermissionAsync(Guid permissionId, CancellationToken ct = default)
    {
        await using var cmd = await CreateCommandAsync("auth.sp_GetAssignedRoleCountForPermission", ct);

        cmd.Parameters.Add(new SqlParameter("@PermissionId", SqlDbType.UniqueIdentifier) { Value = permissionId });

        var result = await cmd.ExecuteScalarAsync(ct);

        return result is not null && result != DBNull.Value
            ? Convert.ToInt32(result)
            : 0;
    }

    public async Task<(List<Permission> Permissions, int TotalCount)> SearchPermissionsAsync(PermissionSearchFilter filter, CancellationToken ct = default)
    {
        await using var cmd = await CreateCommandAsync("auth.sp_SearchPermissions", ct);

        cmd.Parameters.Add(new SqlParameter("@SearchTerm", SqlDbType.NVarChar, 100) 
        { 
            Value = string.IsNullOrWhiteSpace(filter.SearchTerm) ? DBNull.Value : filter.SearchTerm.Trim() 
        });
        cmd.Parameters.Add(new SqlParameter("@Module", SqlDbType.NVarChar, 50) 
        { 
            Value = string.IsNullOrWhiteSpace(filter.Module) ? DBNull.Value : filter.Module.Trim() 
        });
        cmd.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.Bit) 
        { 
            Value = (object?)filter.IsActive ?? DBNull.Value 
        });
        cmd.Parameters.Add(new SqlParameter("@PageNumber", SqlDbType.Int) { Value = filter.PageNumber < 1 ? 1 : filter.PageNumber });
        cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = filter.PageSize < 1 ? 10 : filter.PageSize });

        var permissions = new List<Permission>();
        int totalCount = 0;

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        if (await reader.ReadAsync(ct))
        {
            totalCount = reader.GetInt32(0);
        }
        
        if (await reader.NextResultAsync(ct))
        {
            var ordinals = new PermissionDataMapperExtensions.PermissionOrdinals(reader);

            while (await reader.ReadAsync(ct))
            {
                permissions.Add(reader.MapToPermission(ordinals));
            }
        }

        return (permissions, totalCount);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  ROLE - PERMISSIONS
    // ════════════════════════════════════════════════════════════════════════

    public async Task<IReadOnlyList<Permission>> GetPermissionsByRoleIdAsync(Guid roleId, CancellationToken ct = default)
    {
        await using var cmd = await CreateCommandAsync("auth.sp_GetPermissionsByRoleId", ct);
        cmd.Parameters.Add(new SqlParameter("@RoleId", SqlDbType.UniqueIdentifier) { Value = roleId });

        var permissions = new List<Permission>();

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (reader.HasRows)
        {
            var ordinals = new PermissionDataMapperExtensions.PermissionOrdinals(reader);

            while (await reader.ReadAsync(ct))
            {
                permissions.Add(reader.MapToPermission(ordinals));
            }
        }

        return permissions;
    }

    public async Task<IReadOnlyList<Permission>> GetPermissionsByIdsAsync(IEnumerable<Guid> permissionIds, CancellationToken ct = default)
    {
        var idsList = permissionIds.Distinct().ToList();
        if (!idsList.Any())
        {
            return Array.Empty<Permission>();
        }

        await using var cmd = await CreateCommandAsync("auth.sp_GetPermissionsByIds", ct);

        var tvpRecords = idsList.Select(id =>
        {
            var record = new Microsoft.Data.SqlClient.Server.SqlDataRecord(
                new Microsoft.Data.SqlClient.Server.SqlMetaData("Id", SqlDbType.UniqueIdentifier));
            record.SetGuid(0, id);
            return record;
        });

        var tvpParam = cmd.Parameters.Add(new SqlParameter("@PermissionIds", SqlDbType.Structured)
        {
            TypeName = "dbo.GuidListType",
            Value = tvpRecords
        });

        var permissions = new List<Permission>(idsList.Count);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (reader.HasRows)
        {
            var ordinals = new PermissionDataMapperExtensions.PermissionOrdinals(reader);

            while (await reader.ReadAsync(ct))
            {
                permissions.Add(reader.MapToPermission(ordinals));
            }
        }

        return permissions;
    }

    public async Task<bool> RoleHasPermissionAsync(Guid roleId, Guid permissionId, CancellationToken ct = default)
    {
        await using var cmd = await CreateCommandAsync("auth.sp_RoleHasPermission", ct);

        cmd.Parameters.Add(new SqlParameter("@RoleId", SqlDbType.UniqueIdentifier) { Value = roleId });
        cmd.Parameters.Add(new SqlParameter("@PermissionId", SqlDbType.UniqueIdentifier) { Value = permissionId });

        var result = await cmd.ExecuteScalarAsync(ct);

        return result != null && result != DBNull.Value && Convert.ToBoolean(result);
    }

    public async Task<bool> RoleHasPermissionByCodeAsync(Guid roleId, string permissionCode, CancellationToken ct = default)
    {
        await using var cmd = await CreateCommandAsync("auth.sp_RoleHasPermissionByCode", ct);

        cmd.Parameters.Add(new SqlParameter("@RoleId", SqlDbType.UniqueIdentifier) { Value = roleId });
        cmd.Parameters.Add(new SqlParameter("@PermissionCode", SqlDbType.NVarChar, 150) { Value = permissionCode });

        var result = await cmd.ExecuteScalarAsync(ct);

        return result != null && result != DBNull.Value && Convert.ToBoolean(result);
    }

    public async Task AddPermissionToRoleAsync(RolePermission rolePermission, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(rolePermission);

        await using var cmd = await CreateCommandAsync("auth.sp_AddPermissionToRole", ct);

        if (rolePermission.Id == Guid.Empty)
        {
            rolePermission.Id = Guid.NewGuid();
        }

        cmd.Parameters.Add(new SqlParameter("@RolePermissionId", SqlDbType.UniqueIdentifier) { Value = rolePermission.Id });
        cmd.Parameters.Add(new SqlParameter("@RoleId", SqlDbType.UniqueIdentifier) { Value = rolePermission.RoleId });
        cmd.Parameters.Add(new SqlParameter("@PermissionId", SqlDbType.UniqueIdentifier) { Value = rolePermission.PermissionId });
        cmd.Parameters.Add(new SqlParameter("@CreatedBy", SqlDbType.UniqueIdentifier) 
        { 
            Value = (object?)rolePermission.CreatedBy ?? DBNull.Value 
        });

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId, CancellationToken ct = default)
    {
        if (roleId == Guid.Empty)
            throw new ArgumentException("Role ID cannot be empty.", nameof(roleId));

        if (permissionId == Guid.Empty)
            throw new ArgumentException("Permission ID cannot be empty.", nameof(permissionId));

        await using var cmd = await CreateCommandAsync("auth.sp_RemovePermissionFromRole", ct);

        cmd.Parameters.Add(new SqlParameter("@RoleId", SqlDbType.UniqueIdentifier) { Value = roleId });
        cmd.Parameters.Add(new SqlParameter("@PermissionId", SqlDbType.UniqueIdentifier) { Value = permissionId });

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task RemoveAllPermissionsFromRoleAsync(Guid roleId, CancellationToken ct = default)
    {
        if (roleId == Guid.Empty)
            throw new ArgumentException("Role ID cannot be empty.", nameof(roleId));

        await using var cmd = await CreateCommandAsync("auth.sp_RemoveAllPermissionsFromRole", ct);

        cmd.Parameters.Add(new SqlParameter("@RoleId", SqlDbType.UniqueIdentifier) { Value = roleId });

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task AssignPermissionsToRoleAsync(Guid roleId, IEnumerable<RolePermission> rolePermissions, CancellationToken ct = default)
    {
        if (roleId == Guid.Empty)
            throw new ArgumentException("Role ID cannot be empty.", nameof(roleId));

        var permissionsList = rolePermissions?.ToList();
        if (permissionsList == null || !permissionsList.Any())
            return;

        var tvpRecords = permissionsList.Select(item =>
        {
            var record = new Microsoft.Data.SqlClient.Server.SqlDataRecord(
                new Microsoft.Data.SqlClient.Server.SqlMetaData("PermissionId", SqlDbType.UniqueIdentifier),
                new Microsoft.Data.SqlClient.Server.SqlMetaData("CreatedBy", SqlDbType.UniqueIdentifier)
            );

            record.SetGuid(0, item.PermissionId);

            if (item.CreatedBy.HasValue)
            {
                record.SetGuid(1, item.CreatedBy.Value);
            }
            else
            {
                record.SetDBNull(1);
            }

            return record;
        });

        await using var cmd = await CreateCommandAsync("auth.sp_AssignPermissionsToRole", ct);

        cmd.Parameters.Add(new SqlParameter("@RoleId", SqlDbType.UniqueIdentifier) { Value = roleId });

        var tvpParam = cmd.Parameters.Add(new SqlParameter("@Permissions", SqlDbType.Structured)
        {
            TypeName = "auth._RolePermissionsInput",
            Value = tvpRecords
        });

        await cmd.ExecuteNonQueryAsync(ct);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  USER - ROLES
    // ════════════════════════════════════════════════════════════════════════

    public async Task<IReadOnlyList<Role>> GetRolesByIdsAsync(IEnumerable<Guid> roleIds, CancellationToken ct = default)
    {
        if (roleIds is null)
            return Array.Empty<Role>();

        var distinctIds = roleIds.Where(id => id != Guid.Empty).Distinct().ToList();
        if (distinctIds.Count == 0)
            return Array.Empty<Role>();

        using var tvpTable = new DataTable();
        tvpTable.Columns.Add("Id", typeof(Guid));

        foreach (var id in distinctIds)
        {
            tvpTable.Rows.Add(id);
        }

        await using var cmd = await CreateCommandAsync("auth.sp_GetRolesByIds", ct);

        var param = cmd.Parameters.AddWithValue("@RoleIds", tvpTable);
        param.SqlDbType = SqlDbType.Structured;
        param.TypeName = "dbo.UDT_GuidList";

        var result = new List<Role>();

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (reader.HasRows)
        {
            var ordinals = new RoleDataMapperExtensions.RoleOrdinals(reader);
            while (await reader.ReadAsync(ct))
            {
                result.Add(RoleDataMapperExtensions.MapToRole(reader, ordinals));
            }
        }

        return result;
    }

    public async Task<Role?> GetRoleByNameAsync(string roleName, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            return null;

        using var cmd = await CreateCommandAsync("auth.sp_GetRoleByName", ct);
        cmd.Parameters.Add(new SqlParameter("@RoleName", SqlDbType.NVarChar, 100) { Value = roleName });

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
        {
            var ordinals = new RoleDataMapperExtensions.RoleOrdinals(reader);
            return RoleDataMapperExtensions.MapToRole(reader, ordinals);
        }

        return null;
    }

    public async Task<IReadOnlyList<UserRole>> GetRolesByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
            return Array.Empty<UserRole>();

        await using var cmd = await CreateCommandAsync("auth.sp_GetRolesByUserId", ct);
        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });

        var userRoles = new List<UserRole>();

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (reader.HasRows)
        {
            var ordinals = new UserRoleDataMapperExtensions.UserRoleOrdinals(reader);
            while (await reader.ReadAsync(ct))
            {
                userRoles.Add(reader.MapToUserRole(ordinals));
            }
        }

        return userRoles;
    }

    public async Task<bool> UserHasRoleAsync(Guid userId, Guid roleId, CancellationToken ct = default)
    {
        if (userId == Guid.Empty || roleId == Guid.Empty)
            return false;

        await using var cmd = await CreateCommandAsync("auth.sp_UserHasRole", ct);

        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });
        cmd.Parameters.Add(new SqlParameter("@RoleId", SqlDbType.UniqueIdentifier) { Value = roleId });

        var result = await cmd.ExecuteScalarAsync(ct);

        return result is bool hasRole && hasRole;
    }

    public async Task<bool> UserHasActiveRoleByCodeAsync(Guid userId, string roleCode, CancellationToken ct = default)
    {
        if (userId == Guid.Empty || string.IsNullOrWhiteSpace(roleCode))
            return false;

        await using var cmd = await CreateCommandAsync("auth.sp_UserHasActiveRoleByCode", ct);

        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });
        cmd.Parameters.Add(new SqlParameter("@RoleCode", SqlDbType.NVarChar, 100) { Value = roleCode.Trim() });

        var result = await cmd.ExecuteScalarAsync(ct);

        return result is bool hasRole && hasRole;
    }

    public async Task AddUserRoleAsync(UserRole userRole, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(userRole);

        if (userRole.UserId == Guid.Empty || userRole.RoleId == Guid.Empty)
            throw new ArgumentException("UserId and RoleId must not be empty Guid.");

        await using var cmd = await CreateCommandAsync("auth.sp_AddUserRole", ct);

        cmd.Parameters.Add(new SqlParameter("@UserRoleId", SqlDbType.UniqueIdentifier) 
        { 
            Value = userRole.Id == Guid.Empty ? Guid.NewGuid() : userRole.Id 
        });
        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userRole.UserId });
        cmd.Parameters.Add(new SqlParameter("@RoleId", SqlDbType.UniqueIdentifier) { Value = userRole.RoleId });
        cmd.Parameters.Add(new SqlParameter("@ValidFrom", SqlDbType.DateTimeOffset) { Value = userRole.ValidFrom });
        cmd.Parameters.Add(new SqlParameter("@ValidTo", SqlDbType.DateTimeOffset) 
        { 
            Value = (object?)userRole.ValidTo ?? DBNull.Value 
        });
        cmd.Parameters.Add(new SqlParameter("@AssignedBy", SqlDbType.UniqueIdentifier) 
        { 
            Value = (object?)userRole.AssignedBy ?? DBNull.Value 
        });
        cmd.Parameters.Add(new SqlParameter("@CreatedAt", SqlDbType.DateTimeOffset) 
        { 
            Value = userRole.CreatedAt == default ? DateTimeOffset.UtcNow : userRole.CreatedAt 
        });

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task RemoveUserRoleAsync(Guid userId, Guid roleId, CancellationToken ct = default)
    {
        if (userId == Guid.Empty || roleId == Guid.Empty)
            return;

        await using var cmd = await CreateCommandAsync("auth.sp_RemoveUserRole", ct);

        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });
        cmd.Parameters.Add(new SqlParameter("@RoleId", SqlDbType.UniqueIdentifier) { Value = roleId });

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task RemoveAllRolesFromUserAsync(Guid userId, CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
            return;

        await using var cmd = await CreateCommandAsync("auth.sp_RemoveAllRolesFromUser", ct);

        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task AssignRolesToUserAsync(Guid userId, IEnumerable<UserRole> userRoles, CancellationToken ct = default)
    {
        if (userId == Guid.Empty || userRoles == null)
            return;

        var rolesList = userRoles.ToList();
        if (rolesList.Count == 0)
            return;

        using var table = new DataTable();
        table.Columns.Add("RoleId", typeof(Guid));
        table.Columns.Add("ValidFrom", typeof(DateTimeOffset));
        table.Columns.Add("ValidTo", typeof(DateTimeOffset));
        table.Columns.Add("AssignedBy", typeof(Guid));

        foreach (var role in rolesList)
        {
            table.Rows.Add(
                role.RoleId,
                role.ValidFrom,
                role.ValidTo.HasValue ? role.ValidTo.Value : DBNull.Value,
                role.AssignedBy.HasValue ? role.AssignedBy.Value : DBNull.Value
            );
        }

        await using var cmd = await CreateCommandAsync("auth.sp_AssignRolesToUser", ct);

        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });

        var tvpParam = cmd.Parameters.AddWithValue("@UserRoles", table);
        tvpParam.SqlDbType = SqlDbType.Structured;
        tvpParam.TypeName = "auth.UserRoleTableType";

        await cmd.ExecuteNonQueryAsync(ct);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  USER - PERMISSION OVERRIDES
    // ════════════════════════════════════════════════════════════════════════

    public async Task<UserPermissionResponseDto?> GetUserPermissionOverrideByIdAsync(Guid userPermissionId, CancellationToken ct = default)
    {
        if (userPermissionId == Guid.Empty)
            return null;

        await using var cmd = await CreateCommandAsync("auth.sp_GetUserPermissionOverrideById", ct);

        cmd.Parameters.Add(new SqlParameter("@UserPermissionId", SqlDbType.UniqueIdentifier) { Value = userPermissionId });

        await using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow, ct);

        if (await reader.ReadAsync(ct))
        {
            var ordinals = new UserPermissionDataMapperExtensions.UserPermissionResponseOrdinals(reader);
            return reader.MapToUserPermissionResponseDto(ordinals);
        }

        return null;
    }

    public async Task<IReadOnlyList<UserPermissionResponseDto>> GetUserPermissionOverridesAsync(Guid userId, CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
            return Array.Empty<UserPermissionResponseDto>();

        await using var cmd = await CreateCommandAsync("auth.sp_GetUserPermissionOverridesForManagement", ct);

        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        if (!reader.HasRows)
            return Array.Empty<UserPermissionResponseDto>();

        var list = new List<UserPermissionResponseDto>();
        var ordinals = new UserPermissionDataMapperExtensions.UserPermissionResponseOrdinals(reader);

        while (await reader.ReadAsync(ct))
        {
            list.Add(reader.MapToUserPermissionResponseDto(ordinals));
        }

        return list;
    }

    public async Task<bool> UserPermissionOverrideExistsAsync(Guid userId, Guid permissionId, string grantType, CancellationToken ct = default)
    {
        if (userId == Guid.Empty || permissionId == Guid.Empty || string.IsNullOrWhiteSpace(grantType))
            return false;

        await using var cmd = await CreateCommandAsync("auth.sp_CheckUserPermissionOverrideExists", ct);

        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });
        cmd.Parameters.Add(new SqlParameter("@PermissionId", SqlDbType.UniqueIdentifier) { Value = permissionId });
        cmd.Parameters.Add(new SqlParameter("@GrantType", SqlDbType.VarChar, 5) { Value = grantType.Trim().ToUpperInvariant() });

        var result = await cmd.ExecuteScalarAsync(ct);

        return result is not null && result != DBNull.Value && Convert.ToBoolean(result);
    }

    public async Task CreateUserPermissionOverrideAsync(UserPermission userPermission, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(userPermission);

        await using var cmd = await CreateCommandAsync("auth.sp_CreateUserPermissionOverride", ct);

        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userPermission.UserId });
        cmd.Parameters.Add(new SqlParameter("@PermissionId", SqlDbType.UniqueIdentifier) { Value = userPermission.PermissionId });
        cmd.Parameters.Add(new SqlParameter("@GrantType", SqlDbType.VarChar, 5) { Value = userPermission.GrantType.ToString().ToUpperInvariant() });
        cmd.Parameters.Add(new SqlParameter("@Reason", SqlDbType.NVarChar, 500) { Value = (object?)userPermission.Reason ?? DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@ValidFrom", SqlDbType.DateTimeOffset) { Value = userPermission.ValidFrom });
        cmd.Parameters.Add(new SqlParameter("@ValidTo", SqlDbType.DateTimeOffset) { Value = (object?)userPermission.ValidTo ?? DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@GrantedBy", SqlDbType.UniqueIdentifier) { Value = (object?)userPermission.GrantedBy ?? DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.Bit) { Value = userPermission.IsActive });
        cmd.Parameters.Add(new SqlParameter("@CreatedBy", SqlDbType.UniqueIdentifier) { Value = (object?)userPermission.CreatedBy ?? DBNull.Value });

        var generatedId = await cmd.ExecuteScalarAsync(ct);

        if (generatedId is not null && generatedId != DBNull.Value)
        {
            userPermission.Id = (Guid)generatedId;
        }
    }

    public async Task UpdateUserPermissionOverrideAsync(UserPermission userPermission, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(userPermission);

        await using var cmd = await CreateCommandAsync("auth.sp_UpdateUserPermissionOverride", ct);

        cmd.Parameters.Add(new SqlParameter("@UserPermissionId", SqlDbType.UniqueIdentifier) { Value = userPermission.Id });
        cmd.Parameters.Add(new SqlParameter("@GrantType", SqlDbType.VarChar, 5) { Value = userPermission.GrantType.ToString().ToUpperInvariant() });
        cmd.Parameters.Add(new SqlParameter("@Reason", SqlDbType.NVarChar, 500) { Value = (object?)userPermission.Reason ?? DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@ValidFrom", SqlDbType.DateTimeOffset) { Value = userPermission.ValidFrom });
        cmd.Parameters.Add(new SqlParameter("@ValidTo", SqlDbType.DateTimeOffset) { Value = (object?)userPermission.ValidTo ?? DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.Bit) { Value = userPermission.IsActive });
        cmd.Parameters.Add(new SqlParameter("@UpdatedBy", SqlDbType.UniqueIdentifier) { Value = (object?)userPermission.UpdatedBy ?? DBNull.Value });

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task DeleteUserPermissionOverrideAsync(Guid userPermissionId, CancellationToken ct = default)
    {
        await using var cmd = await CreateCommandAsync("auth.sp_DeleteUserPermissionOverride", ct);

        cmd.Parameters.Add(new SqlParameter("@UserPermissionId", SqlDbType.UniqueIdentifier) { Value = userPermissionId });

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task RemoveAllPermissionOverridesFromUserAsync(Guid userId, CancellationToken ct = default)
    {
        await using var cmd = await CreateCommandAsync("auth.sp_RemoveAllPermissionOverridesFromUser", ct);
    
        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });
    
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task AddUserPermissionOverridesBulkAsync(Guid userId, IEnumerable<UserPermission> overrides, CancellationToken ct = default)
    {
        var overrideList = overrides as IReadOnlyCollection<UserPermission> ?? overrides.ToList();
        if (overrideList.Count == 0)
            return;

        using var table = new DataTable();
        table.Columns.Add("PermissionId", typeof(Guid));
        table.Columns.Add("GrantType", typeof(string));
        table.Columns.Add("Reason", typeof(string));
        table.Columns.Add("ValidFrom", typeof(DateTimeOffset));
        table.Columns.Add("ValidTo", typeof(DateTimeOffset));
        table.Columns.Add("GrantedBy", typeof(Guid));
        table.Columns.Add("IsActive", typeof(bool));
        table.Columns.Add("CreatedBy", typeof(Guid));

        foreach (var item in overrideList)
        {
            table.Rows.Add(
                item.PermissionId,
                item.GrantType.ToString().ToUpperInvariant(),
                (object?)item.Reason ?? DBNull.Value,
                item.ValidFrom,
                (object?)item.ValidTo ?? DBNull.Value,
                (object?)item.GrantedBy ?? DBNull.Value,
                item.IsActive,
                (object?)item.CreatedBy ?? DBNull.Value
            );
        }

        await using var cmd = await CreateCommandAsync("auth.sp_AddUserPermissionOverridesBulk", ct);

        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });

        var tvpParam = cmd.Parameters.AddWithValue("@UserPermissions", table);
        tvpParam.SqlDbType = SqlDbType.Structured;
        tvpParam.TypeName = "auth.UserPermissionItemType";

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<bool> AnyRoleHasPermissionByCodeAsync(IEnumerable<Guid> roleIds, string permissionCode, CancellationToken ct = default)
    {
        var roleIdList = roleIds as IReadOnlyCollection<Guid> ?? roleIds.ToList();
        if (roleIdList.Count == 0 || string.IsNullOrWhiteSpace(permissionCode))
            return false;

        using var table = new DataTable();
        table.Columns.Add("Id", typeof(Guid));

        foreach (var roleId in roleIdList.Distinct())
        {
            table.Rows.Add(roleId);
        }

        await using var cmd = await CreateCommandAsync("auth.sp_AnyRoleHasPermissionByCode", ct);

        var tvpParam = cmd.Parameters.AddWithValue("@RoleIds", table);
        tvpParam.SqlDbType = SqlDbType.Structured;
        tvpParam.TypeName = "auth.GuidListType";

        cmd.Parameters.Add(new SqlParameter("@PermissionCode", SqlDbType.NVarChar, 150) { Value = permissionCode });

        var result = await cmd.ExecuteScalarAsync(ct);
        return result is not null && Convert.ToBoolean(result);
    }

    public async Task<bool> HasActiveDenyOverrideAsync(Guid userId, string permissionCode, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(permissionCode))
            return false;

        await using var cmd = await CreateCommandAsync("auth.sp_HasActiveDenyOverride", ct);

        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });
        cmd.Parameters.Add(new SqlParameter("@PermissionCode", SqlDbType.NVarChar, 150) { Value = permissionCode });

        var result = await cmd.ExecuteScalarAsync(ct);
        return result is not null && Convert.ToBoolean(result);
    }

    public async Task<List<string>> GetEffectivePermissionsByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
            return new List<string>();
    
        await using var cmd = await CreateCommandAsync("auth.sp_GetEffectivePermissionsForUser", ct, CommandType.StoredProcedure);
    
        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });
    
        var effectivePermissions = new List<string>();
    
        await using var reader = await cmd.ExecuteReaderAsync(ct);
    
        var codeOrdinal = reader.GetOrdinal("PermissionCode");
    
        while (await reader.ReadAsync(ct))
        {
            effectivePermissions.Add(reader.GetString(codeOrdinal));
        }
    
        return effectivePermissions;
    }

    public async Task<bool> HasActiveGrantOverrideAsync(Guid userId, string permissionCode, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(permissionCode))
            return false;

        await using var cmd = await CreateCommandAsync("auth.sp_HasActiveGrantOverride", ct);

        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });
        cmd.Parameters.Add(new SqlParameter("@PermissionCode", SqlDbType.NVarChar, 150) { Value = permissionCode });

        var result = await cmd.ExecuteScalarAsync(ct);
        return result is not null && Convert.ToBoolean(result);
    }

    public async Task<List<string>> GetUserRolesAsync(Guid userId, CancellationToken ct = default)
    {
        await using var cmd = await CreateCommandAsync("auth.sp_GetUserRoles", ct);

        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });

        var roles = new List<string>();

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        if (reader.HasRows)
        {
            var roleNameOrdinal = reader.GetOrdinal("RoleName");

            while (await reader.ReadAsync(ct))
            {
                roles.Add(reader.GetString(roleNameOrdinal));
            }
        }

        return roles;
    }
    
    public async Task<List<string>> GetUserPermissionsAsync(Guid userId, CancellationToken ct = default)
    {
        await using var cmd = await CreateCommandAsync("auth.sp_GetActiveUserPermissionOverrides", ct);

        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });

        var overridePermissions = new List<string>();

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        if (reader.HasRows)
        {
            var permissionCodeOrdinal = reader.GetOrdinal("PermissionCode");

            while (await reader.ReadAsync(ct))
            {
                overridePermissions.Add(reader.GetString(permissionCodeOrdinal));
            }
        }

        return overridePermissions;
    }
    
    // ════════════════════════════════════════════════════════════════════════
    //  SESSIONS MANAGEMENT
    // ════════════════════════════════════════════════════════════════════════

    public async Task CreateSessionAsync(UserSession session, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(session);

        if (session.Id == Guid.Empty)
        {
            session.Id = Guid.NewGuid();
        }

        await using var cmd = await CreateCommandAsync("auth.sp_CreateUserSession", ct);

        cmd.Parameters.Add(new SqlParameter("@SessionId", SqlDbType.UniqueIdentifier) { Value = session.Id });
        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = session.UserId });
        cmd.Parameters.Add(new SqlParameter("@RefreshToken", SqlDbType.NVarChar, 512) { Value = session.RefreshToken });
        cmd.Parameters.Add(new SqlParameter("@DeviceInfo", SqlDbType.NVarChar, 500) { Value = (object?)session.DeviceInfo ?? DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@IpAddress", SqlDbType.VarChar, 45) { Value = (object?)session.IpAddress ?? DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@UserAgent", SqlDbType.NVarChar, 500) { Value = (object?)session.UserAgent ?? DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@IssuedAtUtc", SqlDbType.DateTimeOffset) { Value = session.IssuedAtUtc });
        cmd.Parameters.Add(new SqlParameter("@ExpiresAtUtc", SqlDbType.DateTimeOffset) { Value = session.ExpiresAtUtc });
        cmd.Parameters.Add(new SqlParameter("@RevokedAtUtc", SqlDbType.DateTimeOffset) { Value = (object?)session.RevokedAtUtc ?? DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@IsRevoked", SqlDbType.Bit) { Value = session.IsRevoked });
        cmd.Parameters.Add(new SqlParameter("@ReplacedByToken", SqlDbType.NVarChar, 512) { Value = (object?)session.ReplacedByToken ?? DBNull.Value });

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<UserSession?> GetSessionByRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return null;

        await using var cmd = await CreateCommandAsync("auth.sp_GetSessionByRefreshToken", ct);

        cmd.Parameters.Add(new SqlParameter("@RefreshToken", SqlDbType.NVarChar, 512) { Value = refreshToken });

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        if (await reader.ReadAsync(ct))
        {
            var ordinals = new UserSessionDataMapperExtensions.UserSessionOrdinals(reader);
            return reader.MapToUserSession(ordinals);
        }

        return null;
    }

    public async Task RevokeSessionAsync(Guid sessionId, string? replacedByToken, DateTimeOffset revokedAt, CancellationToken ct = default)
    {
        await using var cmd = await CreateCommandAsync("auth.sp_RevokeSession", ct);

        cmd.Parameters.Add(new SqlParameter("@SessionId", SqlDbType.UniqueIdentifier) { Value = sessionId });
        cmd.Parameters.Add(new SqlParameter("@ReplacedByToken", SqlDbType.NVarChar, 512) { Value = (object?)replacedByToken ?? DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@RevokedAtUtc", SqlDbType.DateTimeOffset) { Value = revokedAt });

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task RevokeAllUserSessionsAsync(Guid userId, DateTimeOffset revokedAt, CancellationToken ct = default)
    {
        await using var cmd = await CreateCommandAsync("auth.sp_RevokeAllUserSessions", ct);

        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });
        cmd.Parameters.Add(new SqlParameter("@RevokedAtUtc", SqlDbType.DateTimeOffset) { Value = revokedAt });

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<IReadOnlyList<UserSessionResponseDto>> GetActiveSessionsByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        await using var cmd = await CreateCommandAsync("auth.sp_GetActiveSessionsByUserId", ct);

        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        if (!reader.HasRows)
            return Array.Empty<UserSessionResponseDto>();

        int sessionIdOrd  = reader.GetOrdinal("SessionId");
        int deviceInfoOrd = reader.GetOrdinal("DeviceInfo");
        int ipAddressOrd  = reader.GetOrdinal("IpAddress");
        int userAgentOrd  = reader.GetOrdinal("UserAgent");
        int issuedAtOrd   = reader.GetOrdinal("IssuedAtUtc");
        int expiresAtOrd  = reader.GetOrdinal("ExpiresAtUtc");
        int isActiveOrd   = reader.GetOrdinal("IsActive");

        var sessions = new List<UserSessionResponseDto>();

        while (await reader.ReadAsync(ct))
        {
            sessions.Add(new UserSessionResponseDto
            {
                SessionId   = reader.GetGuid(sessionIdOrd),
                DeviceInfo  = reader.IsDBNull(deviceInfoOrd) ? null : reader.GetString(deviceInfoOrd),
                IpAddress   = reader.IsDBNull(ipAddressOrd) ? null : reader.GetString(ipAddressOrd),
                UserAgent   = reader.IsDBNull(userAgentOrd) ? null : reader.GetString(userAgentOrd),
                IssuedAtUtc = reader.GetDateTimeOffset(issuedAtOrd),
                ExpiresAtUtc = reader.GetDateTimeOffset(expiresAtOrd),
                IsActive    = reader.GetBoolean(isActiveOrd)
            });
        }

        return sessions;
    }

    // ════════════════════════════════════════════════════════════════════════
    //  TOKENS & OTP MANAGEMENT
    // ════════════════════════════════════════════════════════════════════════

    public async Task SaveEmailOtpAsync(Guid userId, string otpCode, DateTimeOffset expiresAt, CancellationToken ct = default)
    {
        await using var cmd = await CreateCommandAsync("auth.sp_SaveEmailOtp", ct);

        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });
        cmd.Parameters.Add(new SqlParameter("@TokenHash", SqlDbType.NVarChar, 256) { Value = otpCode });
        cmd.Parameters.Add(new SqlParameter("@ExpiresAt", SqlDbType.DateTimeOffset) { Value = expiresAt });

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<bool> ValidateAndConsumeEmailOtpAsync(Guid userId, string otpCode, DateTimeOffset currentTime, CancellationToken ct = default)
    {
        await using var cmd = await CreateCommandAsync("auth.sp_ValidateAndConsumeEmailOtp", ct);

        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });
        cmd.Parameters.Add(new SqlParameter("@TokenHash", SqlDbType.NVarChar, 256) { Value = otpCode });
        cmd.Parameters.Add(new SqlParameter("@CurrentTime", SqlDbType.DateTimeOffset) { Value = currentTime });

        var result = await cmd.ExecuteScalarAsync(ct);

        return result is not null && Convert.ToBoolean(result);
    }

    public async Task SetEmailVerificationTokenAsync(Guid userId, string token, DateTimeOffset expiresAt, CancellationToken ct = default)
    {
        await using var cmd = await CreateCommandAsync("auth.sp_SetEmailVerificationToken", ct);

        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });
        cmd.Parameters.Add(new SqlParameter("@TokenHash", SqlDbType.NVarChar, 256) { Value = token });
        cmd.Parameters.Add(new SqlParameter("@ExpiresAt", SqlDbType.DateTimeOffset) { Value = expiresAt });

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task SetPasswordResetTokenAsync(Guid userId, string token, DateTimeOffset expiresAt, CancellationToken ct = default)
    {
        await using var cmd = await CreateCommandAsync("auth.sp_SetPasswordResetToken", ct);

        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });
        cmd.Parameters.Add(new SqlParameter("@TokenHash", SqlDbType.NVarChar, 256) { Value = token });
        cmd.Parameters.Add(new SqlParameter("@ExpiresAt", SqlDbType.DateTimeOffset) { Value = expiresAt });

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<ApplicationUser?> GetUserByPasswordResetTokenAsync(string token, DateTimeOffset now, CancellationToken ct = default)
    {
        await using var cmd = await CreateCommandAsync("auth.sp_GetUserByPasswordResetToken", ct);

        cmd.Parameters.Add(new SqlParameter("@TokenHash", SqlDbType.NVarChar, 256) { Value = token });
        cmd.Parameters.Add(new SqlParameter("@CurrentTime", SqlDbType.DateTimeOffset) { Value = now });

        await using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow, ct);

        if (await reader.ReadAsync(ct))
        {
            var ordinals = new UserDataMapperExtensions.UserOrdinals(reader);
            return reader.MapToApplicationUser(ordinals);
        }

        return null;
    }

    public async Task SaveUserTokenAsync(UserToken userToken, CancellationToken ct = default)
    {
        await using var cmd = await CreateCommandAsync("auth.sp_SaveUserToken", ct);

        cmd.Parameters.Add(new SqlParameter("@TokenId", SqlDbType.UniqueIdentifier) 
        { 
            Value = userToken.Id == Guid.Empty ? DBNull.Value : userToken.Id 
        });
        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userToken.UserId });
        cmd.Parameters.Add(new SqlParameter("@TokenTypeId", SqlDbType.TinyInt) { Value = (byte)userToken.TokenType });
        cmd.Parameters.Add(new SqlParameter("@TokenHash", SqlDbType.NVarChar, 256) { Value = userToken.TokenHash });
        cmd.Parameters.Add(new SqlParameter("@ExpiresAt", SqlDbType.DateTimeOffset) { Value = userToken.ExpiresAt });
        cmd.Parameters.Add(new SqlParameter("@IsUsed", SqlDbType.Bit) { Value = userToken.IsUsed });
        cmd.Parameters.Add(new SqlParameter("@UsedAt", SqlDbType.DateTimeOffset) 
        { 
            Value = userToken.UsedAt.HasValue ? userToken.UsedAt.Value : DBNull.Value 
        });
        cmd.Parameters.Add(new SqlParameter("@CreatedByIp", SqlDbType.VarChar, 45) 
        { 
            Value = (object?)userToken.CreatedByIp ?? DBNull.Value 
        });

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<Guid?> GetUserIdByValidTokenAsync(string token, TokenType tokenType, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        await using var cmd = await CreateCommandAsync("auth.sp_GetUserIdByValidToken", ct);

        cmd.Parameters.Add(new SqlParameter("@TokenHash", SqlDbType.NVarChar, 256) { Value = token });
        cmd.Parameters.Add(new SqlParameter("@TokenTypeId", SqlDbType.TinyInt) { Value = (byte)tokenType });

        var result = await cmd.ExecuteScalarAsync(ct);

        if (result is null or DBNull)
            return null;

        return (Guid)result;
    }

    public async Task MarkTokenAsUsedAsync(string token, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return;

        await using var cmd = await CreateCommandAsync("auth.sp_MarkTokenAsUsed", ct);

        cmd.Parameters.Add(new SqlParameter("@TokenHash", SqlDbType.NVarChar, 256) { Value = token });

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<UserToken?> GetActiveTokenByHashAsync(Guid userId, string tokenHash, byte tokenTypeId, DateTimeOffset now, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tokenHash))
            return null;

        await using var cmd = await CreateCommandAsync("auth.sp_GetActiveTokenByHash", ct);

        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });
        cmd.Parameters.Add(new SqlParameter("@TokenHash", SqlDbType.NVarChar, 256) { Value = tokenHash });
        cmd.Parameters.Add(new SqlParameter("@TokenTypeId", SqlDbType.TinyInt) { Value = tokenTypeId });
        cmd.Parameters.Add(new SqlParameter("@Now", SqlDbType.DateTimeOffset) { Value = now });

        await using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow, ct);

        if (await reader.ReadAsync(ct))
        {
            var ordinals = new UserTokenDataMapperExtensions.UserTokenOrdinals(reader);
            return reader.MapToUserToken(ordinals);
        }

        return null;
    }

    public async Task UpdateUserTokenAsync(UserToken userToken, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(userToken);

        await using var cmd = await CreateCommandAsync("auth.sp_UpdateUserToken", ct);

        cmd.Parameters.Add(new SqlParameter("@TokenId", SqlDbType.UniqueIdentifier) { Value = userToken.Id });
        cmd.Parameters.Add(new SqlParameter("@TokenTypeId", SqlDbType.TinyInt) { Value = (byte)userToken.TokenType });
        cmd.Parameters.Add(new SqlParameter("@ExpiresAt", SqlDbType.DateTimeOffset) { Value = userToken.ExpiresAt });
        cmd.Parameters.Add(new SqlParameter("@IsUsed", SqlDbType.Bit) { Value = userToken.IsUsed });

        cmd.Parameters.Add(new SqlParameter("@UsedAt", SqlDbType.DateTimeOffset) 
        { 
            Value = userToken.UsedAt.HasValue ? userToken.UsedAt.Value : DBNull.Value 
        });

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task InvalidateAllUser2FaTokensAsync(Guid userId, DateTimeOffset now, CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
            return;
    
        await using var cmd = await CreateCommandAsync("auth.sp_InvalidateAllUser2FaTokens", ct);
    
        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });
        cmd.Parameters.Add(new SqlParameter("@Now", SqlDbType.DateTimeOffset) { Value = now });
    
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task InvalidateAllUserTokensByTypeAsync(Guid userId, byte tokenTypeId, DateTimeOffset now, CancellationToken ct = default)
    {
        if (userId == Guid.Empty || tokenTypeId <= 0)
            return;

        await using var cmd = await CreateCommandAsync("auth.sp_InvalidateAllUserTokensByType", ct);

        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });
        cmd.Parameters.Add(new SqlParameter("@TokenTypeId", SqlDbType.Int) { Value = tokenTypeId });
        cmd.Parameters.Add(new SqlParameter("@Now", SqlDbType.DateTimeOffset) { Value = now });

        await cmd.ExecuteNonQueryAsync(ct);
    }
}