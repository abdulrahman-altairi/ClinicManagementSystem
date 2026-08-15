using System.Data;
using ClinicManagementSystem.Domain.Entities.Auth;

namespace ClinicManagementSystem.Infrastructure.Persistence.DataMappers;


public static class UserMapper
{
    public static ApplicationUser MapToApplicationUser(IDataReader r) => new()
    {
        Id                  = r.GetGuid(r.GetOrdinal("UserId")),
        Username            = r.GetString(r.GetOrdinal("Username")),
        NormalizedUsername  = r.GetString(r.GetOrdinal("NormalizedUsername")),
        Email               = r.GetString(r.GetOrdinal("Email")),
        NormalizedEmail     = r.GetString(r.GetOrdinal("NormalizedEmail")),
        PasswordHash        = r.GetString(r.GetOrdinal("PasswordHash")),
        PasswordSalt        = r.GetString(r.GetOrdinal("PasswordSalt")),
        FirstName           = r.GetString(r.GetOrdinal("FirstName")),
        LastName            = r.GetString(r.GetOrdinal("LastName")),
        PhoneNumber         = r.GetNullableString("PhoneNumber"),
        PhoneVerified       = r.GetBoolean(r.GetOrdinal("PhoneVerified")),
        EmailVerified       = r.GetBoolean(r.GetOrdinal("EmailVerified")),
        TwoFactorEnabled    = r.GetBoolean(r.GetOrdinal("TwoFactorEnabled")),
        TwoFactorSecret     = r.GetNullableString("TwoFactorSecret"),
        LockoutEnabled      = r.GetBoolean(r.GetOrdinal("LockoutEnabled")),
        LockoutEnd          = r.GetNullableDateTimeOffset("LockoutEnd"),
        AccessFailedCount   = r.GetByte(r.GetOrdinal("AccessFailedCount")),
        LastLoginUtc        = r.GetNullableDateTimeOffset("LastLoginUtc"),
        LastLoginIp         = r.GetNullableString("LastLoginIp"),
        PasswordChangedUtc  = r.GetNullableDateTimeOffset("PasswordChangedUtc"),
        IsActive            = r.GetBoolean(r.GetOrdinal("IsActive")),
        AvatarUrl           = r.GetNullableString("AvatarUrl"),
        IsDeleted           = r.GetBoolean(r.GetOrdinal("IsDeleted")),
        CreatedAt           = r.GetDateTimeOffset(r.GetOrdinal("CreatedAt")),
        CreatedBy           = r.GetNullableGuid("CreatedBy"),
        UpdatedAt           = r.GetDateTimeOffset(r.GetOrdinal("UpdatedAt")),
        UpdatedBy           = r.GetNullableGuid("UpdatedBy"),
    };

    public static UserSession MapToUserSession(IDataReader r) => new()
    {
        Id              = r.GetGuid(r.GetOrdinal("SessionId")),
        UserId          = r.GetGuid(r.GetOrdinal("UserId")),
        RefreshToken    = r.GetString(r.GetOrdinal("RefreshToken")),
        DeviceInfo      = r.GetNullableString("DeviceInfo"),
        IpAddress       = r.GetNullableString("IpAddress"),
        UserAgent       = r.GetNullableString("UserAgent"),
        IssuedAtUtc     = r.GetDateTimeOffset(r.GetOrdinal("IssuedAtUtc")),
        ExpiresAtUtc    = r.GetDateTimeOffset(r.GetOrdinal("ExpiresAtUtc")),
        RevokedAtUtc    = r.GetNullableDateTimeOffset("RevokedAtUtc"),
        IsRevoked       = r.GetBoolean(r.GetOrdinal("IsRevoked")),
        ReplacedByToken = r.GetNullableString("ReplacedByToken"),
        CreatedAt       = r.GetDateTimeOffset(r.GetOrdinal("IssuedAtUtc")), 
    };


    private static string? GetNullableString(this IDataReader r, string col)
    {
        var o = r.GetOrdinal(col);
        return r.IsDBNull(o) ? null : r.GetString(o);
    }

    private static DateTimeOffset? GetNullableDateTimeOffset(this IDataReader r, string col)
    {
        var o = r.GetOrdinal(col);
        return r.IsDBNull(o) ? null : (DateTimeOffset)r.GetValue(o);
    }

    private static DateTimeOffset GetDateTimeOffset(this IDataReader r, int ordinal)
        => (DateTimeOffset)r.GetValue(ordinal);

    private static Guid? GetNullableGuid(this IDataReader r, string col)
    {
        var o = r.GetOrdinal(col);
        return r.IsDBNull(o) ? null : r.GetGuid(o);
    }
}