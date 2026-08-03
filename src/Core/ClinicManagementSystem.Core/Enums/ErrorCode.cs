namespace ClinicManagementSystem.Domain.Enums;

public enum ErrorCode
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
    RoleAlreadyExists = 6003,
    InvalidRoleId = 6004,
    SystemRoleProtected = 6005,
    RoleInUse = 6006,
    InvalidPermissionId = 6007,
    PermissionAlreadyExists = 6008,
    InvalidPermissionCodeFormat = 6009,
    SystemPermissionProtected = 6010,
    PermissionInUse = 6011,
    RolePermissionAlreadyExists = 6012,
    RolePermissionNotFound = 6013,
    CannotModifySystemRolePermissions = 6014,
    EmptyPermissionList = 6015,
    UserRoleAlreadyExists = 6016,
    UserRoleNotFound = 6017,
    InvalidRoleValidityPeriod = 6018,
    CannotRemoveLastAdminRole = 6019,
    EmptyRoleList = 6020,
    UserPermissionAlreadyExists = 6021,
    UserPermissionNotFound = 6022,
    InvalidGrantType = 6023,
    InvalidOverrideValidityPeriod = 6024,
    EmptyOverrideList = 6025,

    // ── Two-Factor Authentication (2FA) ──────────────────────────────────────
    RequiresTwoFactor = 7000,
    InvalidTwoFactorCode = 7001,

    // ── Empty or Invalid ──────────────────────────────────────-─────────-──────
    InvalidUserId = 8000,
    UserNotFound = 8001,

}
