using ClinicManagementSystem.Application.DTOs.Auth.UserPermissions;
using FluentValidation;

namespace ClinicManagementSystem.Application.Validators.Auth;

public sealed class AddUserPermissionOverrideRequestValidator : AbstractValidator<AddUserPermissionOverrideRequestDto>
{
    public AddUserPermissionOverrideRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.PermissionId)
            .NotEmpty()
            .WithMessage("Permission ID is required.");

        RuleFor(x => x.GrantType)
            .IsInEnum()
            .WithMessage("Invalid grant type specified.");

        RuleFor(x => x.Reason)
            .MaximumLength(500)
            .WithMessage("Reason must not exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Reason));

        RuleFor(x => x)
            .Must(x => !x.ValidTo.HasValue || !x.ValidFrom.HasValue || x.ValidTo > x.ValidFrom)
            .WithMessage("'ValidTo' date must be greater than 'ValidFrom' date.");
    }
}