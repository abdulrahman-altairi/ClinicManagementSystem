namespace ClinicManagementSystem.Application.Common.Options;

public class AuthOptions
{
    public const string SectionName = "AuthSettings";

    public int AccessTokenExpiryMinutes { get; init; } = 15;

    public int RefreshTokenExpiryDays { get; init; } = 7;

    public int MaxFailedAttempts { get; init; } = 5;

    public TimeSpan LockoutDuration { get; init; } = TimeSpan.FromMinutes(15);

    public int PasswordHistoryDepth { get; init; } = 5;
}
