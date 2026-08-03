using ClinicManagementSystem.Application.Common.Errors;
using ClinicManagementSystem.Application.Common.Interfaces;
using ClinicManagementSystem.Application.Common.Models;
using ClinicManagementSystem.Application.DTOs.Auth.AssignPermissions;
using ClinicManagementSystem.Application.Interfaces.Repositories;
using ClinicManagementSystem.Application.Interfaces.Services.Auth;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace ClinicManagementSystem.Application.Services.Auth;

public sealed class RolePermissionService : IRolePermissionService
{
    private readonly IIdentityRepository _repo;
    private readonly ILogger<RolePermissionService> _logger;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<AssignPermissionsToRoleRequestDto> _assignPermissionsValidator;

    public RolePermissionService(
        IIdentityRepository repo,
        ILogger<RolePermissionService> logger,
        IUnitOfWork uow,
        IValidator<AssignPermissionsToRoleRequestDto> assignPermissionsValidator)
    {
        _repo = repo;
        _logger = logger;
        _uow = uow;
        _assignPermissionsValidator = assignPermissionsValidator;
    }

    public async Task<ApiResponse<RolePermissionsDetailsResponseDto>> GetPermissionsByRoleIdAsync(Guid roleId, CancellationToken ct = default)
    {
        if (roleId == Guid.Empty)
        {
            return ApiResponse<RolePermissionsDetailsResponseDto>.Failure(
                "Invalid role identifier.",
                RoleErrors.InvalidRoleId);
        }

        var role = await _repo.GetRoleByIdAsync(roleId, ct);
        if (role is null)
        {
            return ApiResponse<RolePermissionsDetailsResponseDto>.Failure(
                "Role not found.",
                RoleErrors.RoleNotFound);
        }

        var permissions = await _repo.GetPermissionsByRoleIdAsync(roleId, ct);

        var response = new RolePermissionsDetailsResponseDto
        {
            RoleId = role.RoleId,
            RoleName = role.RoleName,
            AssignedPermissions = permissions
        };

        return ApiResponse<RolePermissionsDetailsResponseDto>.Success(response, "Role permissions retrieved successfully.");
    }

    public async Task<ApiResponse<bool>> AssignPermissionsToRoleAsync(AssignPermissionsToRoleRequestDto requestDto, CancellationToken ct = default)
    {
        var validation = await _assignPermissionsValidator.ValidateAsync(requestDto, ct);
        if (!validation.IsValid)
        {
            return ApiResponse<bool>.Failure(
                "Payload validation failed.",
                validation.Errors.Select(e => new ErrorModel(e.PropertyName, e.ErrorMessage, e.ErrorCode)).ToList());
        }

        var role = await _repo.GetRoleByIdAsync(requestDto.RoleId, ct);
        if (role is null)
        {
            return ApiResponse<bool>.Failure(
                "Role not found.",
                RoleErrors.RoleNotFound);
        }

        if (role.IsSystemRole)
        {
            return ApiResponse<bool>.Failure(
                "Operation prohibited.",
                RolePermissionErrors.CannotModifySystemRolePermissions);
        }

        var distinctPermissionIds = requestDto.PermissionIds.Distinct().ToList();
        var existingPermissions = await _repo.GetPermissionsByIdsAsync(distinctPermissionIds, ct);

        if (existingPermissions.Count != distinctPermissionIds.Count)
        {
            return ApiResponse<bool>.Failure(
                "One or more permission IDs are invalid or non-existent.",
                PermissionErrors.PermissionNotFound);
        }

        await _uow.ExecuteInTransactionAsync(async () =>
        {
            await _repo.RemoveAllPermissionsFromRoleAsync(requestDto.RoleId, ct);
            await _repo.AssignPermissionsToRoleAsync(requestDto.RoleId, distinctPermissionIds, ct);
        }, ct);

        _logger.LogInformation("Successfully assigned {Count} permissions to Role '{RoleId}'.", distinctPermissionIds.Count, requestDto.RoleId);
        return ApiResponse<bool>.Success(true, "Permissions assigned to role successfully.");
    }

