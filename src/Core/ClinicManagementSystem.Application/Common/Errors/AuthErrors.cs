using ClinicManagementSystem.Application.Common.Models;
using ClinicManagementSystem.Domain.Enums;

namespace ClinicManagementSystem.Application.Common.Errors;

public static class AuthErrors
{
    // ── Validation Errors ───────────────────────────────────────────────────────

    public static readonly ErrorModel RequiredFieldMissing = ErrorModel.Global(
        "Required field is missing.",
        ErrorCode.RequiredFieldMissing.ToString());

    // ── Registration Errors ─────────────────────────────────────────────────────

    public static readonly ErrorModel EmailAlreadyTaken = ErrorModel.Global(
        "The specified email address is already registered.",
        ErrorCode.EmailAlreadyTaken.ToString());

    public static readonly ErrorModel UsernameAlreadyTaken = ErrorModel.Global(
        "The specified username is already in use.",
        ErrorCode.UsernameAlreadyTaken.ToString());

    // ── Authentication Errors ───────────────────────────────────────────────────

    public static readonly ErrorModel InvalidCredentials = ErrorModel.Global(
        "Invalid email or password.",
        ErrorCode.InvalidCredentials.ToString());

    public static readonly ErrorModel AccountInactive = ErrorModel.Global(
        "Your account is inactive. Please contact support.",
        ErrorCode.AccountInactive.ToString());

    public static readonly ErrorModel AccountLocked = ErrorModel.Global(
        "Your account has been locked due to multiple failed login attempts.",
        ErrorCode.AccountLocked.ToString());

    // ── Token / Session Errors ──────────────────────────────────────────────────

    public static readonly ErrorModel AccessTokenExpired = ErrorModel.Global(
        "Access token has expired.",
        ErrorCode.AccessTokenExpired.ToString());

    public static readonly ErrorModel InvalidRefreshToken = ErrorModel.Global(
        "Invalid or expired refresh token.",
        ErrorCode.InvalidRefreshToken.ToString());

    // ── Authorization Errors ────────────────────────────────────────────────────

    public static readonly ErrorModel InsufficientPermissions = ErrorModel.Global(
        "You do not have the required permissions to perform this action.",
        ErrorCode.InsufficientPermissions.ToString());

    public static readonly ErrorModel RoleNotFound = ErrorModel.Global(
        "The requested role was not found.",
        ErrorCode.RoleNotFound.ToString());

    // ── Two-Factor Authentication Errors ─────────────────────────────────────

    public static readonly ErrorModel RequiresTwoFactor = ErrorModel.Global(
        "2FA OTP code is required to complete login.",
        ErrorCode.RequiresTwoFactor.ToString());

    public static readonly ErrorModel InvalidTwoFactorCode = ErrorModel.Global(
        "The provided 2FA OTP code is invalid or has expired.",
        ErrorCode.InvalidTwoFactorCode.ToString());

    // ── Token / Session Errors ──────────────────────────────────────────────────

    public static readonly ErrorModel InvalidAccessToken = ErrorModel.Global(
        "Token structure is invalid.",
        ErrorCode.InvalidAccessToken.ToString());

    public static readonly ErrorModel SessionNotFound = ErrorModel.Global(
        "The requested session was not found.",
        ErrorCode.SessionNotFound.ToString());

    // ── Password Errors ─────────────────────────────────────────────────────────

    public static readonly ErrorModel WrongCurrentPassword = ErrorModel.Global(
        "The current password provided is incorrect.",
        ErrorCode.WrongCurrentPassword.ToString());

    public static readonly ErrorModel PasswordReused = ErrorModel.Global(
        "You cannot reuse any of your recent passwords.",
        ErrorCode.PasswordReused.ToString());
}