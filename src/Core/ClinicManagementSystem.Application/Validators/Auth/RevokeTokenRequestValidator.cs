using FluentValidation;
using ClinicManagementSystem.Application.DTOs.Auth.Sessions;

namespace ClinicManagementSystem.Application.Validators.Auth;

public sealed class RevokeTokenRequestValidator : AbstractValidator<RevokeTokenRequestDto>
{
    public RevokeTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("RefreshToken can't be empty");
    }
}