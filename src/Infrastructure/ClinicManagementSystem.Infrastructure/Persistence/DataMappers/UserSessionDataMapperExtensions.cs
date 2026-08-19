using System.Data;
using ClinicManagementSystem.Domain.Entities.Auth;
using Microsoft.Data.SqlClient;

namespace ClinicManagementSystem.Infrastructure.Persistence.DataMappers;

public static class UserSessionDataMapperExtensions
{
    public readonly struct UserSessionOrdinals
    {
        public int SessionId { get; }
        public int UserId { get; }
        public int RefreshToken { get; }
        public int DeviceInfo { get; }
        public int IpAddress { get; }
        public int UserAgent { get; }
        public int IssuedAtUtc { get; }
        public int ExpiresAtUtc { get; }
        public int RevokedAtUtc { get; }
        public int IsRevoked { get; }
        public int ReplacedByToken { get; }

        public UserSessionOrdinals(SqlDataReader reader)
        {
            SessionId = reader.GetOrdinal("SessionId");
            UserId = reader.GetOrdinal("UserId");
            RefreshToken = reader.GetOrdinal("RefreshToken");
            DeviceInfo = reader.GetOrdinal("DeviceInfo");
            IpAddress = reader.GetOrdinal("IpAddress");
            UserAgent = reader.GetOrdinal("UserAgent");
            IssuedAtUtc = reader.GetOrdinal("IssuedAtUtc");
            ExpiresAtUtc = reader.GetOrdinal("ExpiresAtUtc");
            RevokedAtUtc = reader.GetOrdinal("RevokedAtUtc");
            IsRevoked = reader.GetOrdinal("IsRevoked");
            ReplacedByToken = reader.GetOrdinal("ReplacedByToken");
        }
    }

    public static UserSession MapToUserSession(this SqlDataReader reader, UserSessionOrdinals ordinals)
    {
        return new UserSession
        {
            Id = reader.GetGuid(ordinals.SessionId),
            UserId = reader.GetGuid(ordinals.UserId),
            RefreshToken = reader.GetString(ordinals.RefreshToken),
            DeviceInfo = reader.IsDBNull(ordinals.DeviceInfo) ? null : reader.GetString(ordinals.DeviceInfo),
            IpAddress = reader.IsDBNull(ordinals.IpAddress) ? null : reader.GetString(ordinals.IpAddress),
            UserAgent = reader.IsDBNull(ordinals.UserAgent) ? null : reader.GetString(ordinals.UserAgent),
            IssuedAtUtc = reader.GetDateTimeOffset(ordinals.IssuedAtUtc),
            ExpiresAtUtc = reader.GetDateTimeOffset(ordinals.ExpiresAtUtc),
            RevokedAtUtc = reader.IsDBNull(ordinals.RevokedAtUtc) ? null : reader.GetDateTimeOffset(ordinals.RevokedAtUtc),
            IsRevoked = reader.GetBoolean(ordinals.IsRevoked),
            ReplacedByToken = reader.IsDBNull(ordinals.ReplacedByToken) ? null : reader.GetString(ordinals.ReplacedByToken)
        };
    }
}