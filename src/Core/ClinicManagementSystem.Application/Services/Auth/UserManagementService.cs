using ClinicManagementSystem.Application.Common.Errors;
using ClinicManagementSystem.Application.Common.Interfaces;
using ClinicManagementSystem.Application.Common.Models;
using ClinicManagementSystem.Application.DTOs.Auth.Users;
using ClinicManagementSystem.Application.Interfaces.Repositories;
using ClinicManagementSystem.Application.Interfaces.Services.Auth;
using ClinicManagementSystem.Domain.Entities.Auth;
using Microsoft.Extensions.Logging;

namespace ClinicManagementSystem.Application.Services.Auth;

public sealed class UserManagementService : IUserManagementService
{
    private readonly IIdentityRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly IFileStorageService _fileStorage;
    private readonly IDateTimeProvider _date;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<UserManagementService> _logger;

    public UserManagementService(
        IIdentityRepository repo,
        IUnitOfWork uow,
        IFileStorageService fileStorage,
        IDateTimeProvider date,
        ICurrentUserService currentUser,
        ILogger<UserManagementService> logger)
    {
        _repo = repo;
        _uow = uow;
        _fileStorage = fileStorage;
        _date = date;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<ApiResponse<UserResponseDto>> GetUserByIdAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _repo.GetUserByIdAsync(userId, ct);
        if (user is null)
        {
            return ApiResponse<UserResponseDto>.Failure("User not found.", AuthErrors.UserNotFound);
        }

        var roles = await _repo.GetUserRolesAsync(user.Id, ct);
        var userProfile = MapToUserResponseDto(user, roles);

        return ApiResponse<UserResponseDto>.Success(userProfile, "User details retrieved successfully.");
    }

    public async Task<ApiResponse<PaginatedList<UserResponseDto>>> GetAllUsersAsync(UserQueryParams queryParams, CancellationToken ct = default)
    {
        var (users, userRolesMap, totalCount) = await _repo.GetPagedUsersAsync(queryParams, ct);

        if (users is null || !users.Any())
        {
            var emptyList = new PaginatedList<UserResponseDto>(
                Array.Empty<UserResponseDto>(), 
                0, 
                queryParams.PageNumber, 
                queryParams.PageSize);

            return ApiResponse<PaginatedList<UserResponseDto>>.Success(
                emptyList, 
                "No users found.");
        }

        var userDtos = users.Select(user => 
        {
            var roles = userRolesMap.TryGetValue(user.Id, out var rList) ? rList : new List<string>();
            return MapToUserResponseDto(user, roles);
        }).ToList();

        var paginatedResult = new PaginatedList<UserResponseDto>(
            userDtos, 
            totalCount, 
            queryParams.PageNumber, 
            queryParams.PageSize);

        return ApiResponse<PaginatedList<UserResponseDto>>.Success(
            paginatedResult, 
            "Users list retrieved successfully.");
    }

    public async Task<ApiResponse<IEnumerable<UserResponseDto>>> SearchUsersAsync(string searchTerm, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return ApiResponse<IEnumerable<UserResponseDto>>.Failure("Search term cannot be empty.", AuthErrors.RequiredFieldMissing);
        }

        var queryParams = new UserQueryParams { SearchTerm = searchTerm, PageSize = 30 };
        var result = await GetAllUsersAsync(queryParams, ct);
        
