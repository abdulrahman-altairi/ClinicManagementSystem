using ClinicManagementSystem.Application.Common.Errors;
using ClinicManagementSystem.Application.Common.Interfaces;
using ClinicManagementSystem.Application.Common.Models;
using ClinicManagementSystem.Application.DTOs.Auth.UserPermissions;
using ClinicManagementSystem.Domain.Entities.Auth;
using ClinicManagementSystem.Application.Interfaces.Repositories;
using ClinicManagementSystem.Application.Interfaces.Services.Auth;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace ClinicManagementSystem.Application.Services.Auth;

public sealed class UserPermissionService : IUserPermissionService
{
    private readonly IIdentityRepository _repo;
    private readonly ILogger<UserPermissionService> _logger;
    private readonly IUnitOfWork _uow;
    private readonly IDateTimeProvider _date;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<AddUserPermissionOverrideRequestDto> _addOverrideValidator;
    private readonly IValidator<UpdateUserPermissionOverrideRequestDto> _updateOverrideValidator;
    private readonly IValidator<SetUserPermissionsBulkRequestDto> _bulkOverrideValidator;

    public UserPermissionService(
        IIdentityRepository repo,
        ILogger<UserPermissionService> logger,
        IUnitOfWork uow,
        IDateTimeProvider date,
        ICurrentUserService currentUser,
        IValidator<AddUserPermissionOverrideRequestDto> addOverrideValidator,
        IValidator<UpdateUserPermissionOverrideRequestDto> updateOverrideValidator,
        IValidator<SetUserPermissionsBulkRequestDto> bulkOverrideValidator)
    {
        _repo = repo;
        _logger = logger;
        _uow = uow;
        _date = date;
        _currentUser = currentUser;
        _addOverrideValidator = addOverrideValidator;
        _updateOverrideValidator = updateOverrideValidator;
        _bulkOverrideValidator = bulkOverrideValidator;
    }

    public async Task<ApiResponse<UserPermissionOverridesDetailsResponseDto>> GetUserPermissionOverridesAsync(Guid userId, CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
        {
            return ApiResponse<UserPermissionOverridesDetailsResponseDto>.Failure(
                "Invalid user identifier.",
                UserErrors.InvalidUserId);
        }

        var user = await _repo.GetUserByIdAsync(userId, ct);
        if (user is null)
        {
            return ApiResponse<UserPermissionOverridesDetailsResponseDto>.Failure(
                "User not found.",
                UserErrors.UserNotFound);
        }

        var overrides = await _repo.GetUserPermissionOverridesAsync(userId, ct);

        var response = new UserPermissionOverridesDetailsResponseDto
        {
            UserId = user.Id,
            Username = user.Username,
            Overrides = overrides
        };

        return ApiResponse<UserPermissionOverridesDetailsResponseDto>.Success(response, "User permission overrides retrieved successfully.");
    }

    public async Task<ApiResponse<Guid>> AddUserPermissionOverrideAsync(AddUserPermissionOverrideRequestDto requestDto, CancellationToken ct = default)
    {
        var validation = await _addOverrideValidator.ValidateAsync(requestDto, ct);
        if (!validation.IsValid)
        {
            return ApiResponse<Guid>.Failure(
                "Payload validation failed.",
                validation.Errors.Select(e => new ErrorModel(e.PropertyName, e.ErrorMessage, e.ErrorCode)).ToList());
        }

        var user = await _repo.GetUserByIdAsync(requestDto.UserId, ct);
        if (user is null)
        {
            return ApiResponse<Guid>.Failure(
                "User not found.",
                UserErrors.UserNotFound);
        }

        var permission = await _repo.GetPermissionByIdAsync(requestDto.PermissionId, ct);
        if (permission is null)
        {
            return ApiResponse<Guid>.Failure(
                "Permission not found.",
                PermissionErrors.PermissionNotFound);
        }

        var grantTypeString = requestDto.GrantType.ToString(); 
        var exists = await _repo.UserPermissionOverrideExistsAsync(requestDto.UserId, requestDto.PermissionId, grantTypeString, ct);
        if (exists)
        {
            return ApiResponse<Guid>.Failure(
                "A permission override of this type already exists for the specified user and permission.",
                UserPermissionErrors.UserPermissionAlreadyExists);
        }

        var userPermission = new UserPermission
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            PermissionId = permission.Id,
            GrantType = requestDto.GrantType,
            Reason = requestDto.Reason?.Trim(),
            ValidFrom = requestDto.ValidFrom ?? _date.UtcNow,
            ValidTo = requestDto.ValidTo,
            CreatedBy = _currentUser.UserId,
            CreatedAt = _date.UtcNow
        };


