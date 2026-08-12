using ClinicManagementSystem.Application.Common.Errors;
using ClinicManagementSystem.Application.Common.Interfaces;
using ClinicManagementSystem.Application.Common.Models;
using ClinicManagementSystem.Application.DTOs.Auth.Permissions;
using ClinicManagementSystem.Application.Interfaces.Repositories;
using ClinicManagementSystem.Application.Interfaces.Services.Auth;
using ClinicManagementSystem.Domain.Entities.Auth;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace ClinicManagementSystem.Application.Services.Auth;

public sealed class PermissionService : IPermissionService
{
    private readonly IIdentityRepository _repo;
    private readonly ILogger<PermissionService> _logger;
    private readonly IDateTimeProvider _date;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<CreatePermissionRequestDto> _createPermissionValidator;
    private readonly IValidator<UpdatePermissionRequestDto> _updatePermissionValidator;

    public PermissionService(
        IIdentityRepository repo,
        ILogger<PermissionService> logger,
        IDateTimeProvider date,
        ICurrentUserService currentUser,
        IValidator<CreatePermissionRequestDto> createPermissionValidator,
        IValidator<UpdatePermissionRequestDto> updatePermissionValidator)
    {
        _repo = repo;
        _logger = logger;
        _date = date;
        _currentUser = currentUser;
        _createPermissionValidator = createPermissionValidator;
        _updatePermissionValidator = updatePermissionValidator;
    }

    public async Task<ApiResponse<IReadOnlyList<PermissionResponseDto>>> GetAllPermissionsAsync(CancellationToken ct = default)
    {
        var permissions = await _repo.GetAllPermissionsAsync(ct);
        if (permissions is null || !permissions.Any())
        {
            return ApiResponse<IReadOnlyList<PermissionResponseDto>>.Success(
                Array.Empty<PermissionResponseDto>(),
                "No permissions found.");
        }

        return ApiResponse<IReadOnlyList<PermissionResponseDto>>.Success(permissions, "Permissions retrieved successfully.");
    }

    public async Task<ApiResponse<IReadOnlyList<GroupedPermissionsResponseDto>>> GetGroupedPermissionsAsync(CancellationToken ct = default)
    {
        var permissions = await _repo.GetAllPermissionsAsync(ct);

        if (permissions is null || !permissions.Any())
        {
            return ApiResponse<IReadOnlyList<GroupedPermissionsResponseDto>>.Success(
                Array.Empty<GroupedPermissionsResponseDto>(),
                "No permissions found.");
        }

        var grouped = permissions
            .GroupBy(p => string.IsNullOrWhiteSpace(p.Module) ? "General" : p.Module.Trim())
            .Select(g => new GroupedPermissionsResponseDto
            {
                Module = g.Key,
                Permissions = g.ToList()
            })
            .OrderBy(g => g.Module)
            .ToList();

        return ApiResponse<IReadOnlyList<GroupedPermissionsResponseDto>>.Success(grouped, "Grouped permissions retrieved successfully.");
    }

    public async Task<ApiResponse<PermissionResponseDto>> GetPermissionByIdAsync(Guid permissionId, CancellationToken ct = default)
    {
        if (permissionId == Guid.Empty)
        {
            return ApiResponse<PermissionResponseDto>.Failure(
                "Invalid permission identifier.",
                PermissionErrors.InvalidPermissionId);
        }

        var permission = await _repo.GetPermissionByIdAsync(permissionId, ct);
        if (permission is null)
        {
            return ApiResponse<PermissionResponseDto>.Failure(
                "Permission not found.",
                PermissionErrors.PermissionNotFound);
        }

        return ApiResponse<PermissionResponseDto>.Success(permission, "Permission details retrieved successfully.");
    }

    public async Task<ApiResponse<PermissionResponseDto>> GetPermissionByCodeAsync(string permissionCode, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(permissionCode))
        {
            return ApiResponse<PermissionResponseDto>.Failure(
                "Invalid permission code.",
                PermissionErrors.InvalidPermissionCodeFormat);
        }

        var permission = await _repo.GetPermissionByCodeAsync(permissionCode.Trim(), ct);
        if (permission is null)
        {
            return ApiResponse<PermissionResponseDto>.Failure(
                "Permission not found.",
                PermissionErrors.PermissionNotFound);
        }

