using Microsoft.Data.SqlClient;
using ClinicManagementSystem.Domain.Entities.Auth;

namespace ClinicManagementSystem.Infrastructure.Persistence.DataMappers;

public static class RoleMapper
{

    public readonly struct RoleOrdinals
    {
        public int Id { get; }
        public int RoleName { get; }
        public int NormalizedName { get; }
        public int Description { get; }
        public int IsSystemRole { get; }
        public int IsActive { get; }
        public int IsDeleted { get; }
        public int CreatedAt { get; }
        public int CreatedBy { get; }
        public int UpdatedAt { get; }
        public int UpdatedBy { get; }

        public RoleOrdinals(SqlDataReader reader)
        {
            Id = reader.GetOrdinal("RoleId");
            RoleName = reader.GetOrdinal("RoleName");
            NormalizedName = reader.GetOrdinal("NormalizedName");
            Description = reader.GetOrdinal("Description");
            IsSystemRole = reader.GetOrdinal("IsSystemRole");
            IsActive = reader.GetOrdinal("IsActive");
            IsDeleted = reader.GetOrdinal("IsDeleted");
            CreatedAt = reader.GetOrdinal("CreatedAt");
            CreatedBy = reader.GetOrdinal("CreatedBy");
            UpdatedAt = reader.GetOrdinal("UpdatedAt");
            UpdatedBy = reader.GetOrdinal("UpdatedBy");
        }
    }


    public static Role MapToEntity(SqlDataReader reader)
    {
        return new Role
        {
            Id = reader.GetGuid(reader.GetOrdinal("RoleId")),
            RoleName = reader.GetString(reader.GetOrdinal("RoleName")),
            NormalizedName = reader.GetString(reader.GetOrdinal("NormalizedName")),
            Description = reader.IsDBNull(reader.GetOrdinal("Description")) 
                ? null 
                : reader.GetString(reader.GetOrdinal("Description")),
            IsSystemRole = reader.GetBoolean(reader.GetOrdinal("IsSystemRole")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
            IsDeleted = reader.GetBoolean(reader.GetOrdinal("IsDeleted")),
            CreatedAt = reader.GetDateTimeOffset(reader.GetOrdinal("CreatedAt")),
            CreatedBy = reader.IsDBNull(reader.GetOrdinal("CreatedBy")) 
                ? null 
                : reader.GetGuid(reader.GetOrdinal("CreatedBy")),
            UpdatedAt = reader.GetDateTimeOffset(reader.GetOrdinal("UpdatedAt")),
            UpdatedBy = reader.IsDBNull(reader.GetOrdinal("UpdatedBy")) 
                ? null 
                : reader.GetGuid(reader.GetOrdinal("UpdatedBy"))
        };
    }

    public static Role MapToEntity(SqlDataReader reader, RoleOrdinals ordinals)
    {
        return new Role
        {
            Id = reader.GetGuid(ordinals.Id),
            RoleName = reader.GetString(ordinals.RoleName),
            NormalizedName = reader.GetString(ordinals.NormalizedName),
            Description = reader.IsDBNull(ordinals.Description) ? null : reader.GetString(ordinals.Description),
            IsSystemRole = reader.GetBoolean(ordinals.IsSystemRole),
            IsActive = reader.GetBoolean(ordinals.IsActive),
            IsDeleted = reader.GetBoolean(ordinals.IsDeleted),
            CreatedAt = reader.GetDateTimeOffset(ordinals.CreatedAt),
            CreatedBy = reader.IsDBNull(ordinals.CreatedBy) ? null : reader.GetGuid(ordinals.CreatedBy),
            UpdatedAt = reader.GetDateTimeOffset(ordinals.UpdatedAt),
            UpdatedBy = reader.IsDBNull(ordinals.UpdatedBy) ? null : reader.GetGuid(ordinals.UpdatedBy)
        };
    }
}