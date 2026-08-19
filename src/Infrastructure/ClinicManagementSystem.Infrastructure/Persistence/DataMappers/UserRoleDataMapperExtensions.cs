using System.Data;
using ClinicManagementSystem.Domain.Entities.Auth;
using Microsoft.Data.SqlClient;

namespace ClinicManagementSystem.Infrastructure.Persistence.DataMappers;

public static class UserRoleDataMapperExtensions
{
    public readonly struct UserRoleOrdinals
    {
        public int UserRoleId { get; }
        public int UserId { get; }
        public int RoleId { get; }
        public int ValidFrom { get; }
        public int ValidTo { get; }
        public int AssignedBy { get; }
        public int CreatedAt { get; }

        public UserRoleOrdinals(SqlDataReader reader)
        {
            UserRoleId = reader.GetOrdinal("UserRoleId");
            UserId = reader.GetOrdinal("UserId");
            RoleId = reader.GetOrdinal("RoleId");
            ValidFrom = reader.GetOrdinal("ValidFrom");
            ValidTo = reader.GetOrdinal("ValidTo");
            AssignedBy = reader.GetOrdinal("AssignedBy");
            CreatedAt = reader.GetOrdinal("CreatedAt");
        }
    }

    public static UserRole MapToUserRole(this SqlDataReader reader, UserRoleOrdinals ordinals)
    {
        return new UserRole
        {
            Id = reader.GetGuid(ordinals.UserRoleId),
            UserId = reader.GetGuid(ordinals.UserId),
            RoleId = reader.GetGuid(ordinals.RoleId),
            ValidFrom = reader.GetDateTimeOffset(ordinals.ValidFrom),
            ValidTo = reader.IsDBNull(ordinals.ValidTo) ? null : reader.GetDateTimeOffset(ordinals.ValidTo),
            AssignedBy = reader.IsDBNull(ordinals.AssignedBy) ? null : reader.GetGuid(ordinals.AssignedBy),
            CreatedAt = reader.GetDateTimeOffset(ordinals.CreatedAt)
        };
    }
}