        return ApiResponse<PermissionResponseDto>.Success(permission, "Permission details retrieved successfully.");
    }

    public async Task<ApiResponse<Guid>> CreatePermissionAsync(CreatePermissionRequestDto requestDto, CancellationToken ct = default)
    {
        var validation = await _createPermissionValidator.ValidateAsync(requestDto, ct);
        if (!validation.IsValid)
        {
            return ApiResponse<Guid>.Failure(
                "Permission creation payload validation failed.",
                validation.Errors.Select(e => new ErrorModel(e.PropertyName, e.ErrorMessage, e.ErrorCode)).ToList());
        }

        var normalizedCode = requestDto.PermissionCode.Trim().ToUpperInvariant();

        var exists = await _repo.PermissionExistsByCodeAsync(normalizedCode, ct);
        if (exists)
        {
            return ApiResponse<Guid>.Failure(
                "Permission code already exists.",
                PermissionErrors.PermissionAlreadyExists);
        }

        var now = _date.UtcNow;

        var permission = new Permission
        {
            Id = Guid.NewGuid(),
            PermissionCode = normalizedCode,
            PermissionName = requestDto.PermissionName.Trim(),
            Module = string.IsNullOrWhiteSpace(requestDto.Module) ? "General" : requestDto.Module.Trim(),
            Description = requestDto.Description?.Trim(),
            CreatedAt = now,
            CreatedBy = _currentUser.UserId,
            UpdatedAt = now
        };

        await _repo.CreatePermissionAsync(permission, ct);

        _logger.LogInformation("Permission '{PermissionCode}' (ID: {PermissionId}) successfully created.", normalizedCode, permission.Id);

        return ApiResponse<Guid>.Success(permission.Id, "Permission created successfully.");
    }

    public async Task<ApiResponse<bool>> UpdatePermissionAsync(Guid permissionId, UpdatePermissionRequestDto requestDto, CancellationToken ct = default)
    {
        if (permissionId == Guid.Empty)
        {
            return ApiResponse<bool>.Failure(
                "Invalid permission identifier.",
                PermissionErrors.InvalidPermissionId);
        }

        var validation = await _updatePermissionValidator.ValidateAsync(requestDto, ct);
        if (!validation.IsValid)
        {
            return ApiResponse<bool>.Failure(
                "Permission update payload validation failed.",
                validation.Errors.Select(e => new ErrorModel(e.PropertyName, e.ErrorMessage, e.ErrorCode)).ToList());
        }

        var existingPermission = await _repo.GetPermissionByIdAsync(permissionId, ct);
        if (existingPermission is null)
        {
            return ApiResponse<bool>.Failure(
                "Permission not found.",
                PermissionErrors.PermissionNotFound);
        }

        var permission = new Permission
        {
          Id = permissionId,
          PermissionCode = existingPermission.PermissionCode,
          PermissionName = requestDto.PermissionName.Trim(),
          Module = string.IsNullOrWhiteSpace(requestDto.Module) ? "General" : requestDto.Module.Trim(),
          Description = requestDto.Description,
          IsActive = requestDto.IsActive,
          UpdatedAt = _date.UtcNow,
          UpdatedBy = _currentUser.UserId
        };

        await _repo.UpdatePermissionAsync(permission, ct);

        _logger.LogInformation("Permission '{PermissionId}' updated successfully.", permissionId);
        return ApiResponse<bool>.Success(true, "Permission details updated successfully.");
    }

    public async Task<ApiResponse<bool>> DeletePermissionAsync(Guid permissionId, CancellationToken ct = default)
    {
        if (permissionId == Guid.Empty)
        {
            return ApiResponse<bool>.Failure(
                "Invalid permission identifier.",
                PermissionErrors.InvalidPermissionId);
        }

        var permission = await _repo.GetPermissionByIdAsync(permissionId, ct);
        if (permission is null)
        {
            return ApiResponse<bool>.Failure(
                "Permission not found.",
                PermissionErrors.PermissionNotFound);
        }

        var assignedRoleCount = await _repo.GetAssignedRoleCountForPermissionAsync(permissionId, ct);
        if (assignedRoleCount > 0)
        {
            return ApiResponse<bool>.Failure(
                "Permission deletion blocked.",
                PermissionErrors.PermissionInUse);
        }


        await _repo.DeletePermissionAsync(permissionId, ct);

        _logger.LogInformation("Permission '{PermissionId}' deleted successfully.", permissionId);
        return ApiResponse<bool>.Success(true, "Permission deleted successfully.");
    }

    public async Task<ApiResponse<PaginatedList<PermissionResponseDto>>> SearchPermissionsAsync(PermissionSearchFilter filter, CancellationToken ct = default)
    {
        var (permissions, totalCount) = await _repo.SearchPermissionsAsync(filter, ct);

        if (permissions is null || !permissions.Any())
        {
            var emptyList = new PaginatedList<PermissionResponseDto>(Array.Empty<PermissionResponseDto>(), 0, filter.PageNumber, filter.PageSize);
            return ApiResponse<PaginatedList<PermissionResponseDto>>.Success(emptyList, "No permissions matched the search criteria.");
        }

        var permissionsDto = permissions.Select(p => new PermissionResponseDto
        {
            PermissionId = p.Id,
            PermissionCode = p.PermissionCode,
            PermissionName = p.PermissionName,
            Module = p.Module,
            Description = p.Description,
            IsActive = p.IsActive
        }).ToList();

        var paginatedResult = new PaginatedList<PermissionResponseDto>(permissionsDto, totalCount, filter.PageNumber, filter.PageSize);

        return ApiResponse<PaginatedList<PermissionResponseDto>>.Success(paginatedResult, "Permissions search completed successfully.");
    }

    public async Task<ApiResponse<bool>> TogglePermissionStatusAsync(Guid permissionId, bool isActive, CancellationToken ct = default)
    {
        if (permissionId == Guid.Empty)
        {
            return ApiResponse<bool>.Failure(
                "Invalid permission identifier.",
                PermissionErrors.InvalidPermissionId);
        }

        var existingPermission = await _repo.GetPermissionByIdAsync(permissionId, ct);
        if (existingPermission is null)
        {
            return ApiResponse<bool>.Failure(
                "Permission not found.",
                PermissionErrors.PermissionNotFound);
        }

        var updatedPermission = new Permission
        {
            Id = existingPermission.PermissionId,
            PermissionCode = existingPermission.PermissionCode,
            PermissionName = existingPermission.PermissionName,
            Module = existingPermission.Module,
            Description = existingPermission.Description,
            IsActive = isActive,
            UpdatedAt = _date.UtcNow,
            UpdatedBy = _currentUser.UserId
        };

        await _repo.UpdatePermissionAsync(updatedPermission, ct);

        _logger.LogInformation("Permission '{PermissionId}' status successfully toggled to Active = {IsActive} by User '{UpdatedBy}'.", 
            permissionId, isActive, _currentUser.UserId);

        return ApiResponse<bool>.Success(true, $"Permission status successfully updated to {(isActive ? "Active" : "Inactive")}.");
    }
}