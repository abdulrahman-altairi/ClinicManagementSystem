using ClinicManagementSystem.Application.Common.Models;
using ClinicManagementSystem.Domain.Enums;

namespace ClinicManagementSystem.Application.Common.Errors;

public static class RolePermissionErrors
{
    public static readonly ErrorModel RolePermissionAlreadyExists = ErrorModel.Global(
        "This permission is already assigned to the specified role.",
        ErrorCode.RolePermissionAlreadyExists.ToString());

    public static readonly ErrorModel RolePermissionNotFound = ErrorModel.Global(
        "The specified permission mapping was not found for this role.",
        ErrorCode.RolePermissionNotFound.ToString());

    public static readonly ErrorModel CannotModifySystemRolePermissions = ErrorModel.Global(
        "Permissions for core system roles cannot be modified.",
        ErrorCode.CannotModifySystemRolePermissions.ToString());

    public static readonly ErrorModel EmptyPermissionList = ErrorModel.Global(
        "At least one permission ID must be provided.",
        ErrorCode.EmptyPermissionList.ToString());
}