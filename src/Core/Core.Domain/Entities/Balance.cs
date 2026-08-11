using Core.Domain.Common;
using Core.Domain.Exceptions;
using Core.Domain.ValueObjects;

namespace Core.Domain.Entities;

public class Balance : AuditableEntity
{
    private Balance()
    {
    }

    public Guid UserId { get; private set; }
    public User? User { get; private set; }

    public string Currency { get; private set; } = string.Empty;
    public decimal Available { get; private set; }
    public decimal Reserved { get; private set; }

    public decimal Total => Available + Reserved;

    public Money AvailableMoney => new(Available, Currency);
    public Money ReservedMoney => new(Reserved, Currency);
    public Money TotalMoney => new(Total, Currency);

    internal static Balance CreateForUser(User user, string currency)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (user.Id == Guid.Empty)
            throw new DomainException("User id is required for balance.");

        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new DomainException("Balance currency must be a 3-letter ISO code.");

        var balance = new Balance
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            Currency = currency.ToUpperInvariant(),
            Available = 0m,
            Reserved = 0m
        };

        balance.MarkCreated();
        return balance;
    }

    public void Credit(decimal amount)
    {
        EnsurePositiveAmount(amount);
        Available += amount;
        MarkUpdated();
    }

    public void Debit(decimal amount)
    {
        EnsurePositiveAmount(amount);

        if (amount > Available)
            throw new DomainException("Insufficient available balance.");

        Available -= amount;
        MarkUpdated();
    }

    public void Reserve(decimal amount)
    {
        EnsurePositiveAmount(amount);

        if (amount > Available)
            throw new DomainException("Insufficient available balance to reserve.");

        Available -= amount;
        Reserved += amount;
        MarkUpdated();
    }

    public void Release(decimal amount)
    {
        EnsurePositiveAmount(amount);

        if (amount > Reserved)
            throw new DomainException("Cannot release more than the reserved amount.");

        Reserved -= amount;
        Available += amount;
        MarkUpdated();
    }

    public void CaptureReserved(decimal amount)
    {
        EnsurePositiveAmount(amount);

        if (amount > Reserved)
            throw new DomainException("Cannot capture more than the reserved amount.");

        Reserved -= amount;
        MarkUpdated();
    }

    private static void EnsurePositiveAmount(decimal amount)
    {
        if (amount <= 0)
            throw new DomainException("Amount must be greater than zero.");
    }
}
