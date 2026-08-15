using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ClinicManagementSystem.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ClinicManagementSystem.Infrastructure.ExternalServices;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var claim = User?.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User?.FindFirstValue(JwtRegisteredClaimNames.Sub);
            return Guid.TryParse(claim, out var id) ? id : null;
        }
    }

    public string? Username
        => User?.FindFirstValue(ClaimTypes.Name)
        ?? User?.FindFirstValue(JwtRegisteredClaimNames.UniqueName);

    public string? Email
        => User?.FindFirstValue(ClaimTypes.Email)
        ?? User?.FindFirstValue(JwtRegisteredClaimNames.Email);

    /// <inheritdoc />
    public string? IpAddress
    {
        get
        {
            var ctx = _httpContextAccessor.HttpContext;
            if (ctx is null) return null;

            var forwarded = ctx.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(forwarded))
                return forwarded.Split(',', StringSplitOptions.TrimEntries)[0];

            return ctx.Connection.RemoteIpAddress?.ToString();
        }
    }

    public string? UserAgent
        => _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();

    public bool IsAuthenticated
        => User?.Identity?.IsAuthenticated == true;
}