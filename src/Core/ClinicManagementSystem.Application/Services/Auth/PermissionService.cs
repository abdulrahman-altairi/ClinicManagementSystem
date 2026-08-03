using ClinicManagementSystem.Application.Common.Errors;
using ClinicManagementSystem.Application.Common.Interfaces;
using ClinicManagementSystem.Application.Common.Models;
using ClinicManagementSystem.Application.DTOs.Auth.Permissions;
using ClinicManagementSystem.Application.Interfaces.Repositories;
using ClinicManagementSystem.Application.Interfaces.Services.Auth;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace ClinicManagementSystem.Application.Services.Auth;

public sealed class PermissionService : IPermissionService
{
    private readonly IIdentityRepository _repo;
    private readonly ILogger<PermissionService> _logger;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<CreatePermissionRequestDto> _createPermissionValidator;
    private readonly IValidator<UpdatePermissionRequestDto> _updatePermissionValidator;

    public PermissionService(
        IIdentityRepository repo,
        ILogger<PermissionService> logger,
        IUnitOfWork uow,
        IValidator<CreatePermissionRequestDto> createPermissionValidator,
        IValidator<UpdatePermissionRequestDto> updatePermissionValidator)
    {
        _repo = repo;
        _logger = logger;
        _uow = uow;
        _createPermissionValidator = createPermissionValidator;
        _updatePermissionValidator = updatePermissionValidator;
    }

    public async Task<ApiResponse<IReadOnlyList<PermissionResponseDto>>> GetAllPermissionsAsync(CancellationToken ct = default)
    {
        var permissions = await _repo.GetAllPermissionsAsync(ct);
        return ApiResponse<IReadOnlyList<PermissionResponseDto>>.Success(permissions, "Permissions retrieved successfully.");
    }

    public async Task<ApiResponse<IReadOnlyList<GroupedPermissionsResponseDto>>> GetGroupedPermissionsAsync(CancellationToken ct = default)
    {
        var permissions = await _repo.GetAllPermissionsAsync(ct);

        var grouped = permissions
            .GroupBy(p => p.Module)
            .Select(g => new GroupedPermissionsResponseDto
            {
                Module = g.Key,
                Permissions = g.ToList()
            })
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

        var normalizedCode = requestDto.PermissionCode.Trim();

        var exists = await _repo.PermissionExistsByCodeAsync(normalizedCode, ct);
        if (exists)
        {
            return ApiResponse<Guid>.Failure(
                "Permission code already exists.",
                PermissionErrors.PermissionAlreadyExists);
        }

        var newPermissionId = Guid.NewGuid();

        await _uow.ExecuteInTransactionAsync(async () =>
        {
            await _repo.CreatePermissionAsync(
                newPermissionId,
                normalizedCode,
                requestDto.PermissionName.Trim(),
                requestDto.Module.Trim(),
                requestDto.Description,
                ct);
        }, ct);

        _logger.LogInformation("Permission '{PermissionCode}' (ID: {PermissionId}) successfully created.", normalizedCode, newPermissionId);
        return ApiResponse<Guid>.Success(newPermissionId, "Permission created successfully.");
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

        await _repo.UpdatePermissionAsync(
            permissionId,
            requestDto.PermissionName.Trim(),
            requestDto.Module.Trim(),
            requestDto.Description,
            requestDto.IsActive,
            ct);

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

        await _uow.ExecuteInTransactionAsync(async () =>
        {
            await _repo.DeletePermissionAsync(permissionId, ct);
        }, ct);

        _logger.LogInformation("Permission '{PermissionId}' deleted successfully.", permissionId);
        return ApiResponse<bool>.Success(true, "Permission deleted successfully.");
    }
}