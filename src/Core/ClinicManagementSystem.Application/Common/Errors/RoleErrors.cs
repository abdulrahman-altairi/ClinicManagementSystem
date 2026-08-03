using ClinicManagementSystem.Application.Common.Models;
using ClinicManagementSystem.Domain.Enums;

namespace ClinicManagementSystem.Application.Common.Errors;

public static class RoleErrors
{
    public static readonly ErrorModel InvalidRoleId = ErrorModel.Global(
        "Role ID cannot be empty or invalid.",
        ErrorCode.InvalidRoleId.ToString());

    public static readonly ErrorModel RoleNotFound = ErrorModel.Global(
        "The requested role was not found.",
        ErrorCode.RoleNotFound.ToString());

    public static readonly ErrorModel RoleAlreadyExists = ErrorModel.Global(
        "A role with the specified name already exists.",
        ErrorCode.RoleAlreadyExists.ToString());

    public static readonly ErrorModel SystemRoleProtected = ErrorModel.Global(
        "Built-in system roles are protected and cannot be modified or deleted.",
        ErrorCode.SystemRoleProtected.ToString());

    public static readonly ErrorModel RoleInUse = ErrorModel.Global(
        "Cannot delete role as it is currently assigned to one or more active users.",
        ErrorCode.RoleInUse.ToString());
}