using ClinicManagementSystem.Application.Common.Models;
using ClinicManagementSystem.Domain.Enums;

namespace ClinicManagementSystem.Application.Common.Errors;

public static class UserPermissionErrors
{
    public static readonly ErrorModel UserPermissionAlreadyExists = ErrorModel.Global(
        "A permission override with the same type already exists for this user and permission.",
        ErrorCode.UserPermissionAlreadyExists.ToString());

    public static readonly ErrorModel UserPermissionNotFound = ErrorModel.Global(
        "The specified user permission override was not found.",
        ErrorCode.UserPermissionNotFound.ToString());

    public static readonly ErrorModel InvalidGrantType = ErrorModel.Global(
        "GrantType must be either 'GRANT' or 'DENY'.",
        ErrorCode.InvalidGrantType.ToString());

    public static readonly ErrorModel InvalidOverrideValidityPeriod = ErrorModel.Global(
        "The 'ValidTo' date must be greater than the 'ValidFrom' date.",
        ErrorCode.InvalidOverrideValidityPeriod.ToString());

    public static readonly ErrorModel EmptyOverrideList = ErrorModel.Global(
        "At least one permission override item must be provided.",
        ErrorCode.EmptyOverrideList.ToString());
}