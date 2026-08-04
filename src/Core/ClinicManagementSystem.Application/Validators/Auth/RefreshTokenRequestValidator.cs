using FluentValidation;
using ClinicManagementSystem.Application.DTOs.Auth.Sessions;

namespace ClinicManagementSystem.Application.Validators.Auth;

public sealed class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequestDto>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.AccessToken)
            .NotEmpty().WithMessage("AccessToken can't be empty");

        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("RefreshToken can't be empty");
    }
}