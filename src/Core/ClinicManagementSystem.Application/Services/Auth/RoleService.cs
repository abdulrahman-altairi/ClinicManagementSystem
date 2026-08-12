using ClinicManagementSystem.Application.Common.Errors;
using ClinicManagementSystem.Application.Common.Interfaces;
using ClinicManagementSystem.Application.Common.Models;
using ClinicManagementSystem.Application.DTOs.Auth.Role;
using ClinicManagementSystem.Application.Interfaces.Repositories;
using ClinicManagementSystem.Application.Interfaces.Services.Auth;
using ClinicManagementSystem.Domain.Entities.Auth;
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

        if (roles is null || !roles.Any())
        {
            return ApiResponse<IReadOnlyList<RoleResponseDto>>.Success(
                Array.Empty<RoleResponseDto>(), 
                "No roles found.");
        }

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

        var role = new Role
        {
            Id = Guid.NewGuid(),
            RoleName = requestDto.RoleName.Trim(),
            NormalizedName = normalizedName,
            Description = requestDto.Description
        };

        await _repo.CreateRoleAsync(role, ct);

        _logger.LogInformation("Role '{RoleName}' (ID: {RoleId}) successfully created.", role.RoleName, role.Id);
        return ApiResponse<Guid>.Success(role.Id, "Role created successfully.");
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

        var role = new Role
        {
            Id = roleId,
            RoleName = requestDto.RoleName.Trim(),
            NormalizedName = normalizedName,
            Description = requestDto.Description
        };

        await _repo.UpdateRoleAsync(role, ct);

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


        await _repo.DeleteRoleAsync(roleId, ct);

        _logger.LogInformation("Role '{RoleId}' deleted successfully.", roleId);
        return ApiResponse<bool>.Success(true, "Role deleted successfully.");
    }

    public async Task<ApiResponse<PaginatedList<RoleResponseDto>>> SearchRolesAsync(RoleSearchFilter filter, CancellationToken ct = default)
    {
        var (roles, totalCount) = await _repo.SearchRolesAsync(filter, ct);

        if (roles is null || !roles.Any())
        {
            var emptyList = new PaginatedList<RoleResponseDto>(Array.Empty<RoleResponseDto>(), 0, filter.PageNumber, filter.PageSize);
            return ApiResponse<PaginatedList<RoleResponseDto>>.Success(emptyList, "No roles matched the search criteria.");
        }

        var rolesDto = roles.Select(r => new RoleResponseDto
        {
            RoleId = r.Id,
            RoleName = r.RoleName,
            Description = r.Description,
            IsSystemRole = r.IsSystemRole,
            IsActive = r.IsActive,
            CreatedAt = r.CreatedAt
        }).ToList();

        var paginatedResult = new PaginatedList<RoleResponseDto>(rolesDto, totalCount, filter.PageNumber, filter.PageSize);

        return ApiResponse<PaginatedList<RoleResponseDto>>.Success(paginatedResult, "Roles search completed successfully.");
    }

    public async Task<ApiResponse<List<RoleResponseDto>>> GetSystemRolesAsync(CancellationToken ct = default)
    {
        var roles = await _repo.GetSystemRolesAsync(ct);

        if (roles is null || !roles.Any())
        {
            return ApiResponse<List<RoleResponseDto>>.Success(new List<RoleResponseDto>(), "No system roles found.");
        }

        var rolesDto = roles.Select(r => new RoleResponseDto
        {
            RoleId = r.Id,
            RoleName = r.RoleName,
            Description = r.Description,
            IsSystemRole = r.IsSystemRole,
            IsActive = r.IsActive,
            CreatedAt = r.CreatedAt
        }).ToList();

        return ApiResponse<List<RoleResponseDto>>.Success(rolesDto, "System roles retrieved successfully.");
    }

   public async Task<ApiResponse<bool>> ToggleRoleStatusAsync(Guid roleId, bool isActive, CancellationToken ct = default)
    {
        if (roleId == Guid.Empty)
        {
            return ApiResponse<bool>.Failure(
                "Invalid role identifier.",
                RoleErrors.InvalidRoleId);
        }

        var existingRole = await _repo.GetRoleByIdAsync(roleId, ct);
        if (existingRole is null)
        {
            return ApiResponse<bool>.Failure(
                "Role not found.",
                RoleErrors.RoleNotFound);
        }

        if (existingRole.IsSystemRole && !isActive)
        {
            return ApiResponse<bool>.Failure(
                "Operation prohibited. System critical roles cannot be deactivated.",
                RoleErrors.SystemRoleProtected);
        }

        var updatedRole = new Role
        {
            Id = existingRole.RoleId,
            RoleName = existingRole.RoleName,
            NormalizedName = existingRole.RoleName.Trim().ToUpperInvariant(),
            Description = existingRole.Description,
            IsActive = isActive,
            IsSystemRole = existingRole.IsSystemRole,
            CreatedAt = existingRole.CreatedAt
        };

        await _uow.ExecuteInTransactionAsync(async () =>
        {
            await _repo.UpdateRoleAsync(updatedRole, ct);
        }, ct);

        _logger.LogInformation("Role '{RoleId}' status successfully toggled to Active = {IsActive}.", roleId, isActive);
        return ApiResponse<bool>.Success(true, $"Role status successfully updated to {(isActive ? "Active" : "Inactive")}.");
    }
}
