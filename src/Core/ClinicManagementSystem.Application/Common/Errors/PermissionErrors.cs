using ClinicManagementSystem.Application.Common.Models;
using ClinicManagementSystem.Domain.Enums;

namespace ClinicManagementSystem.Application.Common.Errors;

public static class PermissionErrors
{
    public static readonly ErrorModel InvalidPermissionId = ErrorModel.Global(
        "Permission ID cannot be empty or invalid.",
        ErrorCode.InvalidPermissionId.ToString());

    public static readonly ErrorModel PermissionNotFound = ErrorModel.Global(
        "The requested permission was not found.",
        ErrorCode.PermissionNotFound.ToString());

    public static readonly ErrorModel PermissionAlreadyExists = ErrorModel.Global(
        "A permission with the specified code already exists.",
        ErrorCode.PermissionAlreadyExists.ToString());

    public static readonly ErrorModel InvalidPermissionCodeFormat = ErrorModel.Global(
        "Permission code format is invalid. Expected format: 'Module.Entity.Action' (e.g., 'Clinical.Appointment.Create').",
        ErrorCode.InvalidPermissionCodeFormat.ToString());

    public static readonly ErrorModel SystemPermissionProtected = ErrorModel.Global(
        "Built-in system permissions are protected and cannot be modified or deleted.",
        ErrorCode.SystemPermissionProtected.ToString());

    public static readonly ErrorModel PermissionInUse = ErrorModel.Global(
        "Cannot delete permission as it is currently assigned to one or more roles or users.",
        ErrorCode.PermissionInUse.ToString());
}