        await _repo.CreateUserPermissionOverrideAsync(userPermission, ct);

        _logger.LogInformation("Permission override '{GrantType}' created for User '{UserId}' on Permission '{PermissionId}'.", grantTypeString, requestDto.UserId, requestDto.PermissionId);
        return ApiResponse<Guid>.Success(userPermission.Id, "User permission override added successfully.");
    }

    public async Task<ApiResponse<bool>> UpdateUserPermissionOverrideAsync(Guid userPermissionId, UpdateUserPermissionOverrideRequestDto requestDto, CancellationToken ct = default)
    {
        if (userPermissionId == Guid.Empty)
        {
            return ApiResponse<bool>.Failure(
                "Invalid user permission identifier.",
                UserPermissionErrors.UserPermissionNotFound);
        }

        var validation = await _updateOverrideValidator.ValidateAsync(requestDto, ct);
        if (!validation.IsValid)
        {
            return ApiResponse<bool>.Failure(
                "Payload validation failed.",
                validation.Errors.Select(e => new ErrorModel(e.PropertyName, e.ErrorMessage, e.ErrorCode)).ToList());
        }

        var existingOverride = await _repo.GetUserPermissionOverrideByIdAsync(userPermissionId, ct);
        if (existingOverride is null)
        {
            return ApiResponse<bool>.Failure(
                "User permission override not found.",
                UserPermissionErrors.UserPermissionNotFound);
        }

        var grantTypeString = requestDto.GrantType.ToString();
        var validFrom = requestDto.ValidFrom ?? existingOverride.ValidFrom;


        var updatedPermission = new UserPermission
        {
            Id = userPermissionId,
            GrantType = requestDto.GrantType, 
            Reason = requestDto.Reason?.Trim(),
            ValidFrom = validFrom,
            ValidTo = requestDto.ValidTo,
            IsActive = requestDto.IsActive,
            UpdatedBy = _currentUser.UserId,
            UpdatedAt = _date.UtcNow
        };

        await _repo.UpdateUserPermissionOverrideAsync(updatedPermission, ct);

        _logger.LogInformation("User permission override '{UserPermissionId}' updated successfully.", userPermissionId);
        return ApiResponse<bool>.Success(true, "User permission override updated successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteUserPermissionOverrideAsync(Guid userPermissionId, CancellationToken ct = default)
    {
        if (userPermissionId == Guid.Empty)
        {
            return ApiResponse<bool>.Failure(
                "Invalid user permission identifier.",
                UserPermissionErrors.UserPermissionNotFound);
        }

        var existingOverride = await _repo.GetUserPermissionOverrideByIdAsync(userPermissionId, ct);
        if (existingOverride is null)
        {
            return ApiResponse<bool>.Failure(
                "User permission override not found.",
                UserPermissionErrors.UserPermissionNotFound);
        }


         await _repo.DeleteUserPermissionOverrideAsync(userPermissionId, ct);

        _logger.LogInformation("User permission override '{UserPermissionId}' deleted successfully.", userPermissionId);
        return ApiResponse<bool>.Success(true, "User permission override deleted successfully.");
    }

    public async Task<ApiResponse<bool>> SetUserPermissionsBulkAsync(SetUserPermissionsBulkRequestDto requestDto, CancellationToken ct = default)
    {
        var validation = await _bulkOverrideValidator.ValidateAsync(requestDto, ct);
        if (!validation.IsValid)
        {
            return ApiResponse<bool>.Failure(
                "Payload validation failed.",
                validation.Errors.Select(e => new ErrorModel(e.PropertyName, e.ErrorMessage, e.ErrorCode)).ToList());
        }

        var user = await _repo.GetUserByIdAsync(requestDto.UserId, ct);
        if (user is null)
        {
            return ApiResponse<bool>.Failure(
                "User not found.",
                UserErrors.UserNotFound);
        }

        var distinctPermissionIds = requestDto.Overrides.Select(o => o.PermissionId).Distinct().ToList();

        if (distinctPermissionIds.Any())
        {
            var existingPermissions = await _repo.GetPermissionsByIdsAsync(distinctPermissionIds, ct);
            if (existingPermissions.Count != distinctPermissionIds.Count)
            {
                return ApiResponse<bool>.Failure(
                    "One or more permission IDs are invalid or non-existent.",
                    PermissionErrors.PermissionNotFound);
            }
        }

        var permissionsToSet = requestDto.Overrides.Select(o => new UserPermission
        {
            Id = Guid.NewGuid(),
            UserId = requestDto.UserId,
            PermissionId = o.PermissionId,
            GrantType = o.GrantType, 
            Reason = o.Reason?.Trim(),
            ValidFrom = o.ValidFrom ?? _date.UtcNow,
            ValidTo = o.ValidTo,
            GrantedBy = _currentUser.UserId,
            IsActive = true
        }).ToList();

        await _uow.ExecuteInTransactionAsync(async () =>
        {
            await _repo.RemoveAllPermissionOverridesFromUserAsync(requestDto.UserId, ct);
            if (permissionsToSet.Any())
            {
                await _repo.AddUserPermissionOverridesBulkAsync(requestDto.UserId, permissionsToSet, ct);
            }
        }, ct);
        _logger.LogInformation("Bulk permission overrides updated for User '{UserId}'. Total: {Count}.", requestDto.UserId, requestDto.Overrides.Count);
        return ApiResponse<bool>.Success(true, "Bulk user permission overrides updated successfully.");
    }

    public async Task<ApiResponse<bool>> EvaluateUserPermissionAsync(Guid userId, string permissionCode, CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
        {
            return ApiResponse<bool>.Failure(
                "Invalid user identifier.",
                UserErrors.InvalidUserId);
        }

        if (string.IsNullOrWhiteSpace(permissionCode))
        {
            return ApiResponse<bool>.Failure(
                "Invalid permission code format.",
                PermissionErrors.InvalidPermissionCodeFormat);
        }

        var normalizedCode = permissionCode.Trim();

        var hasDeny = await _repo.HasActiveDenyOverrideAsync(userId, normalizedCode, ct);
        if (hasDeny)
        {
            return ApiResponse<bool>.Success(false, "Permission explicitly denied at user level.");
        }

        var hasDirectGrant = await _repo.HasActiveGrantOverrideAsync(userId, normalizedCode, ct);
        if (hasDirectGrant)
        {
            return ApiResponse<bool>.Success(true, "Permission explicitly granted at user level.");
        }

        var userRoles = await _repo.GetRolesByUserIdAsync(userId, ct);
        var now = _date.UtcNow;
        var activeRoleIds = userRoles.Where(r => r.IsActive(now)).Select(r => r.RoleId).ToList();

        if (activeRoleIds.Any())
        {
            var hasRolePermission = await _repo.AnyRoleHasPermissionByCodeAsync(activeRoleIds, normalizedCode, ct);
            if (hasRolePermission)
            {
                return ApiResponse<bool>.Success(true, "Permission granted via user role.");
            }
        }

        return ApiResponse<bool>.Success(false, "Permission not granted.");
    }

    public async Task<ApiResponse<List<string>>> GetEffectivePermissionsForUserAsync(Guid userId, CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
        {
            return ApiResponse<List<string>>.Failure(
                "Invalid user identifier.",
                UserErrors.InvalidUserId);
        }

        var user = await _repo.GetUserByIdAsync(userId, ct);
        if (user is null)
        {
            return ApiResponse<List<string>>.Failure(
                "User not found.",
                UserErrors.UserNotFound);
        }

        var effectivePermissions = await _repo.GetEffectivePermissionsByUserIdAsync(userId, ct);

        _logger.LogInformation("Retrieved {Count} effective permissions for User '{UserId}'.", effectivePermissions.Count, userId);

        return ApiResponse<List<string>>.Success(
            effectivePermissions, 
            "Effective permissions retrieved successfully.");
    }
}