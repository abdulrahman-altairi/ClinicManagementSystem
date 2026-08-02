using ClinicManagementSystem.Application.DTOs.Auth.Users;
using FluentValidation;

namespace ClinicManagementSystem.Application.Validators.Auth;

public sealed class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequestDto>
{
    ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
                .Empty().WithMessage("Current password is required.");

        RuleFor(x => x.NewPassword)
            .Empty().WithMessage("New password is required.")
            .MinimumLength(8).WithMessage("Password must be greater than 8 charachter.")
            .MaximumLength(128).WithMessage("Password must be less than 120 charachter.")
            .Matches("[A-Z]").WithMessage("Must contain an uppercase letter.")
            .Matches("[a-z]").WithMessage("Must contain a lowercase letter.")
            .Matches("[0-9]").WithMessage("Must contain a digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Must contain a special character.")
            .NotEqual(x => x.CurrentPassword).WithMessage("New password must differ from the current password.");

        RuleFor(x => x.ConfirmPassword)
        .Equal(x => x.NewPassword).WithMessage("Passwords do not match.");
    }
}
