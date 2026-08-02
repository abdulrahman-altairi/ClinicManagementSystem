using ClinicManagementSystem.Application.DTOs.Auth.Users;
using FluentValidation;

namespace ClinicManagementSystem.Application.Validators.Auth;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequestDto>
{
    LoginRequestValidator()
    {
        RuleFor(x => x.Identifier)
            .NotEmpty().WithMessage("Email or username is required.")
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MaximumLength(128);
    }
}
