using ClinicManagementSystem.Domain.Common;
using System.Net;

namespace ClinicManagementSystem.Domain.ValueObjects;

public sealed class IpAddress : ValueObject
{
    public string Value { get; }

    public bool IsIpv6 { get; }

    public IpAddress(string value, bool isIpv6 = false)
    {
        Value = value;
        IsIpv6 = isIpv6;
    }

    public static IpAddress Create(string address)
    {
        if (string.IsNullOrEmpty(address))
            throw new ArgumentException("IP address cannot be empty.", nameof(address));

        if (!IPAddress.TryParse(address.Trim(), out var parsed))
            throw new ArgumentException($"'{address}' is not a valid IP address.", nameof(address));

        return new IpAddress(
            parsed.ToString(),
            parsed.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6);
    }

    public static IpAddress? TryCreate(string? address)
    {
        if (string.IsNullOrWhiteSpace(address)) return null;
        try { return Create(address); }
        catch { return null; }
    }

    public bool IsLoopback
    => IPAddress.TryParse(Value, out var ip) && IPAddress.IsLoopback(ip);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(IpAddress ip) => ip.Value;
}
