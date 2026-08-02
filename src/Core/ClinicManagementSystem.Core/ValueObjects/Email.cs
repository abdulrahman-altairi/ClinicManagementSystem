using ClinicManagementSystem.Domain.Common;
using System.Text.RegularExpressions;

namespace ClinicManagementSystem.Domain.ValueObjects;

public sealed class Email : ValueObject
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase,
        TimeSpan.FromMilliseconds(250));

    public string Value { get; }

    public string Normalized { get; }

    private Email(string value)
    {
        Value = value.Trim();
        Normalized = Value.ToUpperInvariant();
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email address cannot be empty.", nameof(email));

        if (email.Length > 256)
            throw new ArgumentException("Email address cannot exceed 256 characters.", nameof(email));

        if (!EmailRegex.IsMatch(email))
            throw new ArgumentException($"'{email}' is not a valid email address.", nameof(email));

        return new Email(email);
    }

    public static Email? TryCreate(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return null;
        try { return Create(email); }
        catch { return null; }
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Normalized;
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}
