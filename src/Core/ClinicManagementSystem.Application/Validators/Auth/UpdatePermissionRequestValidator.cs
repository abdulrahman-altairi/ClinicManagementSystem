using ClinicManagementSystem.Application.DTOs.Auth.Permissions;
using ClinicManagementSystem.Domain.Enums;
using FluentValidation;

namespace ClinicManagementSystem.Application.Validators.Auth;

public sealed class UpdatePermissionRequestValidator : AbstractValidator<UpdatePermissionRequestDto>
{
    public UpdatePermissionRequestValidator()
    {
        RuleFor(x => x.PermissionName)
            .NotEmpty()
            .WithMessage("Permission name is required.")
            .MaximumLength(200)
            .WithMessage("Permission name must not exceed 200 characters.");

        RuleFor(x => x.Module)
            .NotEmpty()
            .WithMessage("Module name is required.")
            .MaximumLength(100)
            .WithMessage("Module name must not exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description must not exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}