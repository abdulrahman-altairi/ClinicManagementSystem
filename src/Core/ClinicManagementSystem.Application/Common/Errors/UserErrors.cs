using ClinicManagementSystem.Application.Common.Models;
using ClinicManagementSystem.Domain.Enums;

namespace ClinicManagementSystem.Application.Common.Errors;

public static class UserErrors
{
    public static readonly ErrorModel InvalidUserId = ErrorModel.Global(
        "User ID cannot be empty or invalid.",
        ErrorCode.InvalidUserId.ToString());

    public static readonly ErrorModel UserNotFound = ErrorModel.Global(
        "The requested user was not found.",
        ErrorCode.UserNotFound.ToString());
}