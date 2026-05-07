using HouseBudget.Domain.Events;
using HouseBudget.Domain.Exceptions;
using HouseBudget.Domain.ValueObjects;

namespace HouseBudget.Domain.Entities;

public sealed class User : BaseEntity
{
    private readonly List<Account> _accounts = new();
    private readonly List<Category> _categories = new();

    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public Email Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string? PhoneNumber { get; private set; }
    public string DefaultCurrency { get; private set; } = "USD";
    public string? AvatarUrl { get; private set; }
    public bool IsEmailVerified { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiry { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    public string FullName => $"{FirstName} {LastName}";

    public IReadOnlyCollection<Account> Accounts => _accounts.AsReadOnly();
    public IReadOnlyCollection<Category> Categories => _categories.AsReadOnly();

    private User() { }

    public static User Create(string firstName, string lastName, string email, string passwordHash, string currency = "USD")
    {
        if (string.IsNullOrWhiteSpace(firstName)) throw new DomainException("First name is required.");
        if (string.IsNullOrWhiteSpace(lastName)) throw new DomainException("Last name is required.");

        var user = new User
        {
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = Email.Of(email),
            PasswordHash = passwordHash,
            DefaultCurrency = currency
        };

        user.AddDomainEvent(new UserRegisteredEvent(user.Id, email));
        return user;
    }

    public void UpdateProfile(string firstName, string lastName, string? phoneNumber, string? avatarUrl)
    {
        if (string.IsNullOrWhiteSpace(firstName)) throw new DomainException("First name is required.");
        if (string.IsNullOrWhiteSpace(lastName)) throw new DomainException("Last name is required.");
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        PhoneNumber = phoneNumber;
        AvatarUrl = avatarUrl;
        MarkUpdated();
    }

    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash)) throw new DomainException("Password hash cannot be empty.");
        PasswordHash = newPasswordHash;
        RefreshToken = null;
        RefreshTokenExpiry = null;
        MarkUpdated();
    }

    public void SetRefreshToken(string token, DateTime expiry)
    {
        RefreshToken = token;
        RefreshTokenExpiry = expiry;
        MarkUpdated();
    }

    public void RecordLogin() { LastLoginAt = DateTime.UtcNow; MarkUpdated(); }
    public void VerifyEmail() { IsEmailVerified = true; MarkUpdated(); }
    public void Deactivate() { IsActive = false; MarkUpdated(); }
    public void SetDefaultCurrency(string currency) { DefaultCurrency = currency; MarkUpdated(); }
}
