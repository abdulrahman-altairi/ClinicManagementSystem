using ClinicManagementSystem.Application.Common.Models;
using ClinicManagementSystem.Domain.Enums;

namespace ClinicManagementSystem.Application.Common.Errors;

public static class AuthErrors
{
    // ── Validation Errors ───────────────────────────────────────────────────────

    public static readonly ErrorModel RequiredFieldMissing = ErrorModel.Global(
        "Required field is missing.",
        AuthErrorCode.RequiredFieldMissing.ToString());

    // ── Registration Errors ─────────────────────────────────────────────────────

    public static readonly ErrorModel EmailAlreadyTaken = ErrorModel.Global(
        "The specified email address is already registered.",
        AuthErrorCode.EmailAlreadyTaken.ToString());

    public static readonly ErrorModel UsernameAlreadyTaken = ErrorModel.Global(
        "The specified username is already in use.",
        AuthErrorCode.UsernameAlreadyTaken.ToString());

    // ── Authentication Errors ───────────────────────────────────────────────────

    public static readonly ErrorModel InvalidCredentials = ErrorModel.Global(
        "Invalid email or password.",
        AuthErrorCode.InvalidCredentials.ToString());

    public static readonly ErrorModel AccountInactive = ErrorModel.Global(
        "Your account is inactive. Please contact support.",
        AuthErrorCode.AccountInactive.ToString());

    public static readonly ErrorModel AccountLocked = ErrorModel.Global(
        "Your account has been locked due to multiple failed login attempts.",
        AuthErrorCode.AccountLocked.ToString());

    // ── Token / Session Errors ──────────────────────────────────────────────────

    public static readonly ErrorModel AccessTokenExpired = ErrorModel.Global(
        "Access token has expired.",
        AuthErrorCode.AccessTokenExpired.ToString());

    public static readonly ErrorModel InvalidRefreshToken = ErrorModel.Global(
        "Invalid or expired refresh token.",
        AuthErrorCode.InvalidRefreshToken.ToString());

    // ── Authorization Errors ────────────────────────────────────────────────────

    public static readonly ErrorModel InsufficientPermissions = ErrorModel.Global(
        "You do not have the required permissions to perform this action.",
        AuthErrorCode.InsufficientPermissions.ToString());

    public static readonly ErrorModel RoleNotFound = ErrorModel.Global(
        "The requested role was not found.",
        AuthErrorCode.RoleNotFound.ToString());

    // ── Two-Factor Authentication Errors ─────────────────────────────────────

    public static readonly ErrorModel RequiresTwoFactor = ErrorModel.Global(
        "2FA OTP code is required to complete login.",
        AuthErrorCode.RequiresTwoFactor.ToString());

    public static readonly ErrorModel InvalidTwoFactorCode = ErrorModel.Global(
        "The provided 2FA OTP code is invalid or has expired.",
        AuthErrorCode.InvalidTwoFactorCode.ToString());

    // ── Token / Session Errors ──────────────────────────────────────────────────

    public static readonly ErrorModel InvalidAccessToken = ErrorModel.Global(
        "Token structure is invalid.",
        AuthErrorCode.InvalidAccessToken.ToString());

    public static readonly ErrorModel SessionNotFound = ErrorModel.Global(
        "The requested session was not found.",
        AuthErrorCode.SessionNotFound.ToString());

    // ── Password Errors ─────────────────────────────────────────────────────────

    public static readonly ErrorModel WrongCurrentPassword = ErrorModel.Global(
        "The current password provided is incorrect.",
        AuthErrorCode.WrongCurrentPassword.ToString());

    public static readonly ErrorModel PasswordReused = ErrorModel.Global(
        "You cannot reuse any of your recent passwords.",
        AuthErrorCode.PasswordReused.ToString());
}