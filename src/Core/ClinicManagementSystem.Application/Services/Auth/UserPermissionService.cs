using ClinicManagementSystem.Application.Common.Errors;
using ClinicManagementSystem.Application.Common.Interfaces;
using ClinicManagementSystem.Application.Common.Models;
using ClinicManagementSystem.Application.DTOs.Auth.UserPermissions;
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
    private readonly IValidator<AddUserPermissionOverrideRequestDto> _addOverrideValidator;
    private readonly IValidator<UpdateUserPermissionOverrideRequestDto> _updateOverrideValidator;
    private readonly IValidator<SetUserPermissionsBulkRequestDto> _bulkOverrideValidator;

    public UserPermissionService(
        IIdentityRepository repo,
        ILogger<UserPermissionService> logger,
        IUnitOfWork uow,
        IValidator<AddUserPermissionOverrideRequestDto> addOverrideValidator,
        IValidator<UpdateUserPermissionOverrideRequestDto> updateOverrideValidator,
        IValidator<SetUserPermissionsBulkRequestDto> bulkOverrideValidator)
    {
        _repo = repo;
        _logger = logger;
        _uow = uow;
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

    public async Task<ApiResponse<Guid>> AddUserPermissionOverrideAsync(AddUserPermissionOverrideRequestDto requestDto, Guid? grantedBy = null, CancellationToken ct = default)
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

        var userPermissionId = Guid.NewGuid();
        var validFrom = requestDto.ValidFrom ?? DateTimeOffset.UtcNow;

        await _uow.ExecuteInTransactionAsync(async () =>
        {
            await _repo.CreateUserPermissionOverrideAsync(
                userPermissionId,
                requestDto.UserId,
                requestDto.PermissionId,
                grantTypeString,
                requestDto.Reason?.Trim(),
                validFrom,
                requestDto.ValidTo,
                grantedBy,
                ct);
        }, ct);

        _logger.LogInformation("Permission override '{GrantType}' created for User '{UserId}' on Permission '{PermissionId}'.", grantTypeString, requestDto.UserId, requestDto.PermissionId);
        return ApiResponse<Guid>.Success(userPermissionId, "User permission override added successfully.");
    }

    public async Task<ApiResponse<bool>> UpdateUserPermissionOverrideAsync(Guid userPermissionId, UpdateUserPermissionOverrideRequestDto requestDto, Guid? updatedBy = null, CancellationToken ct = default)
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

        await _uow.ExecuteInTransactionAsync(async () =>
        {

            await _repo.UpdateUserPermissionOverrideAsync(
                userPermissionId,
                grantTypeString,
                requestDto.Reason?.Trim(),
                validFrom,
                requestDto.ValidTo,
                requestDto.IsActive,
                updatedBy,
                ct);
        }, ct);

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

        await _uow.ExecuteInTransactionAsync(async () =>
        {
            await _repo.DeleteUserPermissionOverrideAsync(userPermissionId, ct);
        }, ct);

        _logger.LogInformation("User permission override '{UserPermissionId}' deleted successfully.", userPermissionId);
        return ApiResponse<bool>.Success(true, "User permission override deleted successfully.");
    }

    public async Task<ApiResponse<bool>> SetUserPermissionsBulkAsync(SetUserPermissionsBulkRequestDto requestDto, Guid? grantedBy = null, CancellationToken ct = default)
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
        var existingPermissions = await _repo.GetPermissionsByIdsAsync(distinctPermissionIds, ct);
        if (existingPermissions.Count != distinctPermissionIds.Count)
        {
            return ApiResponse<bool>.Failure(
                "One or more permission IDs are invalid or non-existent.",
                PermissionErrors.PermissionNotFound);
        }

        await _uow.ExecuteInTransactionAsync(async () =>
        {
            await _repo.RemoveAllPermissionOverridesFromUserAsync(requestDto.UserId, ct);
            await _repo.AddUserPermissionOverridesBulkAsync(requestDto.UserId, requestDto.Overrides, grantedBy, ct);
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
        var activeRoleIds = userRoles.Where(r => r.IsActive).Select(r => r.RoleId).ToList();

        foreach (var roleId in activeRoleIds)
        {
            var hasRolePermission = await _repo.RoleHasPermissionByCodeAsync(roleId, normalizedCode, ct);
            if (hasRolePermission)
            {
                return ApiResponse<bool>.Success(true, "Permission granted via user role.");
            }
        }

        return ApiResponse<bool>.Success(false, "Permission not granted.");
    }
}