        return ApiResponse<IEnumerable<UserResponseDto>>.Success(result.Data!.Items, "Search results completed.");
    }

    public async Task<ApiResponse<bool>> UpdateProfileAsync(
        Guid userId, 
        UpdateUserProfileRequestDto requestDto, 
        Stream? avatarStream = null, 
        string? avatarFileName = null, 
        CancellationToken ct = default)
    {
        var user = await _repo.GetUserByIdAsync(userId, ct);
        if (user is null)
        {
            return ApiResponse<bool>.Failure("User not found.", AuthErrors.UserNotFound);
        }

        if (!user.IsActive)
        {
            return ApiResponse<bool>.Failure("User account is inactive.", AuthErrors.AccountInactive);
        }

        string? newAvatarUrl = null;

        if (avatarStream is not null && !string.IsNullOrWhiteSpace(avatarFileName))
        {
            if (!string.IsNullOrWhiteSpace(user.AvatarUrl))
            {
                try
                {
                    await _fileStorage.DeleteFileAsync(user.AvatarUrl, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete old avatar for user {UserId} from storage.", userId);
                }
            }

            newAvatarUrl = await _fileStorage.UploadFileAsync(avatarStream, avatarFileName, "avatars", ct);
        }

        if (!string.IsNullOrWhiteSpace(requestDto.FirstName)) user.FirstName = requestDto.FirstName.Trim();
        if (!string.IsNullOrWhiteSpace(requestDto.LastName)) user.LastName = requestDto.LastName.Trim();
        if (!string.IsNullOrWhiteSpace(requestDto.PhoneNumber))
        {
            if(! await _repo.IsPhoneNumberTakenAsync(requestDto.PhoneNumber, ct))
            {
                user.PhoneNumber = requestDto.PhoneNumber.Trim();
                user.PhoneVerified = false;
            }
        } 
        if (!string.IsNullOrWhiteSpace(requestDto.Email))
        {
            if(! await _repo.IsEmailTakenAsync(requestDto.Email, ct))
            {
                user.Email = requestDto.Email.Trim();
                user.EmailVerified = false;
            }
        } 
        if (newAvatarUrl is not null) user.AvatarUrl = newAvatarUrl;

        user.UpdatedAt = _date.UtcNow;
        user.CreatedBy = _currentUser.UserId;

        await _repo.UpdateUserAsync(user, ct);
        _logger.LogInformation("Profile updated successfully for user {UserId}.", userId);

        return ApiResponse<bool>.Success(true, "Your profile has been updated successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteAvatarAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _repo.GetUserByIdAsync(userId, ct);
        if (user is null)
        {
            return ApiResponse<bool>.Failure("User not found.", AuthErrors.UserNotFound);
        }

        if (string.IsNullOrWhiteSpace(user.AvatarUrl))
        {
            return ApiResponse<bool>.Success(true, "User does not have a custom avatar assigned.");
        }

        await _fileStorage.DeleteFileAsync(user.AvatarUrl, ct);

        user.AvatarUrl = null; 
        user.UpdatedAt = _date.UtcNow;

        await _repo.UpdateUserAsync(user, ct);
        _logger.LogInformation("Avatar deleted successfully for user {UserId}.", userId);

        return ApiResponse<bool>.Success(true, "Avatar removed successfully.");
    }

    public async Task<ApiResponse<bool>> AdminUpdateUserAsync(Guid userId, AdminUpdateUserRequestDto requestDto, CancellationToken ct = default)
    {
        var user = await _repo.GetUserByIdAsync(userId, ct);
        if (user is null)
        {
            return ApiResponse<bool>.Failure("User not found.", AuthErrors.UserNotFound);
        }

        var now = _date.UtcNow;

        await _uow.ExecuteInTransactionAsync(async () =>
        {
            if (!string.IsNullOrWhiteSpace(requestDto.FirstName)) user.FirstName = requestDto.FirstName.Trim();
            if (!string.IsNullOrWhiteSpace(requestDto.LastName)) user.LastName = requestDto.LastName.Trim();
            
            user.IsActive = requestDto.IsActive;
            user.LockoutEnabled = requestDto.LockoutEnabled;
            user.UpdatedAt = now;

            await _repo.UpdateUserAsync(user, ct);

            if (requestDto.Roles is not null && requestDto.Roles.Any())
            {
                await _repo.RemoveAllRolesFromUserAsync(user.Id, ct);

                var newUserRoles = new List<UserRole>();
                foreach (var roleName in requestDto.Roles.Distinct())
                {
                    var role = await _repo.GetRoleByNameAsync(roleName.Trim(), ct);
                    if (role is not null)
                    {
                        newUserRoles.Add(new UserRole
                        {
                            Id = Guid.NewGuid(),
                            UserId = user.Id,
                            RoleId = role.RoleId,
                            ValidFrom = now,
                            ValidTo = null,
                            AssignedBy = _currentUser.UserId
                        });
                    }
                }

                if (newUserRoles.Any())
                {
                    await _repo.AssignRolesToUserAsync(user.Id, newUserRoles, ct);
                }
            }

            if (!user.IsActive)
            {
                await _repo.RevokeAllUserSessionsAsync(user.Id, now, ct);
            }
        }, ct);

        _logger.LogInformation("Administrative updates applied for user {UserId}.", userId);
        return ApiResponse<bool>.Success(true, "User account configuration updated successfully by admin.");
    }

    public async Task<ApiResponse<bool>> ToggleUserStatusAsync(Guid userId, bool isActive, CancellationToken ct = default)
    {
        var user = await _repo.GetUserByIdAsync(userId, ct);
        if (user is null)
        {
            return ApiResponse<bool>.Failure("User not found.", AuthErrors.UserNotFound);
        }

        var now = _date.UtcNow;
        user.IsActive = isActive;
        user.UpdatedAt = now;

        await _uow.ExecuteInTransactionAsync(async () =>
        {
            await _repo.UpdateUserAsync(user, ct);

            if (!isActive)
            {
                await _repo.RevokeAllUserSessionsAsync(userId, now, ct);
                _logger.LogWarning("User {UserId} has been deactivated. All active sessions terminated.", userId);
            }
        }, ct);

        string statusMessage = isActive ? "activated" : "suspended";
        return ApiResponse<bool>.Success(true, $"User account has been {statusMessage} successfully.");
    }

    public async Task<ApiResponse<bool>> UnlockUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _repo.GetUserByIdAsync(userId, ct);
        if (user is null)
        {
            return ApiResponse<bool>.Failure("User not found.", AuthErrors.UserNotFound);
        }

        user.AccessFailedCount = 0;
        user.LockoutEnd = null;
        user.UpdatedAt = _date.UtcNow;

        await _repo.UpdateUserAsync(user, ct);
        _logger.LogInformation("User {UserId} has been manually unlocked by administration.", userId);

        return ApiResponse<bool>.Success(true, "User account has been unlocked successfully. The user can now log in.");
    }


    private static UserResponseDto MapToUserResponseDto(ApplicationUser user, List<string> roles)
    {
        return new UserResponseDto
        {
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            DisplayName = $"{user.FirstName} {user.LastName}".Trim(),
            PhoneNumber = user.PhoneNumber,
            PhoneVerified = user.PhoneVerified,
            EmailVerified = user.EmailVerified,
            TwoFactorEnabled = user.TwoFactorEnabled,
            IsActive = user.IsActive,
            AvatarUrl = user.AvatarUrl,
            LastLoginUtc = user.LastLoginUtc,
            CreatedAt = user.CreatedAt,
            Roles = roles,
            AuthResponseDto = null 
        };
    }
}