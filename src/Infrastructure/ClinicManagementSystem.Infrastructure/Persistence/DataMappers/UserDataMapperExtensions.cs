using System.Data;
using ClinicManagementSystem.Domain.Entities.Auth;
using Microsoft.Data.SqlClient;

namespace ClinicManagementSystem.Infrastructure.Persistence.DataMappers;

public static class UserDataMapperExtensions
{
    public readonly struct UserOrdinals
    {
        public int Id { get; }
        public int Username { get; }
        public int NormalizedUsername { get; }
        public int Email { get; }
        public int NormalizedEmail { get; }
        public int PasswordHash { get; }
        public int PasswordSalt { get; }
        public int FirstName { get; }
        public int LastName { get; }
        public int PhoneNumber { get; }
        public int PhoneVerified { get; }
        public int EmailVerified { get; }
        public int TwoFactorEnabled { get; }
        public int TwoFactorSecret { get; }
        public int LockoutEnabled { get; }
        public int LockoutEnd { get; }
        public int AccessFailedCount { get; }
        public int LastLoginUtc { get; }
        public int LastLoginIp { get; }
        public int PasswordChangedUtc { get; }
        public int IsActive { get; }
        public int AvatarUrl { get; }
        public int IsDeleted { get; }
        public int CreatedAt { get; }
        public int CreatedBy { get; }
        public int UpdatedAt { get; }
        public int UpdatedBy { get; }

        public UserOrdinals(SqlDataReader reader)
        {
            Id = reader.GetOrdinal("UserId");
            Username = reader.GetOrdinal("Username");
            NormalizedUsername = reader.GetOrdinal("NormalizedUsername");
            Email = reader.GetOrdinal("Email");
            NormalizedEmail = reader.GetOrdinal("NormalizedEmail");
            PasswordHash = reader.GetOrdinal("PasswordHash");
            PasswordSalt = reader.GetOrdinal("PasswordSalt");
            FirstName = reader.GetOrdinal("FirstName");
            LastName = reader.GetOrdinal("LastName");
            PhoneNumber = reader.GetOrdinal("PhoneNumber");
            PhoneVerified = reader.GetOrdinal("PhoneVerified");
            EmailVerified = reader.GetOrdinal("EmailVerified");
            TwoFactorEnabled = reader.GetOrdinal("TwoFactorEnabled");
            TwoFactorSecret = reader.GetOrdinal("TwoFactorSecret");
            LockoutEnabled = reader.GetOrdinal("LockoutEnabled");
            LockoutEnd = reader.GetOrdinal("LockoutEnd");
            AccessFailedCount = reader.GetOrdinal("AccessFailedCount");
            LastLoginUtc = reader.GetOrdinal("LastLoginUtc");
            LastLoginIp = reader.GetOrdinal("LastLoginIp");
            PasswordChangedUtc = reader.GetOrdinal("PasswordChangedUtc");
            IsActive = reader.GetOrdinal("IsActive");
            AvatarUrl = reader.GetOrdinal("AvatarUrl");
            IsDeleted = reader.GetOrdinal("IsDeleted");
            CreatedAt = reader.GetOrdinal("CreatedAt");
            CreatedBy = reader.GetOrdinal("CreatedBy");
            UpdatedAt = reader.GetOrdinal("UpdatedAt");
            UpdatedBy = reader.GetOrdinal("UpdatedBy");
        }
    }

    public static ApplicationUser MapToApplicationUser(this SqlDataReader r, UserOrdinals ordinals) => new()
    {
        Id                  = r.GetGuid(ordinals.Id),
        Username            = r.GetString(ordinals.Username),
        NormalizedUsername  = r.GetString(ordinals.NormalizedUsername),
        Email               = r.GetString(ordinals.Email),
        NormalizedEmail     = r.GetString(ordinals.NormalizedEmail),
        PasswordHash        = r.GetString(ordinals.PasswordHash),
        PasswordSalt        = r.GetString(ordinals.PasswordSalt),
        FirstName           = r.IsDBNull(ordinals.FirstName) ? string.Empty : r.GetString(ordinals.FirstName),
        LastName            = r.IsDBNull(ordinals.LastName) ? string.Empty : r.GetString(ordinals.LastName),
        PhoneNumber         = r.IsDBNull(ordinals.PhoneNumber) ? null : r.GetString(ordinals.PhoneNumber),
        PhoneVerified       = r.GetBoolean(ordinals.PhoneVerified),
        EmailVerified       = r.GetBoolean(ordinals.EmailVerified),
        TwoFactorEnabled    = r.GetBoolean(ordinals.TwoFactorEnabled),
        TwoFactorSecret     = r.IsDBNull(ordinals.TwoFactorSecret) ? null : r.GetString(ordinals.TwoFactorSecret),
        LockoutEnabled      = r.GetBoolean(ordinals.LockoutEnabled),
        LockoutEnd          = r.IsDBNull(ordinals.LockoutEnd) ? null : (DateTimeOffset)r.GetValue(ordinals.LockoutEnd),
        AccessFailedCount   = r.GetByte(ordinals.AccessFailedCount),
        LastLoginUtc        = r.IsDBNull(ordinals.LastLoginUtc) ? null : (DateTimeOffset)r.GetValue(ordinals.LastLoginUtc),
        LastLoginIp         = r.IsDBNull(ordinals.LastLoginIp) ? null : r.GetString(ordinals.LastLoginIp),
        PasswordChangedUtc  = r.IsDBNull(ordinals.PasswordChangedUtc) ? null : (DateTimeOffset)r.GetValue(ordinals.PasswordChangedUtc),
        IsActive            = r.GetBoolean(ordinals.IsActive),
        AvatarUrl           = r.IsDBNull(ordinals.AvatarUrl) ? null : r.GetString(ordinals.AvatarUrl),
        IsDeleted           = r.GetBoolean(ordinals.IsDeleted),
        CreatedAt           = (DateTimeOffset)r.GetValue(ordinals.CreatedAt),
        CreatedBy           = r.IsDBNull(ordinals.CreatedBy) ? null : r.GetGuid(ordinals.CreatedBy),
        UpdatedAt           = (DateTimeOffset)r.GetValue(ordinals.UpdatedAt),
        UpdatedBy           = r.IsDBNull(ordinals.UpdatedBy) ? null : r.GetGuid(ordinals.UpdatedBy),
    };

    public static ApplicationUser MapToApplicationUser(this SqlDataReader r)
    {
        var ordinals = new UserOrdinals(r);
        return r.MapToApplicationUser(ordinals);
    }
}