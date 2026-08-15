using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ClinicManagementSystem.Application.Common.Interfaces;
using ClinicManagementSystem.Domain.Entities.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ClinicManagementSystem.Infrastructure.Identity;

public sealed class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public string Secret    { get; init; } = string.Empty;
    public string Issuer    { get; init; } = string.Empty;
    public string Audience  { get; init; } = string.Empty;

    public int ExpiryMinutes { get; init; } = 15;
}

public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings                  _settings;
    private readonly ILogger<JwtTokenGenerator>   _logger;

    public JwtTokenGenerator(IOptions<JwtSettings> settings, ILogger<JwtTokenGenerator> logger)
    {
        _settings = settings.Value;
        _logger   = logger;

        if (_settings.Secret.Length < 64)
            throw new InvalidOperationException(
                "JwtSettings:Secret must be at least 64 characters to satisfy HS512 requirements.");
    }


    public string GenerateAccessToken(
        ApplicationUser user,
        IEnumerable<string> roles,
        IEnumerable<string> permissions)
    {
        var key         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
        var expiry      = DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes);

        var claims = new List<Claim>(capacity: 8 + roles.Count() + permissions.Count())
        {
            new(JwtRegisteredClaimNames.Sub,  user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new(JwtRegisteredClaimNames.Jti,  Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        foreach (var permission in permissions)
            claims.Add(new Claim("permission", permission));

        var descriptor = new SecurityTokenDescriptor
        {
            Subject            = new ClaimsIdentity(claims),
            Expires            = expiry,
            Issuer             = _settings.Issuer,
            Audience           = _settings.Audience,
            SigningCredentials = credentials,
            NotBefore          = DateTime.UtcNow,
        };

        var handler = new JwtSecurityTokenHandler();
        var token   = handler.CreateToken(descriptor);
        return handler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret)),
            ValidateIssuer           = true,
            ValidIssuer              = _settings.Issuer,
            ValidateAudience         = true,
            ValidAudience            = _settings.Audience,
            ValidateLifetime         = false, 
            ClockSkew                = TimeSpan.Zero,
        };

        try
        {
            var handler    = new JwtSecurityTokenHandler();
            var principal  = handler.ValidateToken(token, validationParams, out var securityToken);

            if (securityToken is not JwtSecurityToken jwt ||
                !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("GetPrincipalFromExpiredToken: unexpected algorithm {Alg}", securityToken?.ToString());
                return null;
            }

            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "GetPrincipalFromExpiredToken: token validation failed.");
            return null;
        }
    }
}
