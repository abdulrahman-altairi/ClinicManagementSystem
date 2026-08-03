using ClinicManagementSystem.Application.DTOs.Auth.AssignPermissions;
using ClinicManagementSystem.Domain.Enums;
using FluentValidation;

namespace ClinicManagementSystem.Application.Validators.Auth;

public sealed class AssignPermissionsToRoleRequestValidator : AbstractValidator<AssignPermissionsToRoleRequestDto>
{
    public AssignPermissionsToRoleRequestValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty()
            .WithMessage("Role ID is required.");
        RuleFor(x => x.PermissionIds)
            .NotNull()
            .WithMessage("Permission IDs list cannot be null.")
            .Must(x => x != null && x.Count > 0)
            .WithMessage("At least one permission ID must be provided.");
    }
}