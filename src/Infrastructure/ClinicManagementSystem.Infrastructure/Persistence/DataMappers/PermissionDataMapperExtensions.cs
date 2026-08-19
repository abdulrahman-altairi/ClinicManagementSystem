using System.Data;
using ClinicManagementSystem.Domain.Entities.Auth;
using Microsoft.Data.SqlClient;

namespace ClinicManagementSystem.Infrastructure.Persistence.DataMappers;

public static class PermissionDataMapperExtensions
{
    public readonly struct PermissionOrdinals
    {
        public int Id { get; }
        public int PermissionCode { get; }
        public int PermissionName { get; }
        public int Module { get; }
        public int Description { get; }
        public int IsActive { get; }
        public int IsDeleted { get; }
        public int CreatedAt { get; }
        public int CreatedBy { get; }
        public int UpdatedAt { get; }
        public int UpdatedBy { get; }

        public PermissionOrdinals(SqlDataReader reader)
        {
            Id = reader.GetOrdinal("PermissionId");
            PermissionCode = reader.GetOrdinal("PermissionCode");
            PermissionName = reader.GetOrdinal("PermissionName");
            Module = reader.GetOrdinal("Module");
            Description = reader.GetOrdinal("Description");
            IsActive = reader.GetOrdinal("IsActive");
            IsDeleted = reader.GetOrdinal("IsDeleted");
            CreatedAt = reader.GetOrdinal("CreatedAt");
            CreatedBy = reader.GetOrdinal("CreatedBy");
            UpdatedAt = reader.GetOrdinal("UpdatedAt");
            UpdatedBy = reader.GetOrdinal("UpdatedBy");
        }
    }

    public static Permission MapToPermission(this SqlDataReader reader)
    {
        return new Permission
        {
            Id = reader.GetGuid(reader.GetOrdinal("PermissionId")),
            PermissionCode = reader.GetString(reader.GetOrdinal("PermissionCode")),
            PermissionName = reader.GetString(reader.GetOrdinal("PermissionName")),
            Module = reader.GetString(reader.GetOrdinal("Module")),
            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
            IsDeleted = reader.GetBoolean(reader.GetOrdinal("IsDeleted")),
            CreatedAt = reader.GetDateTimeOffset(reader.GetOrdinal("CreatedAt")),
            CreatedBy = reader.IsDBNull(reader.GetOrdinal("CreatedBy")) ? null : reader.GetGuid(reader.GetOrdinal("CreatedBy")),
            UpdatedAt = reader.GetDateTimeOffset(reader.GetOrdinal("UpdatedAt")),
            UpdatedBy = reader.IsDBNull(reader.GetOrdinal("UpdatedBy")) ? null : reader.GetGuid(reader.GetOrdinal("UpdatedBy"))
        };
    }

    public static Permission MapToPermission(this SqlDataReader reader, PermissionOrdinals ordinals)
    {
        return new Permission
        {
            Id = reader.GetGuid(ordinals.Id),
            PermissionCode = reader.GetString(ordinals.PermissionCode),
            PermissionName = reader.GetString(ordinals.PermissionName),
            Module = reader.GetString(ordinals.Module),
            Description = reader.IsDBNull(ordinals.Description) ? null : reader.GetString(ordinals.Description),
            IsActive = reader.GetBoolean(ordinals.IsActive),
            IsDeleted = reader.GetBoolean(ordinals.IsDeleted),
            CreatedAt = reader.GetDateTimeOffset(ordinals.CreatedAt),
            CreatedBy = reader.IsDBNull(ordinals.CreatedBy) ? null : reader.GetGuid(ordinals.CreatedBy),
            UpdatedAt = reader.GetDateTimeOffset(ordinals.UpdatedAt),
            UpdatedBy = reader.IsDBNull(ordinals.UpdatedBy) ? null : reader.GetGuid(ordinals.UpdatedBy)
        };
    }
}