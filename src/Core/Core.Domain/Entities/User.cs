using Core.Domain.Common;
using Core.Domain.Enums;
using Core.Domain.Exceptions;

namespace Core.Domain.Entities;

public class User : AuditableEntity
{
    private User()
    {
    }

    public string Email { get; private set; } = string.Empty;
    public string UserName { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;

    public UserRole Role { get; private set; }
    public UserStatus Status { get; private set; }

    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public string? TimeZoneId { get; private set; }

    /// <summary>Trader-facing display name. Unused for admins.</summary>
    public string? DisplayName { get; private set; }

    /// <summary>ISO currency the trader prefers for balances and reporting.</summary>
    public string? PreferredCurrency { get; private set; }

    public DateTime? LastLoginAtUtc { get; private set; }

    public Balance? Balance { get; private set; }

    public string FullName => $"{FirstName} {LastName}".Trim();

    public bool IsAdmin => Role == UserRole.Admin;
    public bool IsTrader => Role == UserRole.Trader;
    public bool IsActive => Status == UserStatus.Active && !IsDeleted;

    public static User CreateAdmin(
        string email,
        string userName,
        string passwordHash,
        string firstName,
        string lastName,
        string? phoneNumber = null)
    {
        var user = CreateCore(
            email,
            userName,
            passwordHash,
            firstName,
            lastName,
            phoneNumber,
            UserRole.Admin);

        user.Status = UserStatus.Active;
        return user;
    }

    public static User CreateTrader(
        string email,
        string userName,
        string passwordHash,
        string firstName,
        string lastName,
        string preferredCurrency,
        string? phoneNumber = null,
        string? displayName = null,
        string? timeZoneId = null)
    {
        if (string.IsNullOrWhiteSpace(preferredCurrency) || preferredCurrency.Length != 3)
            throw new DomainException("Trader preferred currency must be a 3-letter ISO code.");

        var user = CreateCore(
            email,
            userName,
            passwordHash,
            firstName,
            lastName,
            phoneNumber,
            UserRole.Trader);

        user.Status = UserStatus.Pending;
        user.PreferredCurrency = preferredCurrency.ToUpperInvariant();
        user.DisplayName = string.IsNullOrWhiteSpace(displayName)
            ? user.FullName
            : displayName.Trim();
        user.TimeZoneId = timeZoneId;

        user.Balance = Balance.CreateForUser(user, user.PreferredCurrency);

        return user;
    }

    private static User CreateCore(
        string email,
        string userName,
        string passwordHash,
        string firstName,
        string lastName,
        string? phoneNumber,
        UserRole role)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email is required.");

        if (string.IsNullOrWhiteSpace(userName))
            throw new DomainException("User name is required.");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash is required.");

        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("First name is required.");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Last name is required.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email.Trim().ToLowerInvariant(),
            UserName = userName.Trim(),
            PasswordHash = passwordHash,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim(),
            Role = role
        };

        user.MarkCreated();
        return user;
    }

    public void UpdateProfile(
        string firstName,
        string lastName,
        string? phoneNumber = null,
        string? displayName = null,
        string? timeZoneId = null)
    {
        EnsureNotClosed();

        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("First name is required.");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Last name is required.");

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim();
        TimeZoneId = timeZoneId;

        if (IsTrader)
        {
            DisplayName = string.IsNullOrWhiteSpace(displayName)
                ? FullName
                : displayName.Trim();
        }

        MarkUpdated();
    }

    public void ChangePassword(string newPasswordHash)
    {
        EnsureNotClosed();

        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new DomainException("Password hash is required.");

        PasswordHash = newPasswordHash;
        MarkUpdated();
    }

    public void Activate()
    {
        EnsureNotDeleted();

        if (Status == UserStatus.Closed)
            throw new DomainException("Closed users cannot be activated.");

        Status = UserStatus.Active;
        MarkUpdated();
    }

    public void Suspend()
    {
        EnsureNotDeleted();

        if (Status != UserStatus.Active)
            throw new DomainException("Only active users can be suspended.");

        Status = UserStatus.Suspended;
        MarkUpdated();
    }

    public void Close()
    {
        EnsureNotDeleted();

        Status = UserStatus.Closed;
        MarkUpdated();
    }

    public void RecordLogin()
    {
        if (!IsActive)
            throw new DomainException("Inactive users cannot sign in.");

        LastLoginAtUtc = DateTime.UtcNow;
        MarkUpdated();
    }

    public void SoftDelete()
    {
        Status = UserStatus.Closed;
        MarkDeleted();
    }

    private void EnsureNotClosed()
    {
        EnsureNotDeleted();

        if (Status == UserStatus.Closed)
            throw new DomainException("Closed users cannot be modified.");
    }

    private void EnsureNotDeleted()
    {
        if (IsDeleted)
            throw new DomainException("Deleted users cannot be modified.");
    }
}
