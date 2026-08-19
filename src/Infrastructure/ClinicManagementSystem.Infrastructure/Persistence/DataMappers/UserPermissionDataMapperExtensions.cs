using System.Data;
using ClinicManagementSystem.Application.DTOs.Auth.UserPermissions;
using ClinicManagementSystem.Domain.Enums;
using Microsoft.Data.SqlClient;

namespace ClinicManagementSystem.Infrastructure.Persistence.DataMappers;

public static class UserPermissionDataMapperExtensions
{
    public readonly struct UserPermissionResponseOrdinals
    {
        public int UserPermissionId { get; }
        public int UserId { get; }
        public int PermissionId { get; }
        public int PermissionCode { get; }
        public int PermissionName { get; }
        public int Module { get; }
        public int GrantType { get; }
        public int Reason { get; }
        public int ValidFrom { get; }
        public int ValidTo { get; }
        public int GrantedBy { get; }
        public int IsActive { get; }
        public int CreatedAt { get; }

        public UserPermissionResponseOrdinals(SqlDataReader reader)
        {
            UserPermissionId = reader.GetOrdinal("UserPermissionId");
            UserId = reader.GetOrdinal("UserId");
            PermissionId = reader.GetOrdinal("PermissionId");
            PermissionCode = reader.GetOrdinal("PermissionCode");
            PermissionName = reader.GetOrdinal("PermissionName");
            Module = reader.GetOrdinal("Module");
            GrantType = reader.GetOrdinal("GrantType");
            Reason = reader.GetOrdinal("Reason");
            ValidFrom = reader.GetOrdinal("ValidFrom");
            ValidTo = reader.GetOrdinal("ValidTo");
            GrantedBy = reader.GetOrdinal("GrantedBy");
            IsActive = reader.GetOrdinal("IsActive");
            CreatedAt = reader.GetOrdinal("CreatedAt");
        }
    }

    public static UserPermissionResponseDto MapToUserPermissionResponseDto(
        this SqlDataReader reader, 
        UserPermissionResponseOrdinals ordinals)
    {
        var grantTypeString = reader.GetString(ordinals.GrantType);
        Enum.TryParse<GrantType>(grantTypeString, true, out var grantTypeEnum);

        return new UserPermissionResponseDto
        {
            UserPermissionId = reader.GetGuid(ordinals.UserPermissionId),
            UserId = reader.GetGuid(ordinals.UserId),
            PermissionId = reader.GetGuid(ordinals.PermissionId),
            PermissionCode = reader.GetString(ordinals.PermissionCode),
            PermissionName = reader.GetString(ordinals.PermissionName),
            Module = reader.GetString(ordinals.Module),
            GrantType = grantTypeEnum,
            Reason = reader.IsDBNull(ordinals.Reason) ? null : reader.GetString(ordinals.Reason),
            ValidFrom = reader.GetDateTimeOffset(ordinals.ValidFrom),
            ValidTo = reader.IsDBNull(ordinals.ValidTo) ? null : reader.GetDateTimeOffset(ordinals.ValidTo),
            GrantedBy = reader.IsDBNull(ordinals.GrantedBy) ? null : reader.GetGuid(ordinals.GrantedBy),
            IsActive = reader.GetBoolean(ordinals.IsActive),
            CreatedAt = reader.GetDateTimeOffset(ordinals.CreatedAt)
        };
    }
}