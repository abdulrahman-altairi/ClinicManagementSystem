using ClinicManagementSystem.Application.DTOs.Auth.Users;
using FluentValidation;

namespace ClinicManagementSystem.Application.Validators.Auth;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequestDto>
{
    LoginRequestValidator()
    {
        RuleFor(x => x.Identifier)
            .NotEmpty().WithMessage("Email or username is required.")
            .MaximumLength(256)
            .WithMessage("Email or username can't be longer than 256 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MaximumLength(128)
            .WithMessage("Password can't be longer than 128 characters.");
    }
}
