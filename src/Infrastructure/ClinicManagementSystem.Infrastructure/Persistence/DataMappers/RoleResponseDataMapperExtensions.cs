using System.Data;
using ClinicManagementSystem.Application.DTOs.Auth.Role;
using Microsoft.Data.SqlClient;

namespace ClinicManagementSystem.Infrastructure.Persistence.DataMappers;

public static class RoleResponseDataMapperExtensions
{
    public readonly struct RoleResponseOrdinals
    {
        public int RoleId { get; }
        public int RoleName { get; }
        public int Description { get; }
        public int IsSystemRole { get; }
        public int IsActive { get; }

        public RoleResponseOrdinals(SqlDataReader reader)
        {
            RoleId = reader.GetOrdinal("RoleId");
            RoleName = reader.GetOrdinal("RoleName");
            Description = reader.GetOrdinal("Description");
            IsSystemRole = reader.GetOrdinal("IsSystemRole");
            IsActive = reader.GetOrdinal("IsActive");
        }
    }

    public static RoleResponseDto MapToRoleResponseDto(this SqlDataReader reader, RoleResponseOrdinals ordinals)
    {
        return new RoleResponseDto
        {
            RoleId = reader.GetGuid(ordinals.RoleId),
            RoleName = reader.GetString(ordinals.RoleName),
            Description = reader.IsDBNull(ordinals.Description) ? null : reader.GetString(ordinals.Description),
            IsSystemRole = reader.GetBoolean(ordinals.IsSystemRole),
            IsActive = reader.GetBoolean(ordinals.IsActive)
        };
    }
}