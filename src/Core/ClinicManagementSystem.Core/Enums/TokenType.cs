namespace ClinicManagementSystem.Domain.Enums;

public enum TokenType
{
    EmailVerification = 1,
    PasswordReset = 2,
    PhoneVerification = 3,
    TwoFactorAuth = 4,
    MagicLinkLogin = 5,
    AccountUnlock = 6,
    AccountDeletionConfirm = 7
}
