using ClinicManagementSystem.Application.DTOs.Auth.Role;
using FluentValidation;

namespace ClinicManagementSystem.Application.Validators.Auth.Role;

public sealed class UpdateRoleRequestValidator : AbstractValidator<UpdateRoleRequestDto>
{
    public UpdateRoleRequestValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty()
            .WithMessage("Role ID is required.");

        RuleFor(x => x.RoleName)
            .NotEmpty()
            .WithMessage("Role name is required.")
            .MaximumLength(50)
            .WithMessage("Role name must not exceed 50 characters.")
            .Matches(@"^[a-zA-Z0-9_\s-]+$")
            .WithMessage("Role name can only contain letters, numbers, spaces, underscores, or hyphens.");

        RuleFor(x => x.Description)
            .MaximumLength(250)
            .WithMessage("Description must not exceed 250 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}