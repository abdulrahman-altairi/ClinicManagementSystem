using ClinicManagementSystem.Application.DTOs.Auth.ResetPassword;
using FluentValidation;

namespace ClinicManagementSystem.Application.Validators.Auth;
public class ForgotPasswordRequestDtoValidator : AbstractValidator<ForgotPasswordRequestDto>
{
    public ForgotPasswordRequestDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(150).WithMessage("Email must not exceed 150 characters.");
    }
}