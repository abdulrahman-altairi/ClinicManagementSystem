using ClinicManagementSystem.Application.DTOs.Auth.Users;
using FluentValidation;

namespace ClinicManagementSystem.Application.Validators.Auth;

public sealed class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequestDto>
{
    public RegisterUserRequestValidator()
    {
        RuleFor(x => x.Username)
        .NotEmpty().WithMessage("Username is required.")
        .MinimumLength(3).WithMessage("Username must be at least 3 characters.")
        .MaximumLength(100).WithMessage("Username cannot exceed 100 characters.")
        .Matches("^[a-zA-Z0-9._-]+$").WithMessage("Username may only contain letters, digits, dots, underscores, or hyphens.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .MaximumLength(128)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100);

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{6,14}$").WithMessage("Phone number format is invalid.")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}
