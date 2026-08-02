using ClinicManagementSystem.Domain.Entities.Auth;
using System.Security.Claims;

namespace ClinicManagementSystem.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(
       ApplicationUser user,
       IEnumerable<string> roles,
       IEnumerable<string> permissions);

    string GenerateRefreshToken();

    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
