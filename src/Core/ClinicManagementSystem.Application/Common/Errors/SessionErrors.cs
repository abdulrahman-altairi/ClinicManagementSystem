using ClinicManagementSystem.Application.Common.Models;
using ClinicManagementSystem.Domain.Enums;

namespace ClinicManagementSystem.Application.Common.Errors;

public static class SessionErrors
{
    public static readonly ErrorModel SessionNotFound = new(
        "Session.NotFound",
        "The specified user session was not found.");

    public static readonly ErrorModel InvalidRefreshToken = new(
        "Session.InvalidRefreshToken",
        "The provided refresh token is invalid or expired.");

    public static readonly ErrorModel TokenRevoked = new(
        "Session.TokenRevoked",
        "The refresh token has been revoked.");

    public static readonly ErrorModel TokenExpired = new(
        "Session.TokenExpired",
        "The refresh token has expired.");
}