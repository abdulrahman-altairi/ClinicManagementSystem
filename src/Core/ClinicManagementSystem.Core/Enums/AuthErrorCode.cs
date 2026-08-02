namespace ClinicManagementSystem.Domain.Enums;

public enum AuthErrorCode
{
    // ── Generic ───────────────────────────────────────────────────────────────
    Unknown = 0,

    // ── Validation ────────────────────────────────────────────────────────────
    ValidationFailed = 1000,
    InvalidEmailFormat = 1001,
    InvalidPasswordFormat = 1002,
    RequiredFieldMissing = 1003,

    // ── Registration ─────────────────────────────────────────────────────────
    EmailAlreadyTaken = 2000,
    UsernameAlreadyTaken = 2001,

    // ── Authentication ────────────────────────────────────────────────────────
    InvalidCredentials = 3000,
    AccountInactive = 3001,
    AccountLocked = 3002,
    EmailNotVerified = 3003,

    // ── Token / Session ───────────────────────────────────────────────────────
    InvalidAccessToken = 4000,
    AccessTokenExpired = 4001,
    InvalidRefreshToken = 4002,
    RefreshTokenExpired = 4003,
    RefreshTokenRevoked = 4004,
    SessionNotFound = 4005,

    // ── Password ──────────────────────────────────────────────────────────────
    PasswordMismatch = 5000,
    WrongCurrentPassword = 5001,
    PasswordReused = 5002,
    PasswordTooWeak = 5003,

    // ── Authorization ─────────────────────────────────────────────────────────
    InsufficientPermissions = 6000,
    RoleNotFound = 6001,
    PermissionNotFound = 6002,

    // ── Two-Factor Authentication (2FA) ──────────────────────────────────────
    RequiresTwoFactor = 7000,
    InvalidTwoFactorCode = 7001,
}
