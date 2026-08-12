using ClinicManagementSystem.Application.Common.Errors;
using ClinicManagementSystem.Application.Common.Interfaces;
using ClinicManagementSystem.Application.Common.Models;
using ClinicManagementSystem.Application.DTOs.Auth.UserRole;
using ClinicManagementSystem.Application.Interfaces.Repositories;
using ClinicManagementSystem.Application.Interfaces.Services.Auth;
using ClinicManagementSystem.Domain.Entities.Auth;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace ClinicManagementSystem.Application.Services.Auth;

public sealed class UserRoleService : IUserRoleService
{
    private readonly IIdentityRepository _repo;
    private readonly ILogger<UserRoleService> _logger;
    private readonly IUnitOfWork _uow;
    private readonly IDateTimeProvider _date;
    private readonly IValidator<AssignRolesToUserRequestDto> _assignRolesValidator;
    private readonly IValidator<AddRoleToUserRequestDto> _addRoleValidator;

    public UserRoleService(
        IIdentityRepository repo,
        ILogger<UserRoleService> logger,
        IUnitOfWork uow,
        IDateTimeProvider date,
        IValidator<AssignRolesToUserRequestDto> assignRolesValidator,
        IValidator<AddRoleToUserRequestDto> addRoleValidator)
    {
        _repo = repo;
        _logger = logger;
        _uow = uow;
        _date = date;
        _assignRolesValidator = assignRolesValidator;
        _addRoleValidator = addRoleValidator;
    }

    public async Task<ApiResponse<UserRolesDetailsResponseDto>> GetUserRolesAsync(Guid userId, CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
        {
            return ApiResponse<UserRolesDetailsResponseDto>.Failure(
                "Invalid user identifier.",
                UserErrors.InvalidUserId);
        }

        var user = await _repo.GetUserByIdAsync(userId, ct);
        if (user is null)
        {
            return ApiResponse<UserRolesDetailsResponseDto>.Failure(
                "User not found.",
                UserErrors.UserNotFound);   
        }

        var roles = await _repo.GetRolesByUserIdAsync(userId, ct);

        var response = new UserRolesDetailsResponseDto
        {
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            AssignedRoles = roles
        };

        return ApiResponse<UserRolesDetailsResponseDto>.Success(response, "User roles retrieved successfully.");
    }

    public async Task<ApiResponse<bool>> AssignRolesToUserAsync(AssignRolesToUserRequestDto requestDto, Guid? assignedBy = null, CancellationToken ct = default)
    {
        var validation = await _assignRolesValidator.ValidateAsync(requestDto, ct);
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

        var distinctRoleIds = requestDto.Roles.Select(r => r.RoleId).Distinct().ToList();
        if (distinctRoleIds.Any())
        {
            var existingRoles = await _repo.GetRolesByIdsAsync(distinctRoleIds, ct);

            if (existingRoles.Count != distinctRoleIds.Count)
            {
                return ApiResponse<bool>.Failure(
                    "One or more role IDs are invalid or non-existent.",
                    RoleErrors.RoleNotFound);   
            }
        }

        var userRolesToAssign = requestDto.Roles.Select(r => new UserRole
        {
            Id = Guid.NewGuid(), 
            UserId = requestDto.UserId,
            RoleId = r.RoleId,
            ValidFrom = r.ValidFrom ?? _date.UtcNow,
            ValidTo = r.ValidTo,
            AssignedBy = assignedBy
        }).ToList();

        await _uow.ExecuteInTransactionAsync(async () =>
        {
            await _repo.RemoveAllRolesFromUserAsync(requestDto.UserId, ct);
            await _repo.AssignRolesToUserAsync(requestDto.UserId, userRolesToAssign, ct);
        }, ct);

        _logger.LogInformation("Successfully assigned {Count} roles to User '{UserId}'.", distinctRoleIds.Count, requestDto.UserId);
        return ApiResponse<bool>.Success(true, "Roles assigned to user successfully.");
    }

    public async Task<ApiResponse<Guid>> AddRoleToUserAsync(AddRoleToUserRequestDto requestDto, Guid? assignedBy = null, CancellationToken ct = default)
    {
        var validation = await _addRoleValidator.ValidateAsync(requestDto, ct);
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

        var role = await _repo.GetRoleByIdAsync(requestDto.RoleId, ct);
        if (role is null)
        {
            return ApiResponse<Guid>.Failure(
                "Role not found.",
                RoleErrors.RoleNotFound);   
        }

        var exists = await _repo.UserHasRoleAsync(requestDto.UserId, requestDto.RoleId, ct);
        if (exists)
        {
            return ApiResponse<Guid>.Failure(
                "Role already assigned to user.",
                UserRoleErrors.UserRoleAlreadyExists);   
        }


        var userRole = new UserRole
        {
            Id = Guid.NewGuid(),
            UserId = requestDto.UserId,
            RoleId = requestDto.RoleId,
            ValidFrom = requestDto.ValidFrom ?? _date.UtcNow,
            ValidTo = requestDto.ValidTo,
            AssignedBy = assignedBy
        };

        await _repo.AddUserRoleAsync(userRole, ct);

        _logger.LogInformation("Role '{RoleId}' added to User '{UserId}'.", requestDto.RoleId, requestDto.UserId);
        return ApiResponse<Guid>.Success(userRole.Id, "Role added to user successfully.");
    }

    public async Task<ApiResponse<bool>> RemoveRoleFromUserAsync(Guid userId, Guid roleId, CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
        {
            return ApiResponse<bool>.Failure(
                "Invalid user identifier.",
                UserErrors.InvalidUserId);
        }

        if (roleId == Guid.Empty)
        {
            return ApiResponse<bool>.Failure(
                "Invalid role identifier.",
                RoleErrors.InvalidRoleId);
        }

        var user = await _repo.GetUserByIdAsync(userId, ct);
        if (user is null)
        {
            return ApiResponse<bool>.Failure(
                "User not found.",
                UserErrors.UserNotFound);
        }

        var exists = await _repo.UserHasRoleAsync(userId, roleId, ct);
        if (!exists)
        {
            return ApiResponse<bool>.Failure(
                "Role mapping not found for user.",
                UserRoleErrors.UserRoleNotFound);
        }

        await _repo.RemoveUserRoleAsync(userId, roleId, ct);

        _logger.LogInformation("Role '{RoleId}' removed from User '{UserId}'.", roleId, userId);
        return ApiResponse<bool>.Success(true, "Role removed from user successfully.");
    }

    public async Task<ApiResponse<bool>> UserHasActiveRoleAsync(Guid userId, string roleCode, CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
        {
            return ApiResponse<bool>.Failure(
                "Invalid role code format.",
                RoleErrors.InvalidRoleCode);
        }

        if (string.IsNullOrWhiteSpace(roleCode))
        {
            return ApiResponse<bool>.Failure(
                "Invalid role code.",
                RoleErrors.RoleNotFound);
        }

        var hasRole = await _repo.UserHasActiveRoleByCodeAsync(userId, roleCode.Trim(), ct);
        return ApiResponse<bool>.Success(hasRole, "Role status checked successfully.");
    }
}