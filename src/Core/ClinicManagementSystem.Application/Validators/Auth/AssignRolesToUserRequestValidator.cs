using ClinicManagementSystem.Application.DTOs.Auth.UserRole;
using ClinicManagementSystem.Domain.Enums;
using FluentValidation;

namespace ClinicManagementSystem.Application.Validators.Auth.UserRole;

public sealed class AssignRolesToUserRequestValidator : AbstractValidator<AssignRolesToUserRequestDto>
{
    public AssignRolesToUserRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.Roles)
            .NotNull()
            .WithMessage("Roles list cannot be null.")
            .Must(x => x != null && x.Count > 0)
            .WithMessage("At least one role assignment must be provided.");

        RuleForEach(x => x.Roles).ChildRules(role =>
        {
            role.RuleFor(r => r.RoleId)
                .NotEmpty()
                .WithMessage("Role ID is required.");

            role.RuleFor(r => r)
                .Must(r => !r.ValidTo.HasValue || !r.ValidFrom.HasValue || r.ValidTo > r.ValidFrom)
                .WithMessage("'ValidTo' date must be greater than 'ValidFrom' date.");
        });
    }
}