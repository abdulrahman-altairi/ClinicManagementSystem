using System.Data;
using ClinicManagementSystem.Domain.Entities.Auth;
using ClinicManagementSystem.Domain.Enums;
using Microsoft.Data.SqlClient;

namespace ClinicManagementSystem.Infrastructure.Persistence.DataMappers;

public static class UserTokenDataMapperExtensions
{
    public readonly struct UserTokenOrdinals
    {
        public int TokenId { get; }
        public int UserId { get; }
        public int TokenTypeId { get; }
        public int TokenHash { get; }
        public int ExpiresAt { get; }
        public int IsUsed { get; }
        public int UsedAt { get; }
        public int CreatedAt { get; }
        public int CreatedByIp { get; }

        public UserTokenOrdinals(SqlDataReader reader)
        {
            TokenId     = reader.GetOrdinal("TokenId");
            UserId      = reader.GetOrdinal("UserId");
            TokenTypeId = reader.GetOrdinal("TokenTypeId");
            TokenHash   = reader.GetOrdinal("TokenHash");
            ExpiresAt   = reader.GetOrdinal("ExpiresAt");
            IsUsed      = reader.GetOrdinal("IsUsed");
            UsedAt      = reader.GetOrdinal("UsedAt");
            CreatedAt   = reader.GetOrdinal("CreatedAt");
            CreatedByIp = reader.GetOrdinal("CreatedByIp");
        }
    }

    public static UserToken MapToUserToken(this SqlDataReader reader, UserTokenOrdinals ordinals)
    {
        return new UserToken
        {
            Id          = reader.GetGuid(ordinals.TokenId),
            UserId      = reader.GetGuid(ordinals.UserId),
            TokenType   = (TokenType)reader.GetByte(ordinals.TokenTypeId),
            TokenHash   = reader.GetString(ordinals.TokenHash),
            ExpiresAt   = reader.GetDateTimeOffset(ordinals.ExpiresAt),
            IsUsed      = reader.GetBoolean(ordinals.IsUsed),
            UsedAt      = reader.IsDBNull(ordinals.UsedAt) ? null : reader.GetDateTimeOffset(ordinals.UsedAt),
            CreatedAt   = reader.GetDateTimeOffset(ordinals.CreatedAt),
            CreatedByIp = reader.IsDBNull(ordinals.CreatedByIp) ? null : reader.GetString(ordinals.CreatedByIp)
        };
    }
}