using HouseBudget.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace HouseBudget.Domain.ValueObjects;

public sealed class Email : IEquatable<Email>
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Of(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Email cannot be empty.");
        if (!EmailRegex.IsMatch(value))
            throw new DomainException($"'{value}' is not a valid email address.");
        return new Email(value.ToLowerInvariant().Trim());
    }

    public bool Equals(Email? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is Email e && Equals(e);
    public override int GetHashCode() => HashCode.Combine(Value);
    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}
