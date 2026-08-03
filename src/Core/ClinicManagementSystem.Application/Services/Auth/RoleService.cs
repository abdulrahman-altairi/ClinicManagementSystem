using ClinicManagementSystem.Application.Common.Errors;
using ClinicManagementSystem.Application.Common.Interfaces;
using ClinicManagementSystem.Application.Common.Models;
using ClinicManagementSystem.Application.DTOs.Auth.Role;
using ClinicManagementSystem.Application.Interfaces.Repositories;
using ClinicManagementSystem.Application.Interfaces.Services.Auth;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace ClinicManagementSystem.Application.Services.Auth;

public sealed class RoleService : IRoleService
{

    private readonly IIdentityRepository _repo;
    private readonly ILogger<RoleService> _logger;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<CreateRoleRequestDto> _createRoleValidator;
    private readonly IValidator<UpdateRoleRequestDto> _updateRoleValidator;

    public RoleService
        (
            IIdentityRepository repo, 
            ILogger<RoleService> logger,
            IUnitOfWork uow,
            IValidator<CreateRoleRequestDto> createRoleValidator,
            IValidator<UpdateRoleRequestDto> updateRoleValidator
        )
    {
        _repo = repo;
        _logger = logger;
        _uow = uow;
        _createRoleValidator = createRoleValidator;
        _updateRoleValidator = updateRoleValidator;
    }

    public async Task<ApiResponse<IReadOnlyList<RoleResponseDto>>> GetAllRolesAsync(CancellationToken ct = default)
    {
        var roles = await _repo.GetAllRolesAsync(ct);
        return ApiResponse<IReadOnlyList<RoleResponseDto>>.Success(roles, "Roles retrieved successfully.");
    }

    public async Task<ApiResponse<RoleResponseDto>> GetRoleByIdAsync(Guid roleId, CancellationToken ct = default)
    {
        if (roleId == Guid.Empty)
        {
            return ApiResponse<RoleResponseDto>.Failure(
                "Invalid role identifier.",
                RoleErrors.InvalidRoleId);
        }

        var role = await _repo.GetRoleByIdAsync(roleId, ct);
        if (role is null)
        {
            return ApiResponse<RoleResponseDto>.Failure(
                "Role not found.",
                RoleErrors.RoleNotFound);
        }

        return ApiResponse<RoleResponseDto>.Success(role, "Role details retrieved successfully.");
    }

    public async Task<ApiResponse<Guid>> CreateRoleAsync(CreateRoleRequestDto requestDto, CancellationToken ct = default)
    {
        var validation = await _createRoleValidator.ValidateAsync(requestDto, ct);
        if (!validation.IsValid)
        {
            return ApiResponse<Guid>.Failure(
                "Role creation payload validation failed.",
                validation.Errors.Select(e => new ErrorModel(e.PropertyName, e.ErrorMessage, e.ErrorCode)).ToList());
        }

        var normalizedName = requestDto.RoleName.Trim().ToUpperInvariant();

        var exists = await _repo.RoleExistsByNameAsync(normalizedName, ct);
        if (exists)
        {
            return ApiResponse<Guid>.Failure(
                "Role already exists.",
                RoleErrors.RoleAlreadyExists);
        }

        var newRoleId = Guid.NewGuid();

        await _uow.ExecuteInTransactionAsync(async () =>
        {
            await _repo.CreateRoleAsync(newRoleId, requestDto.RoleName.Trim(), normalizedName, requestDto.Description, ct);
        }, ct);

        _logger.LogInformation("Role '{RoleName}' (ID: {RoleId}) successfully created.", requestDto.RoleName, newRoleId);
        return ApiResponse<Guid>.Success(newRoleId, "Role created successfully.");
    }

    public async Task<ApiResponse<bool>> UpdateRoleAsync(Guid roleId, UpdateRoleRequestDto requestDto, CancellationToken ct = default)
    {
        if (roleId == Guid.Empty)
        {
            return ApiResponse<bool>.Failure(
                "Invalid role identifier.",
                RoleErrors.InvalidRoleId);
        }

        var validation = await _updateRoleValidator.ValidateAsync(requestDto, ct);
        if (!validation.IsValid)
        {
            return ApiResponse<bool>.Failure(
                "Role update payload validation failed.",
                validation.Errors.Select(e => new ErrorModel(e.PropertyName, e.ErrorMessage, e.ErrorCode)).ToList());
        }

        var existingRole = await _repo.GetRoleByIdAsync(roleId, ct);
        if (existingRole is null)
        {
            return ApiResponse<bool>.Failure(
                "Role not found.",
                RoleErrors.RoleNotFound);
        }

        if (existingRole.IsSystemRole && !existingRole.RoleName.Equals(requestDto.RoleName.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return ApiResponse<bool>.Failure(
                "Operation prohibited.",
                RoleErrors.SystemRoleProtected);
        }

        var normalizedName = requestDto.RoleName.Trim().ToUpperInvariant();

        if (!existingRole.RoleName.Equals(requestDto.RoleName.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            var exists = await _repo.RoleExistsByNameAsync(normalizedName, ct);
            if (exists)
            {
                return ApiResponse<bool>.Failure(
                    "Role already exists.",
                    RoleErrors.RoleAlreadyExists);
            }
        }

        await _repo.UpdateRoleAsync(roleId, requestDto.RoleName.Trim(), normalizedName, requestDto.Description, ct);

        _logger.LogInformation("Role '{RoleId}' updated successfully.", roleId);
        return ApiResponse<bool>.Success(true, "Role details updated successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteRoleAsync(Guid roleId, CancellationToken ct = default)
    {
        if (roleId == Guid.Empty)
        {
            return ApiResponse<bool>.Failure(
                "Invalid role identifier.",
                RoleErrors.InvalidRoleId);
        }

        var role = await _repo.GetRoleByIdAsync(roleId, ct);
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
                RoleErrors.SystemRoleProtected);
        }

        var assignedUserCount = await _repo.GetAssignedUserCountForRoleAsync(roleId, ct);
        if (assignedUserCount > 0)
        {
            return ApiResponse<bool>.Failure(
                "Role deletion blocked.",
                RoleErrors.RoleInUse);
        }

        await _uow.ExecuteInTransactionAsync(async () =>
        {
            await _repo.DeleteRoleAsync(roleId, ct);
        }, ct);

        _logger.LogInformation("Role '{RoleId}' deleted successfully.", roleId);
        return ApiResponse<bool>.Success(true, "Role deleted successfully.");
    }
}
