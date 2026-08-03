using ClinicManagementSystem.Application.Common.Models;
using ClinicManagementSystem.Domain.Enums;

namespace ClinicManagementSystem.Application.Common.Errors;

public static class UserRoleErrors
{
    public static readonly ErrorModel UserRoleAlreadyExists = ErrorModel.Global(
        "This role is already assigned to the user.",
        ErrorCode.UserRoleAlreadyExists.ToString());

    public static readonly ErrorModel UserRoleNotFound = ErrorModel.Global(
        "The specified role assignment was not found for this user.",
        ErrorCode.UserRoleNotFound.ToString());

    public static readonly ErrorModel InvalidRoleValidityPeriod = ErrorModel.Global(
        "The 'ValidTo' date must be greater than the 'ValidFrom' date.",
        ErrorCode.InvalidRoleValidityPeriod.ToString());

    public static readonly ErrorModel CannotRemoveLastAdminRole = ErrorModel.Global(
        "Cannot remove the SuperAdmin role from the last remaining system administrator.",
        ErrorCode.CannotRemoveLastAdminRole.ToString());

    public static readonly ErrorModel EmptyRoleList = ErrorModel.Global(
        "At least one role assignment must be provided.",
        ErrorCode.EmptyRoleList.ToString());
}