    public async Task<ApiResponse<Guid>> AddPermissionToRoleAsync(Guid roleId, Guid permissionId, CancellationToken ct = default)
    {
        if (roleId == Guid.Empty)
        {
            return ApiResponse<Guid>.Failure("Invalid role identifier.", RoleErrors.InvalidRoleId);
        }

        if (permissionId == Guid.Empty)
        {
            return ApiResponse<Guid>.Failure("Invalid permission identifier.", PermissionErrors.InvalidPermissionId);
        }

        var role = await _repo.GetRoleByIdAsync(roleId, ct);
        if (role is null)
        {
            return ApiResponse<Guid>.Failure("Role not found.", RoleErrors.RoleNotFound);
        }

        if (role.IsSystemRole)
        {
            return ApiResponse<Guid>.Failure("Operation prohibited.", RolePermissionErrors.CannotModifySystemRolePermissions);
        }

        var permission = await _repo.GetPermissionByIdAsync(permissionId, ct);
        if (permission is null)
        {
            return ApiResponse<Guid>.Failure("Permission not found.", PermissionErrors.PermissionNotFound);
        }

        var exists = await _repo.RoleHasPermissionAsync(roleId, permissionId, ct);
        if (exists)
        {
            return ApiResponse<Guid>.Failure("Permission already assigned.", RolePermissionErrors.RolePermissionAlreadyExists);
        }

        var rolePermissionId = Guid.NewGuid();

        await _uow.ExecuteInTransactionAsync(async () =>
        {
            await _repo.AddPermissionToRoleAsync(rolePermissionId, roleId, permissionId, ct);
        }, ct);

        _logger.LogInformation("Permission '{PermissionId}' added to Role '{RoleId}'.", permissionId, roleId);
        return ApiResponse<Guid>.Success(rolePermissionId, "Permission added to role successfully.");
    }

    public async Task<ApiResponse<bool>> RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId, CancellationToken ct = default)
    {
        if (roleId == Guid.Empty)
        {
            return ApiResponse<bool>.Failure("Invalid role identifier.", RoleErrors.InvalidRoleId);
        }

        if (permissionId == Guid.Empty)
        {
            return ApiResponse<bool>.Failure("Invalid permission identifier.", PermissionErrors.InvalidPermissionId);
        }

        var role = await _repo.GetRoleByIdAsync(roleId, ct);
        if (role is null)
        {
            return ApiResponse<bool>.Failure("Role not found.", RoleErrors.RoleNotFound);
        }

        if (role.IsSystemRole)
        {
            return ApiResponse<bool>.Failure("Operation prohibited.", RolePermissionErrors.CannotModifySystemRolePermissions);
        }

        var exists = await _repo.RoleHasPermissionAsync(roleId, permissionId, ct);
        if (!exists)
        {
            return ApiResponse<bool>.Failure("Mapping not found.", RolePermissionErrors.RolePermissionNotFound);
        }

        await _uow.ExecuteInTransactionAsync(async () =>
        {
            await _repo.RemovePermissionFromRoleAsync(roleId, permissionId, ct);
        }, ct);

        _logger.LogInformation("Permission '{PermissionId}' removed from Role '{RoleId}'.", permissionId, roleId);
        return ApiResponse<bool>.Success(true, "Permission removed from role successfully.");
    }

    public async Task<ApiResponse<bool>> HasPermissionAsync(Guid roleId, string permissionCode, CancellationToken ct = default)
    {
        if (roleId == Guid.Empty)
        {
            return ApiResponse<bool>.Failure("Invalid role identifier.", RoleErrors.InvalidRoleId);
        }

        if (string.IsNullOrWhiteSpace(permissionCode))
        {
            return ApiResponse<bool>.Failure("Invalid permission code.", PermissionErrors.InvalidPermissionCodeFormat);
        }

        var hasPermission = await _repo.RoleHasPermissionByCodeAsync(roleId, permissionCode.Trim(), ct);
        return ApiResponse<bool>.Success(hasPermission, "Permission status checked successfully.");
    }
}