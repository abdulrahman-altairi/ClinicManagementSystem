using ClinicManagementSystem.Application.Common.Models;
using ClinicManagementSystem.Application.DTOs.Auth.ResetPassword;

namespace ClinicManagementSystem.Application.Interfaces.Services.Auth;

public interface IPasswordResetService
{
    Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequestDto request, CancellationToken ct = default);
    Task<ApiResponse<bool>> ValidateResetTokenAsync(string token, CancellationToken ct = default);
    Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken ct = default);
    Task<ApiResponse<bool>> ConfirmEmailAsync(ConfirmEmailRequestDto request, CancellationToken ct = default);
    Task<ApiResponse<bool>> ResendEmailConfirmationAsync(string email, CancellationToken ct = default);
}