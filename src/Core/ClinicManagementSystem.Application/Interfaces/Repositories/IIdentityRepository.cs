using ClinicManagementSystem.Application.DTOs.Auth.UserAssignments;
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
    Task AssignRolesToUserAsync(Guid userId, IEnumerable<RoleAssignmentItemDto> roles, Guid assignedBy, CancellationToken ct = default);
    Task AssignPermissionsToRoleAsync(Guid roleId, IEnumerable<Guid> permissionIds, CancellationToken ct = default);
    Task SetUserPermissionOverridesAsync(Guid userId, IEnumerable<UserPermissionOverrideItemDto> overrides, Guid grantedBy, CancellationToken ct = default);
    Task CreateSessionAsync(UserSession session, CancellationToken ct = default);
    Task<UserSession?> GetSessionByRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    Task RevokeSessionAsync(Guid sessionId, string? replacedByToken, DateTimeOffset revokedAt, CancellationToken ct = default);
    Task RevokeAllUserSessionsAsync(Guid userId, DateTimeOffset revokedAt, CancellationToken ct = default);
    Task SaveEmailOtpAsync(Guid userId, string otpCode, DateTimeOffset expiresAt, CancellationToken ct = default);
    Task<bool> ValidateAndConsumeEmailOtpAsync(Guid userId, string otpCode, DateTimeOffset currentTime, CancellationToken ct = default);
}
