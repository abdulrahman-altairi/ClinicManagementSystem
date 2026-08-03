using ClinicManagementSystem.Application.DTOs.Auth.UserRole;
using ClinicManagementSystem.Domain.Enums;
using FluentValidation;

namespace ClinicManagementSystem.Application.Validators.Auth.UserRole;

public sealed class AddRoleToUserRequestValidator : AbstractValidator<AddRoleToUserRequestDto>
{
    public AddRoleToUserRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.RoleId)
            .NotEmpty()
            .WithMessage("Role ID is required.");

        RuleFor(x => x)
            .Must(x => !x.ValidTo.HasValue || !x.ValidFrom.HasValue || x.ValidTo > x.ValidFrom)
            .WithMessage("'ValidTo' date must be greater than 'ValidFrom' date.");
    }
}